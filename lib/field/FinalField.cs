using System.Reflection;
using lib.model;

namespace lib.field;

/**
 * Final Field, representing the concatenation of a specific field implemented in multiple plugins
 */
public class FinalField
{
    public readonly string FieldName;
    public readonly FieldType FieldType;
    public readonly PluginField FirstOccurence;
    public readonly List<PluginField> AllOccurences = [];
    public string Name;
    public string Description;
    public PluginModel? DefaultValuePluginModel;
    public bool IsDefaultValueAMethod;
    public object? DefaultValue;

    public FinalField(PluginField firstOccurence)
    {
        FieldName = firstOccurence.FieldName;
        FieldType = firstOccurence.FieldType;
        FirstOccurence = firstOccurence;
        AllOccurences.Add(firstOccurence);
        Name = firstOccurence.Name ?? FieldName;
        Description = firstOccurence.Description ?? Name;
        if (firstOccurence.HasDefaultValue)
        {
            DefaultValuePluginModel = firstOccurence.PluginModel;
            IsDefaultValueAMethod = firstOccurence.IsDefaultValueAMethod;
            DefaultValue = firstOccurence.DefaultValue;
        }
    }

    public void MergeWith(PluginField pluginField)
    {
        if (pluginField.FieldName != FieldName)
            throw new InvalidOperationException(
                $"Field {pluginField} cannot be merged with this field as field name are different! (Got {pluginField.FieldName}, but {FieldName} is expected)");
        if (pluginField.FieldType != FieldType)
            throw new InvalidOperationException(
                $"Field {pluginField} cannot be merged with this field as type are different! (Got {pluginField.FieldType}, but {FieldType} is expected)");
        AllOccurences.Add(pluginField);
        if (pluginField.Name != null)
            Name = pluginField.Name;
        if (pluginField.Description != null)
            Description = pluginField.Description;
        if (pluginField.HasDefaultValue)
        {
            DefaultValuePluginModel = pluginField.PluginModel;
            IsDefaultValueAMethod = pluginField.IsDefaultValueAMethod;
            DefaultValue = pluginField.DefaultValue;
        }
    }

    public object? GetDefaultValue()
    {
        if (DefaultValue == null)
            return null;
        if (!IsDefaultValueAMethod)
            return DefaultValue;
        if (DefaultValuePluginModel == null)
            return null;
        if (DefaultValue is not string defaultValue)
            throw new InvalidOperationException($"Default value ({DefaultValue}) should be a string!");
        MethodInfo? methodInfo = DefaultValuePluginModel.Type.GetMethod(defaultValue);
        if (methodInfo == null)
            throw new InvalidOperationException($"Default method not found: {DefaultValue}");
        return methodInfo.Invoke(null, null);
    }
}
