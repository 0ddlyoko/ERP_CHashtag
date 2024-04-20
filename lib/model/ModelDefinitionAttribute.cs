namespace lib.model;

[AttributeUsage(AttributeTargets.Class)]
public class ModelDefinitionAttribute(string name) : Attribute
{
    public string Name { get; private set; } = name.ToLower();
    public string? Description { get; set; }
}
