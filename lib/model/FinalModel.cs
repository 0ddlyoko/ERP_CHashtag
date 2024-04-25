using System.Reflection;
using lib.field;

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

    public Dictionary<string, object> GetDefaultValues()
    {
        // TODO Add default values
        var dict = new Dictionary<string, object>();
        foreach ((string fieldName, FinalField field) in Fields)
        {
            if (field.DefaultValue != null)
                dict[fieldName] = field.DefaultValue;
        }
        return dict;
    }
}
