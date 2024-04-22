namespace lib.model;

/**
 * Model cached per environment
 */
public class CachedModel
{
    public int Id = 0;
    public string Model = "";
    public bool Dirty = false;
    public Dictionary<string, object> Data = new();
}
