namespace lib.field.attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class SelectionAttribute(string key, string value): Attribute
{
    public readonly string Key = key;
    public readonly string Value = value;
}
