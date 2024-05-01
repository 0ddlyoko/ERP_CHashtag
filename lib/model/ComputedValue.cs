using System.Reflection;
using lib.field.attributes;

namespace lib.model;

/**
 * Class representing a default value for a field or a method that will compute a value of a field
 */
public class ComputedValue
{
    public readonly string FieldName;
    public readonly DefaultValueAttribute? DefaultValueAttribute;
    public object? DefaultValue => DefaultValueAttribute?.DefaultValue;
    public readonly ComputedAttribute? ComputedAttribute;
    public readonly MethodInfo? MethodInfo;
    public bool IsComputedStatic => MethodInfo?.IsStatic ?? false;
    public bool IsPresent => DefaultValueAttribute != null;

    public ComputedValue(string fieldName, FieldInfo fieldInfo, Type classType)
    {
        FieldName = fieldName;
        DefaultValueAttribute = fieldInfo.GetCustomAttribute<DefaultValueAttribute>();
        if (DefaultValueAttribute is not { IsMethod: true })
            return;
        if (DefaultValueAttribute.DefaultValue is not string)
            throw new InvalidOperationException($"Default value {DefaultValueAttribute.DefaultValue} of field {fieldName} should be a string!");
        // Computed field
        MethodInfo = classType.GetMethod((DefaultValueAttribute.DefaultValue as string)!);
        if (MethodInfo == null)
            throw new InvalidOperationException($"Default method {DefaultValueAttribute.DefaultValue} not found in class!");
        ComputedAttribute = MethodInfo.GetCustomAttribute<ComputedAttribute>();
    }
}
