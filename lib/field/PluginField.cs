using System.ComponentModel;
using System.Reflection;
using lib.field.attributes;
using lib.model;
using lib.plugin;

namespace lib.field;

/**
 * Field defined in a model
 */
public class PluginField
{
    public readonly PluginModel PluginModel;
    public APlugin Plugin => PluginModel.Plugin;
    public readonly Type Type;
    public readonly string FieldName;
    public readonly FieldType FieldType;
    public readonly string? Name;
    public readonly string? Description;
    public readonly ComputedValue? DefaultComputedMethod;
    public readonly string? TargetField;
    public readonly string? OriginColumnName;
    public readonly string? TargetColumnName;
    public readonly SelectionField? Selection;

    public PluginField(PluginModel pluginModel, FieldDefinitionAttribute definition, PropertyInfo propertyInfo, Type classType)
    {
        PluginModel = pluginModel;
        Type = propertyInfo.PropertyType;
        FieldName = propertyInfo.Name;
        Name = definition.Name;
        Description = definition.Description;
        // Default values
        var defaultComputedMethod = new ComputedValue(FieldName, propertyInfo, classType);
        if (defaultComputedMethod.DefaultValueAttribute != null)
            DefaultComputedMethod = defaultComputedMethod;
        // Field type
        FieldType = Type.GetTypeCode(propertyInfo.PropertyType) switch
        {
            TypeCode.String => FieldType.String,
            TypeCode.Int32 => FieldType.Integer,
            TypeCode.Decimal => FieldType.Float,
            TypeCode.Boolean => FieldType.Boolean,
            TypeCode.DateTime => FieldType.Datetime,
            TypeCode.Object => FieldType.ManyToOne,
            _ => throw new InvalidEnumArgumentException($"Argument type {propertyInfo.PropertyType} is invalid!")
        };
        if (FieldType == FieldType.Datetime)
        {
            // Date / Datetime
            var dateOnlyAttribute = propertyInfo.GetCustomAttribute<DateOnlyAttribute>();
            if (dateOnlyAttribute?.DateOnly ?? false)
            {
                FieldType = FieldType.Date;
            }
        }
        else if (FieldType == FieldType.ManyToOne) 
        {
            // ManyToOne / OneToMany / ManyToMany
            var oneToManyAttribute = propertyInfo.GetCustomAttribute<OneToManyAttribute>();
            if (oneToManyAttribute != null)
            {
                FieldType = FieldType.OneToMany;
                TargetField = oneToManyAttribute.Target;
            }

            var manyToManyAttribute = propertyInfo.GetCustomAttribute<ManyToManyAttribute>();
            if (manyToManyAttribute != null)
            {
                FieldType = FieldType.ManyToMany;
                TargetField = manyToManyAttribute.Target;
                OriginColumnName = manyToManyAttribute.OriginColumnName;
                TargetColumnName = manyToManyAttribute.TargetColumnName;
            }
        }
        // Selection
        var selectionAttributes = propertyInfo.GetCustomAttributes<SelectionAttribute>();
        var selections = selectionAttributes.ToList();
        if (selections.Any())
        {
            FieldType = FieldType.Selection;
            Selection = new SelectionField();
            foreach (var selection in selections)
            {
                Selection.Selections[selection.Key] = selection.Value;
            }
        }
    }
}
