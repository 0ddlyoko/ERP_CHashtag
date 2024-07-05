namespace lib.database;

/**
 * Parameters to pass to the database query
 */
public class DatabaseParameters
{
    public List<SingleParameter> Parameters = [];
}

public class SingleParameter
{
    public string? Key;
    public string? Value;

    public SingleParameter(string? value)
    {
        Value = value;
    }

    public SingleParameter(string key, string? value)
    {
        Key = key;
        Value = value;
    }
}
