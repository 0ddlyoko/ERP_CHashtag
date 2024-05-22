using lib.field;
using lib.model;

namespace lib.cache;

/**
 * Model cached per environment
 */
public class CachedModel
{
    public required Environment Env;
    public required FinalModel Model;
    public Dictionary<string, CachedField> Fields = new();
    public required int Id;

    public object? GetField(FinalField field) => Fields[field.FieldName].Value;

    /**
     * Update given field to given value
     */
    public bool UpdateField(FinalField field, object? newValue)
    {
        if (field.FieldName is "Id" or "CreationDate")
            return false;
        return Fields[field.FieldName].UpdateField(newValue);
    }
}
