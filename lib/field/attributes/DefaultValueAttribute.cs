namespace lib.field.attributes;

[AttributeUsage(AttributeTargets.Property)]
public class DefaultValueAttribute(object defaultValue, bool isMethod = false) : Attribute
{
    public bool IsMethod { get; } = isMethod;
    public object? DefaultValue { get; } = defaultValue;
}
