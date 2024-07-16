using lib.model;

namespace lib.field;

/**
 * Final Field, representing the concatenation of a specific field implemented in multiple plugins
 */
public class FinalField
{
    public readonly FinalModel FinalModel;
    public readonly string FieldName;
    public readonly FieldType FieldType;
    public readonly PluginField FirstOccurence;
    public PluginField? LastOccurenceOfComputedMethod;
    public PluginField LastOccurence;
    public readonly List<PluginField> AllOccurences = [];
    public string Name;
    public string Description;
    public ComputedValue? DefaultComputedMethod;
    public bool IsComputed => DefaultComputedMethod?.IsComputed ?? false;

    /**
     * Contains the inverse compute fields that depends on this FinalField.<br/>
     * string could be a direct or a dotted link to a computed field<br/>
     * For dotted links, each element can be a field name, or if there is no double link between both models, the target model followed by a plus (+) followed by the field name of the target.<br/>
     * If direct link is possible, compute will not directly be performed, but if there is no direct link in one node (aka the + symbol),
     * we need to compute it to have the new correct value.
     */
    public readonly TreeDependency TreeDependency;
    public Type TargetType => FirstOccurence.Type;

    public FinalModel? TargetFinalModel
    {
        get
        {
            try
            {
                return FinalModel.PluginManager.GetFinalModel(FinalModel.PluginManager
                    .GetPluginModelFromType(TargetType).Name);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    public FinalField? TargetFinalField => TargetField == null ? null : TargetFinalModel?.Fields[TargetField];
    public string? TargetField;
    public string? OriginColumnName;
    public string? TargetColumnName;
    public readonly SelectionField? Selection;

    public FinalField(FinalModel finalModel, PluginField firstOccurence)
    {
        FinalModel = finalModel;
        FieldName = firstOccurence.FieldName;
        FieldType = firstOccurence.FieldType;
        FirstOccurence = firstOccurence;
        LastOccurence = firstOccurence;
        AllOccurences.Add(firstOccurence);
        Name = firstOccurence.Name ?? FieldName;
        Description = firstOccurence.Description ?? Name;
        DefaultComputedMethod = firstOccurence.DefaultComputedMethod;
        if (DefaultComputedMethod?.MethodInfo != null)
            LastOccurenceOfComputedMethod = firstOccurence;
        TargetField = firstOccurence.TargetField;
        OriginColumnName = firstOccurence.OriginColumnName;
        TargetColumnName = firstOccurence.TargetColumnName;
        Selection = firstOccurence.Selection;
        
        TreeDependency = new TreeDependency(root: this);
    }

    public void MergeWith(PluginField pluginField)
    {
        if (pluginField.FieldName != FieldName)
            throw new InvalidOperationException(
                $"Field {pluginField} cannot be merged with this field as field name are different! (Got {pluginField.FieldName}, but {FieldName} is expected)");
        if (pluginField.FieldType != FieldType)
        {
            if (!(pluginField.FieldType is FieldType.Datetime && FieldType is FieldType.Date
                 || pluginField.FieldType is FieldType.String && FieldType is FieldType.Selection))
                throw new InvalidOperationException(
                    $"Field {pluginField} cannot be merged with this field as type are different! (Got {pluginField.FieldType}, but {FieldType} is expected)");
        }
        AllOccurences.Add(pluginField);
        if (pluginField.Name != null)
            Name = pluginField.Name;
        if (pluginField.Description != null)
            Description = pluginField.Description;
        if (pluginField.DefaultComputedMethod != null)
        {
            DefaultComputedMethod = pluginField.DefaultComputedMethod;
            if (DefaultComputedMethod?.MethodInfo != null)
                LastOccurenceOfComputedMethod = pluginField;
        }
        LastOccurence = pluginField;
        if (TargetType != pluginField.Type)
            throw new InvalidOperationException($"Field {FieldName} in model {FinalModel.Name} has changed type from {TargetType} to {pluginField.Type}!");
        if (pluginField.TargetField != null)
            TargetField = pluginField.TargetField;
        if (pluginField.OriginColumnName != null)
            OriginColumnName = pluginField.OriginColumnName;
        if (pluginField.TargetColumnName != null)
            TargetColumnName = pluginField.TargetColumnName;
        if (pluginField.Selection != null && Selection != null)
        {
            foreach (var (key, value) in pluginField.Selection.Selections)
            {
                Selection.Selections[key] = value;
            }
        }
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
        if (FieldType == FieldType.Date && defaultValue is DateTime time)
        {
            defaultValue = time.Date;
        }
        return defaultValue;
    }

    public override string ToString() => $"FinalField[Model={FinalModel.Name}, Field={FieldName}, Name={Name}]";
}
