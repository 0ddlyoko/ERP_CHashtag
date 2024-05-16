namespace lib.field.attributes;


[AttributeUsage(AttributeTargets.Property)]
public class FieldDefinitionAttribute: Attribute
{
    // If empty, take the name of the field
    public string? Name { get; set; }
    // If empty, take the name of the field
    public string? Description { get; set; }
    // TODO Put Required field in a custom attribute
    // Mark this field as required in the database
    // public bool? Required { get; set; }
}
