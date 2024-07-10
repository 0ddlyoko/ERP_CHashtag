namespace lib.database;

public class Query
{
    private DatabaseConnection _connection;
    private Environment _env => _connection.Env;
    private string[] _select;
    private string _from;
    private string _where;

    public static Query Select(string[] fields, DatabaseConnection connection)
    {
        return new()
        {
            _select = fields
        };
    }

    public void From(string from)
    {
        _from = from;
    }

    public void Where(string where)
    {
        _where = where;
    }

    public void Where(List<object> domain)
    {
        
    }
    
    /**
     * Transform the domain into a WHERE clause with corresponding LEFT JOIN & arguments, in order:
     * (left join, domain, arguments)
     * [('name', '=', "Test")]
     * [('name', '=', "Test"), ('age', '>=', 18)]
     * [('partner_id.name', '=', 'Test'), ('age', '>=', 18)]
     * [('partner_id.name', '=', 'Test'), ('partner_id.age', '>=', 18)]
     */
    public (String, List<string>, List<string>) DomainToQuery()
    {
        string where = "";
        List<String> leftJoins = [];
        List<string> arguments = [];
        
        return (where, leftJoins, arguments);
    }
}
