using Npgsql;

namespace lib.database;

/**
 * Represent a single connection to a Postgres database
 */
public class DatabaseConnectionConfig
{
    
    public required string Host;
    public required string DbName;
    public required string User;
    public required string Password;
    private string Conn => $"Host={Host};Username={User};Password={Password};Database={DbName}";

    public NpgsqlDataSource Database;

    public DatabaseConnectionConfig(string host, string dbName, string user, string password)
    {
        Host = host;
        DbName = dbName;
        User = user;
        Password = password;

        var datasourceBuilder = new NpgsqlDataSourceBuilder(Conn);
        Database = datasourceBuilder.Build();
    }

    /**
     * Open a new connection to the database.
     * Don't forget to close it otherwise the connection will stay open, and the application could crash
     */
    public DatabaseConnection Open(Environment env) => new() 
    { 
        Env = env,
        Connection = Database.OpenConnection(),
    };
}
