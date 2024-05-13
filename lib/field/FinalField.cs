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
    public PluginField LastOccurence;
    public readonly List<PluginField> AllOccurences = [];
    public string Name;
    public string Description;
    public ComputedValue? DefaultComputedMethod;
    public List<FinalField> InverseCompute = [];

    public FinalField(PluginField firstOccurence)
    {
        FieldName = firstOccurence.FieldName;
        FieldType = firstOccurence.FieldType;
        FirstOccurence = firstOccurence;
        LastOccurence = firstOccurence;
        AllOccurences.Add(firstOccurence);
        Name = firstOccurence.Name ?? FieldName;
        Description = firstOccurence.Description ?? Name;
        DefaultComputedMethod = firstOccurence.DefaultComputedMethod;
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
        if (pluginField.DefaultComputedMethod != null)
            DefaultComputedMethod = pluginField.DefaultComputedMethod;
        LastOccurence = pluginField;
    }

    public object? GetDefaultValue()
    {
        if (DefaultComputedMethod?.DefaultValueAttribute == null)
            return null;
        // Target is a fixed value
        if (!DefaultComputedMethod.DefaultValueAttribute.IsMethod)
            return DefaultComputedMethod.DefaultValue;
        // Target is a computed field
        if (DefaultComputedMethod.ComputedAttribute != null)
            return null;
        // Target is not static
        if (!DefaultComputedMethod.IsComputedStatic)
            return null;
        // Target method does not exist
        if (DefaultComputedMethod.MethodInfo == null)
            throw new InvalidOperationException($"Computed method {DefaultComputedMethod.DefaultValue} does not exist for field {DefaultComputedMethod.FieldName}");
        // Computed values are called later
        var defaultValue = DefaultComputedMethod.MethodInfo.Invoke(null, null);
        if (FieldType == FieldType.Date && defaultValue != null && defaultValue?.GetType() == typeof(DateTime))
        {
            defaultValue = ((DateTime)defaultValue).Date;
        }
        return defaultValue;
    }
}
