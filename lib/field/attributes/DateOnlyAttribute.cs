namespace lib.field.attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class DateOnlyAttribute: Attribute
{
    // If true, field is considered as a Date. If false, field is considered as a Date & Time
    public bool DateOnly = true;
}
