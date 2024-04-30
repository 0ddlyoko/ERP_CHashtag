namespace lib.field.attributes;

[AttributeUsage(AttributeTargets.Method)]
public class ComputedAttribute(string[] fields): Attribute
{
    public string[] Fields { get; } = fields;
}
