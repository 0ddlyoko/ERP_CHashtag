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
    public readonly string? Name;
    public readonly string? Description;
    public readonly ComputedValue? DefaultComputedMethod;
    public readonly FieldType FieldType;

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
            _ => throw new InvalidEnumArgumentException($"Argument type {propertyInfo.PropertyType} is invalid!")
        };
        // Date / Datetime
        if (FieldType != FieldType.Datetime)
            return;
        var dateOnlyAttribute = propertyInfo.GetCustomAttribute<DateOnlyAttribute>();
        if (dateOnlyAttribute == null)
            return;
        if (dateOnlyAttribute.DateOnly)
        {
            FieldType = FieldType.Date;
        }
    }
}
