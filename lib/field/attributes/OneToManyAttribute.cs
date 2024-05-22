namespace lib.field.attributes;

[AttributeUsage(AttributeTargets.Property)]
public class OneToManyAttribute: Attribute
{
    public string? Target { get; set; }
}
