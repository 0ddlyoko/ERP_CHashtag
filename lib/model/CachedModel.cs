using System.Reflection;
using lib.field;
using lib.plugin;

namespace lib.model;

/**
 * Model cached per environment
 */
public class CachedModel
{
    public required Environment Env;
    public required int Id;
    public required FinalModel Model;
    public bool Dirty = false;
    public Dictionary<string, CachedField> Fields = new();

    /**
     * Update the cache based on given data
     */
    public void UpdateCacheFromData(IReadOnlyDictionary<string, object?> data)
    {
        bool hasBeenUpdated = false;
        foreach ((string fieldName, object? newValue) in data)
        {
            // Do not allow modification on those fields
            if (fieldName is "Id" or "CreationDate" or "UpdateDate")
                continue;
            if (!Fields.TryGetValue(fieldName, out CachedField? cachedField))
                throw new KeyNotFoundException($"Cannot find field {fieldName} in model {Model.Name}");
            if (cachedField.ModifyField(newValue))
                hasBeenUpdated = true;
        }
        // Update "UpdateDate" field
        if (hasBeenUpdated)
            Fields["UpdateDate"].ModifyField(DateTimeProvider.Now);
    }

    /**
     * Starting from this model, retrieve the target model of given field.
     * If target model or any model between current model and target one is not loaded, load it in the cache
     */
    public CachedModel? GetCachedModelOfTargetField(string targetField)
    {
        // For now, we only support fields from the same model
        // Later, we will support link to other models
        return this;
    }
    
    public void FlagComputedValues()
    {
        foreach (var (fieldName, cachedField) in Fields)
        {
            FinalField finalField = Model.Fields[fieldName];
            if (finalField.IsComputed)
            {
                cachedField.ToRecompute = true;
                // TODO Manage stored computed field differently than non stored
                cachedField.Dirty = true;
                cachedField.CachedModel.Dirty = true;
            }
        }
    }
}
