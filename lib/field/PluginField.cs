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
    public readonly bool IsDefaultValueAMethod;
    public readonly object? DefaultValue;
    public readonly string[] Compute = [];
    public readonly FieldType FieldType;

    public PluginField(PluginModel pluginModel, FieldDefinitionAttribute definition, FieldInfo fieldInfo, Type classType)
    {
        PluginModel = pluginModel;
        Type = fieldInfo.FieldType;
        FieldName = fieldInfo.Name;
        Name = definition.Name;
        Description = definition.Description;
        // Default values
        var defaultValue = fieldInfo.GetCustomAttribute<DefaultValueAttribute>();
        HasDefaultValue = false;
        if (defaultValue != null)
        {
            HasDefaultValue = true;
            IsDefaultValueAMethod = defaultValue.IsMethod;
            DefaultValue = defaultValue.DefaultValue;
            if (IsDefaultValueAMethod)
                Compute = GetMethodCompute(classType);
        }
        
        // Field type
        FieldType = Type.GetTypeCode(fieldInfo.FieldType) switch
        {
            TypeCode.String => FieldType.String,
            TypeCode.Int32 => FieldType.Integer,
            TypeCode.Decimal => FieldType.Float,
            TypeCode.Boolean => FieldType.Boolean,
            _ => throw new InvalidEnumArgumentException($"Argument type {fieldInfo.FieldType} is invalid!")
        };
    }

    /**
     * Check if default value of current field is a method, and if it's a computed one
     */
    private string[] GetMethodCompute(Type classType)
    {
        if (DefaultValue == null)
            return [];
        // Computed field
        MethodInfo? computedMethodInfo = classType.GetMethod((DefaultValue as string)!);
        if (computedMethodInfo == null)
            throw new InvalidOperationException($"Default method {DefaultValue} not found in class!");
        
        var compute = computedMethodInfo.GetCustomAttribute<ComputedAttribute>();
        return compute?.Fields ?? [];
    }
}
