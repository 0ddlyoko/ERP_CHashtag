using lib.field.attributes;
using lib.model;

namespace lib.@base.models;

[ModelDefinition("partner", Description = "Contact")]
public class Partner: Model
{
    [FieldDefinition(Description = "Name of the partner")]
    [DefaultValue("Test")]
    public string Name { get => Get<string>("Name"); set => Set("Name", value); }

    [FieldDefinition(Description = "Age of the partner")]
    [DefaultValue(42)]
    public int Age  { get => Get<int>("Age"); set => Set("Age", value); }

    [FieldDefinition]
    [DefaultValue(nameof(DefaultRandomColor), isMethod: true)]
    public int Color { get => Get<int>("Color"); set => Set("Color", value); }

    // Compute
    [FieldDefinition(Description = "Name to display of the partner")]
    [DefaultValue(nameof(ComputeDisplayName), isMethod: true)]
    public string DisplayName { get => Get<string>("DisplayName"); set => Set("DisplayName", value); }

    [FieldDefinition(Description = "Random Date")]
    [DefaultValue(nameof(DefaultDateTime), isMethod: true)]
    public DateTime Date { get => Get<DateTime>("Date"); set => Set("Date", value); }
    
    // Default method
    public static int DefaultRandomColor() => 42;

    public static DateTime DefaultDateTime() => DateTimeProvider.Now;

    // Compute method
    [Computed(["Name", "Age"])]
    public void ComputeDisplayName()
    {
        DisplayName = $"Name: {Name}, Age: {Age}";
    }
}

[ModelDefinition("partner", Description = "Contact")]
public class Partner2: Model
{
    [FieldDefinition(Description = "Not the name of the partner")]
    [DefaultValue("LoL")]
    public string Name { get => Get<string>("Name"); set => Set("Name", value); }

    [FieldDefinition]
    public int Test { get => Get<int>("Test"); set => Set("Test", value); }
}

[ModelDefinition("partner", Description = "Contact")]
public class Partner3: Model
{
    [FieldDefinition(Name="Not his Age", Description = "Age of him")]
    [DefaultValue(nameof(DefaultAge), isMethod: true)]
    public int Age { get => Get<int>("Age"); set => Set("Age", value); }

    [FieldDefinition]
    [DefaultValue(30)]
    public int Test { get => Get<int>("Test"); set => Set("Test", value); }
    
    public static int DefaultAge() => 70;
}
