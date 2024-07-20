using lib.field;
using Npgsql;

namespace lib.database;

public static class DatabaseHelper
{
    #region Tables
    
    public static async Task CreateTable(DatabaseConnection connection, string tableName, string? tableDescription = null)
    {
        await CreateRequest(connection, $"CREATE TABLE IF NOT EXISTS {tableName} (id INTEGER PRIMARY KEY)").ExecuteNonQueryAsync();
        await CreateRequest(connection, $"COMMENT ON TABLE {tableName} IS '{tableDescription ?? tableName}'").ExecuteNonQueryAsync();
    }

    public static async Task RenameTable(DatabaseConnection connection, string oldTableName, string newTableName,
        string? tableDescription = null)
    {
        await CreateRequest(connection, $"ALTER TABLE {oldTableName} RENAME TO {newTableName}").ExecuteNonQueryAsync();
        await CreateRequest(connection, $"COMMENT ON TABLE {newTableName} IS '{tableDescription ?? newTableName}'").ExecuteNonQueryAsync();
    }

    public static async Task DropTable(DatabaseConnection connection, string tableName)
    {
        await CreateRequest(connection, $"DROP TABLE {tableName}").ExecuteNonQueryAsync();
    }

    #endregion

    #region Columns
    
    public static async Task CreateColumn(DatabaseConnection connection, string tableName, string columnName, string columnType, string? columnDescription = null, bool? required = null, string? targetTableName = null, string? targetColumnName = null)
    {
        await CreateRequest(connection, $"ALTER TABLE {tableName} ADD COLUMN IF NOT EXISTS {columnName} {columnType}").ExecuteNonQueryAsync();
        
        await CreateRequest(connection, $"COMMENT ON COLUMN {tableName}.{columnName} IS '{columnDescription ?? columnName}'").ExecuteNonQueryAsync();

        if (required != null)
        {
            await CreateRequest(connection, $"ALTER TABLE {tableName} ALTER COLUMN {columnName} {((bool)required ? "SET" : "DROP")} NOT NULL").ExecuteNonQueryAsync();
        }

        if (targetTableName != null && targetColumnName != null)
        {
            var constraintName = $"{tableName}_{columnName}_fkey";
            await CreateRequest(connection, $"ALTER TABLE {tableName} ADD CONSTRAINT {constraintName} FOREIGN KEY ({columnName}) REFERENCES {targetTableName} ({targetColumnName})").ExecuteNonQueryAsync();
        }
    }

    public static async Task RenameColumn(DatabaseConnection connection, string tableName, string oldColumnName, string newColumnName, string? columnDescription = null)
    {
        await CreateRequest(connection, $"ALTER TABLE {tableName} RENAME COLUMN {oldColumnName} TO {newColumnName}").ExecuteNonQueryAsync();
        await CreateRequest(connection, $"COMMENT ON COLUMN {tableName}.{newColumnName} IS '{columnDescription ?? newColumnName}'").ExecuteNonQueryAsync();
    }

    public static async Task DropColumn(DatabaseConnection connection, string tableName, string columnName)
    {
        await CreateRequest(connection, $"ALTER TABLE {tableName} DROP COLUMN {columnName}").ExecuteNonQueryAsync();
    }

    #endregion

    #region CreateRequest

    public static NpgsqlCommand CreateRequest(DatabaseConnection connection, string request, List<DatabaseParameter>? parameters = null)
    {
        return parameters == null ? connection.CreateRequest(request) : connection.CreateRequest(request, parameters);
    }

    #endregion

    public static string FieldTypeToString(FieldType type) => type switch
    {
        FieldType.String => "character",
        FieldType.Selection => "character",
        FieldType.Integer => "integer",
        FieldType.Float => "double precision",
        FieldType.Boolean => "boolean",
        FieldType.Date => "timestamp WITH TIME ZONE",
        FieldType.Datetime => "timestamp WITH TIME ZONE",
        FieldType.ManyToOne => "integer",
        FieldType.OneToMany => throw new InvalidOperationException("OneToMany cannot be transformed into a valid SQL type"),
        FieldType.ManyToMany => throw new InvalidOperationException("ManyToMany cannot be transformed into a valid SQL type"),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
