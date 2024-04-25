using System.ComponentModel;
using System.Reflection;
using lib.field.attributes;
using lib.model;
using lib.plugin;
using DefaultValueAttribute = lib.field.attributes.DefaultValueAttribute;

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
    public readonly bool HasDefaultValue;
    public readonly object? DefaultValue;
    public readonly FieldType FieldType;

    public PluginField(PluginModel pluginModel, FieldDefinitionAttribute definition, FieldInfo fieldInfo)
    {
        PluginModel = pluginModel;
        Type = fieldInfo.FieldType;
        FieldName = fieldInfo.Name;
        Name = definition.Name;
        Description = definition.Description;
        
        var defaultValue = fieldInfo.GetCustomAttribute<DefaultValueAttribute>();
        
        HasDefaultValue = false;
        if (defaultValue != null)
        {
            HasDefaultValue = true;
            DefaultValue = defaultValue.DefaultValue;
        }
        FieldType = Type.GetTypeCode(fieldInfo.FieldType) switch
        {
            TypeCode.String => FieldType.String,
            TypeCode.Int32 => FieldType.Integer,
            TypeCode.Decimal => FieldType.Float,
            TypeCode.Boolean => FieldType.Boolean,
            _ => throw new InvalidEnumArgumentException($"Argument type {fieldInfo.FieldType} is invalid!")
        };
    }
}
