using lib;
using lib.field.attributes;

namespace Test.data.models;

using lib.model;

[ModelDefinition("test_partner", Description = "Contact")]
public class TestPartner: Model
{
    [FieldDefinition(Description = "Name of the partner")]
    [DefaultValue("Test")]
    public string? Name { get => Get<string>("Name"); set => Set("Name", value); }

    [FieldDefinition(Name = "Age 2", Description = "Age of the partner")]
    [DefaultValue(42)]
    public int Age { get => Get<int>("Age"); set => Set("Age", value); }

    [FieldDefinition]
    [DefaultValue(nameof(DefaultRandomColor), isMethod: true)]
    public int Color { get => Get<int>("Age"); set => Set("Age", value); }

    // Compute
    [FieldDefinition(Description = "Name to display of the partner")]
    [DefaultValue(nameof(ComputeDisplayName), isMethod: true)]
    public string DisplayName { get => Get<string>("DisplayName"); set => Set("DisplayName", value); }

    [FieldDefinition(Name = "MyDate", Description = "My Date")]
    [DateOnly]
    [DefaultValue(nameof(DefaultMyDate), isMethod: true)]
    public DateTime MyDate { get => Get<DateTime>("MyDate"); set => Set("MyDate", value); }

    [FieldDefinition(Name = "MyTime", Description = "My Date Time")]
    public DateTime MyDateTime { get => Get<DateTime>("MyDateTime"); set => Set("MyDateTime", value); }
    
    // Default method
    public static int DefaultRandomColor() => 42;

    public static DateTime DefaultMyDate() => DateTimeProvider.Now;

    // Compute method
    [Computed(["Name", "Age"])]
    public void ComputeDisplayName()
    {
        DisplayName = $"Name: {Name}, Age: {Age}";
    }
}

[ModelDefinition("test_partner", Description = "Contact :D")]
public class TestPartner2: Model
{
    [FieldDefinition(Description = "Not the name of the partner")]
    [DefaultValue("LoL")]
    public string Name { get => Get<string>("Name"); set => Set("Name", value); }

    [FieldDefinition]
    public int Test { get => Get<int>("Test"); set => Set("Test", value); }
}

[ModelDefinition("test_partner")]
public class TestPartner3: Model
{
    [FieldDefinition(Name="Not his Age", Description = "Age of him")]
    [DefaultValue(nameof(DefaultAge), isMethod: true)]
    public int Age { get => Get<int>("Age"); set => Set("Age", value); }

    [FieldDefinition]
    [DefaultValue(30)]
    public int Test { get => Get<int>("Test"); set => Set("Test", value); }
    
    public static int DefaultAge() => 70;
}
