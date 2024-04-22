using lib.field;

namespace main.models;

using lib.model;

[ModelDefinition("partner", Description = "Contact")]
public class Partner: Model
{
    [FieldDefinition(Description = "Name of the partner")]
    public string Name = "";

    [FieldDefinition(Description = "Age of the partner")]
    public int Age = 0;

    // Compute
    public string DisplayName => $"Name: {Name}, Age: {Age}";
}

[ModelDefinition("partner", Description = "Contact")]
public class Partner2: Model
{
    [FieldDefinition(Description = "Not the name of the partner")]
    public string Name = "";
}

[ModelDefinition("partner", Description = "Contact")]
public class Partner3: Model
{
    [FieldDefinition(Name="Not his Age", Description = "Age of him")]
    public int Age = 0;
}
