using lib.field;
using lib.util;

namespace lib.model;

/**
 * Final Model, representing the concatenation of a specific model implemented in multiple plugins
 */
public class FinalModel
{
    public readonly string Name;
    public readonly PluginModel FirstOccurence;
    public string Description;
    public readonly List<PluginModel> AllOccurences = [];
    public readonly Dictionary<string, FinalField> Fields;

    public FinalModel(PluginModel firstOccurence)
    {
        Name = firstOccurence.Name;
        FirstOccurence = firstOccurence;
        Description = firstOccurence.Description ?? Name;
        AllOccurences.Add(firstOccurence);
        Fields = new Dictionary<string, FinalField>();
        AddFields(firstOccurence.Fields);
    }

    public void MergeWith(PluginModel pluginModel)
    {
        AllOccurences.Add(pluginModel);
        if (pluginModel.Description != null)
            Description = pluginModel.Description;
        AddFields(pluginModel.Fields);
    }

    private void AddFields(Dictionary<string, PluginField> fields)
    {
        foreach (var (id, field) in fields)
        {
            if (Fields.TryGetValue(id, out var finalField))
                finalField.MergeWith(field);
            else
                Fields[id] = new FinalField(field);
        }
    }

    /**
     * Execute some action once this class is fully loaded, and no more PluginModel will be merged with this class
     */
    public void PostLoading()
    {
        // Compute
        foreach (var (fieldName, field) in Fields)
        {
            if (field.DefaultComputedMethod?.ComputedAttribute == null)
                continue;
            foreach (var compute in field.DefaultComputedMethod.ComputedAttribute.Fields)
            {
                if (!Fields.TryGetValue(compute, out var targetField))
                    throw new InvalidOperationException($"Field {compute} used as a compute of {fieldName} does not exist for model {Name}");
                targetField.InverseCompute.Add(fieldName);
            }
        }
    }

    public Dictionary<string, object> GetDefaultValues(int id)
    {
        var dict = new Dictionary<string, object>
        {
            ["Id"] = id
        };
        
        // Default values
        foreach (var (fieldName, finalField) in Fields)
        {
            object? defaultValue = finalField.GetDefaultValue();
            if (defaultValue != null)
                dict[fieldName] = defaultValue;
        }
        
        return dict;
    }
    
    public void FlagComputedValues(CachedModel cachedModel)
    {
        foreach (var (fieldName, finalField) in Fields)
        {
            if (finalField.IsComputed)
            {
                CachedField field = cachedModel.Fields[fieldName];
                field.ToRecompute = true;
                // TODO Manage stored computed field differently than non stored
                field.Dirty = true;
                field.CachedModel.Dirty = true;
            }
        }
    }
}
