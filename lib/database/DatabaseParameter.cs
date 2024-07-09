namespace lib.database;

/**
 * Parameters to pass to the database query
 */
public class DatabaseParameter
{
    public readonly string? Key;
    public readonly string? Value;

    public DatabaseParameter(string? value)
    {
        Value = value;
    }

    public DatabaseParameter(string key, string? value)
    {
        Key = key;
        Value = value;
    }
}
