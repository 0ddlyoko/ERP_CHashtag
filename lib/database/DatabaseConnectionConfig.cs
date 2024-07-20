using Npgsql;

namespace lib.database;

/**
 * Represent a single connection to a Postgres database
 */
public class DatabaseConnectionConfig
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _dbName;
    private readonly string _user;
    private readonly string _password;
    private string Conn => $"Host={_host};Port={_port};Username={_user};Password={_password};Database={_dbName}";

    private readonly NpgsqlDataSource _database;

    public DatabaseConnectionConfig(string host, int port, string dbName, string user, string password)
    {
        _host = host;
        _port = port;
        _dbName = dbName;
        _user = user;
        _password = password;

        var datasourceBuilder = new NpgsqlDataSourceBuilder(Conn);
        _database = datasourceBuilder.Build();
    }

    /**
     * Open a new connection to the database.
     * Don't forget to close it otherwise the connection will stay open, and the application could crash
     */
    public DatabaseConnection Open(Environment env) => new() 
    { 
        Env = env,
        Connection = _database.OpenConnection(),
    };
}
