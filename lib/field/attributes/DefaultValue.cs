namespace lib.field.attributes;

[AttributeUsage(AttributeTargets.Field)]
public class DefaultValueAttribute(object defaultValue) : Attribute
{
    public object? DefaultValue { get; } = defaultValue;
}
