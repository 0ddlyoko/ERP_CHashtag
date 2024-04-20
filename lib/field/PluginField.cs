using System.ComponentModel;
using System.Reflection;
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
    private readonly Type _type;
    public readonly string FieldName;
    public readonly string? Name;
    public readonly string? Description;
    public readonly FieldType FieldType;

    public PluginField(PluginModel pluginModel, FieldDefinitionAttribute definition, FieldInfo fieldInfo)
    {
        PluginModel = pluginModel;
        _type = fieldInfo.FieldType;
        FieldName = fieldInfo.Name;
        Name = definition.Name;
        Description = definition.Description;
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
