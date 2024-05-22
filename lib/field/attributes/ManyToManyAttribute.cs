namespace lib.field.attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ManyToManyAttribute: Attribute
{
    public string? Target { get; set; }
    public string? OriginColumnName { get; set; }
    public string? TargetColumnName { get; set; }
}
