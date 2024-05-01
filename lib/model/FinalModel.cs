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
                targetField.InverseCompute.Add(field);
            }
        }
    }

    public Dictionary<string, object> GetDefaultValues(int id)
    {
        var dict = new Dictionary<string, object>
        {
            ["id"] = id
        };
        
        // Default values
        foreach (var (fieldName, finalField) in Fields)
        {
            object? defaultValue = finalField.GetDefaultValue();
            if (defaultValue != null)
                dict[fieldName] = defaultValue;
        }
        
        // // Computed values
        // foreach (var (fieldName, finalField) in Fields)
        // {
        //     object? defaultValue = finalField.GetDefaultValue();
        //     if (defaultValue != null)
        //         dict[fieldName] = defaultValue;
        // }
        
        return dict;
    }
    
    public void FillComputedValues(CachedModel cachedModel)
    {
        HashSet<string> fieldsToCompute = [];
        foreach (var (fieldName, finalField) in Fields)
        {
            if (cachedModel.Data.ContainsKey(fieldName))
                continue;
            if (finalField.DefaultComputedMethod?.ComputedAttribute == null || finalField.DefaultComputedMethod.IsComputedStatic)
                continue;
            // Create a new method instance to compute the data
            fieldsToCompute.Add(fieldName);
        }
        ComputeValues(cachedModel, fieldsToCompute);
    }

    public void ComputeValues(CachedModel cachedModel, HashSet<string> fieldsToCompute)
    {
        var dependencies = fieldsToCompute.ToDictionary(f => f, f => Fields[f].DefaultComputedMethod?.ComputedAttribute?.Fields ?? []);
        List<string> orderedFieldsToCompute = DependencyGraph.GetOrderedGraph(dependencies);
        foreach (var fieldName in orderedFieldsToCompute)
        {
            var finalField = Fields[fieldName];
            object? objInstance = Activator.CreateInstance(finalField.LastOccurence.PluginModel.Type);
            if (objInstance is not Model instance)
                throw new InvalidOperationException($"Created instance of type {finalField.LastOccurence.Type} is not a Model! This should not occur");
            instance.Id = cachedModel.Id;
            instance.Env = cachedModel.Env;
            instance.Reset();
            finalField.DefaultComputedMethod?.MethodInfo?.Invoke(instance, null);
            // Save result
            instance.Save();
        }
    }
}
