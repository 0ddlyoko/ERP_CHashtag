using Npgsql;

namespace lib.database;

public static class DatabaseHelper
{
    #region Tables
    
    public static async Task<int> CreateTable(DatabaseConnection connection, string tableName, string? tableDescription = null)
    {
        var cmd = CreateRequest(connection, "CREATE TABLE IF NOT EXISTS @tableName (id INTEGER PRIMARY KEY); COMMENT ON TABLE @tableName IS @comment", [
            new DatabaseParameter("tableName", tableName),
            new DatabaseParameter("comment", tableDescription ?? tableName),
        ]);
        return await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<int> RenameTable(DatabaseConnection connection, string oldTableName, string newTableName,
        string? tableDescription = null)
    {
        var cmd = CreateRequest(connection, "ALTER TABLE @oldTableName RENAME TO @newTableName; COMMENT ON TABLE @tableName IS @comment", [
            new DatabaseParameter("oldTableName", oldTableName),
            new DatabaseParameter("newTableName", newTableName),
            new DatabaseParameter("comment", tableDescription ?? newTableName),
        ]);
        return await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<int> DropTable(DatabaseConnection connection, string tableName)
    {
        var cmd = CreateRequest(connection, "DROP TABLE @tableName", [
            new DatabaseParameter("tableName", tableName),
        ]);
        return await cmd.ExecuteNonQueryAsync();
    }

    #endregion

    #region Columns
    
    public static async Task<int> CreateColumn(DatabaseConnection connection, string tableName, string columnName, string columnType, string? columnDescription = null, bool required = false, string? defaultValue = null, bool unique = false, List<string>? references = null)
    {
        var request = "ALTER TABLE @tableName ADD COLUMN IF NOT EXISTS @columnName @columnType";
        List<DatabaseParameter> parameters =
        [
            new DatabaseParameter("tableName", tableName),
            new DatabaseParameter("columnName", columnName),
            new DatabaseParameter("columnType", columnType),
            new DatabaseParameter("comment", columnDescription ?? columnName),
        ];
        // TODO Check for other type
        if (columnType == "timestamp")
            request += " WITH TIME ZONE";
        if (required)
            request += " NOT NULL";
        if (defaultValue != null)
            request += $" DEFAULT {defaultValue}";
        if (unique)
            request += $" UNIQUE idx_uniq_{tableName}_{columnName}";
        foreach (var reference in references ?? [])
            request += $" REFERENCE {reference}";


        request += "; COMMENT ON COLUMN @tableName.@columnName IS @comment";
        
        var cmd = CreateRequest(connection, request, parameters);
        return await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<int> RenameColumn(DatabaseConnection connection, string tableName, string oldColumnName, string newColumnName, string? columnDescription = null)
    {
        var cmd = CreateRequest(connection, "ALTER TABLE @tableName RENAME COLUMN @oldColumnName TO @newColumnName; COMMENT ON COLUMN @tableName.@newColumnName IS @comment", [
            new DatabaseParameter("tableName", tableName),
            new DatabaseParameter("oldColumnName", oldColumnName),
            new DatabaseParameter("newColumnName", newColumnName),
            new DatabaseParameter("comment", columnDescription ?? newColumnName),
        ]);
        return await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<int> AlterColumn(DatabaseConnection connection, string tableName, string columnName,
        bool? required = null, bool dropDefault = false, string? defaultValue = null, string? columnDescription = null,
        List<string>? constraintsToAdd = null, List<string>? constraintsToRemove = null)
    {
        var request = "ALTER TABLE @tableName";
        List<DatabaseParameter> parameters =
        [
            new DatabaseParameter("tableName", tableName),
            new DatabaseParameter("columnName", columnName),
        ];
        List<string> alters = [];
        if (required == true)
            alters.Add("SET NOT NULL");
        if (required == false)
            alters.Add("DROP NOT NULL");
        if (defaultValue != null)
            alters.Add($"DEFAULT {defaultValue}");
        if (dropDefault)
            alters.Add("DROP DEFAULT");

        foreach (var alter in alters)
        {
            request += $" ALTER COLUMN @columnName {alter},";
        }

        if (request.Last() == ',')
            request = request.Remove(request.Length - 1, 1);

        if (columnDescription != null)
        {
            request += "; COMMENT ON COLUMN @tableName.@columnName IS @comment";
            parameters.Add(new DatabaseParameter("comment", columnDescription));
        }

        if (constraintsToRemove != null)
        {
            foreach (var constraintToRemove in constraintsToRemove)
            {
                request += $"; ALTER TABLE @tableName DROP CONSTRAINT {constraintToRemove}";
            }
        }

        if (constraintsToAdd != null)
        {
            foreach (var constraintToAdd in constraintsToAdd)
            {
                request += $"; ALTER TABLE @tableName ADD CONSTRAINT {constraintToAdd}";
            }
        }
        var cmd = CreateRequest(connection, request, parameters);
        return await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<int> DropColumn(DatabaseConnection connection, string tableName, string columnName)
    {
        var cmd = CreateRequest(connection, "ALTER TABLE @tableName DROP COLUMN @columnName", [
            new DatabaseParameter("tableName", tableName),
            new DatabaseParameter("columnName", columnName),
        ]);
        return await cmd.ExecuteNonQueryAsync();
    }

    #endregion

    #region CreateRequest

    public static NpgsqlCommand CreateRequest(DatabaseConnection connection, string request, List<DatabaseParameter>? parameters = null)
    {
        return parameters == null ? connection.CreateRequest(request) : connection.CreateRequest(request, parameters);
    }

    #endregion
}
