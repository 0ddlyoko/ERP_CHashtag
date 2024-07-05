using Npgsql;

namespace lib.database;

public class DatabaseConnection
{
    public required NpgsqlConnection Connection;
    public bool IsClosed = false;
    public int NumberOfRequests = 0;

    /**
     * Close existing connection.
     */
    public void Close()
    {
        if (IsClosed)
            return;
        IsClosed = true;
        Connection.Close();
    }

    public NpgsqlCommand CreateRequest(string request, DatabaseParameters? parameters = null)
    {
        NumberOfRequests++;
        return _makeNpgsqlCommand(request, parameters);
    }

    private NpgsqlCommand _makeNpgsqlCommand(string request, DatabaseParameters? parameters = null)
    {
        var npgsqlCommand = new NpgsqlCommand(request);
        if (parameters != null)
        {
            foreach (var parameter in parameters.Parameters)
            {
                if (parameter.Key != null)
                    npgsqlCommand.Parameters.Add(new(parameter.Key, parameter.Value));
                else
                    npgsqlCommand.Parameters.Add(new() { Value = parameter.Value });
            }
        }
        return npgsqlCommand;
    }
}
