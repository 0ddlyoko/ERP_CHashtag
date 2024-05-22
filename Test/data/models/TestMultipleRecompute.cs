using lib.field.attributes;
using lib.model;

namespace Test.data.models;

[ModelDefinition(("test_multiple_recompute"))]
public class TestMultipleRecompute: Model
{
    [FieldDefinition]
    [DefaultValue("0ddlyoko")]
    public string? Name { get => Get<string>("Name"); set => Set("Name", value); }

    [FieldDefinition]
    [DefaultValue(nameof(ComputeName2), isMethod: true)]
    public string? Name2 { get => Get<string>("Name2"); set => Set("Name2", value); }

    [FieldDefinition]
    [DefaultValue(nameof(ComputeName3), isMethod: true)]
    public string? Name3 { get => Get<string>("Name3"); set => Set("Name3", value); }

    [FieldDefinition]
    [DefaultValue(nameof(ComputeName4), isMethod: true)]
    public string? Name4 { get => Get<string>("Name4"); set => Set("Name4", value); }
    
    [FieldDefinition]
    public TestMultipleRecompute? Single { get => Get<TestMultipleRecompute>("Single"); set => Set("Single", value); }
    
    [FieldDefinition]
    [DefaultValue(nameof(ComputeSingle2), isMethod: true)]
    public TestMultipleRecompute Single2 { get => Get<TestMultipleRecompute>("Single2"); set => Set("Single2", value); }
    
    [FieldDefinition]
    [DefaultValue(nameof(ComputeSingle3), isMethod: true)]
    public TestMultipleRecompute Single3 { get => Get<TestMultipleRecompute>("Single3"); set => Set("Single3", value); }
    
    [FieldDefinition]
    [OneToMany(Target = "Single2")]
    public TestMultipleRecompute Multi { get => Get<TestMultipleRecompute>("Multi"); set => Set("Multi", value); }

    [Computed(["Name"])]
    public void ComputeName2()
    {
        foreach (TestMultipleRecompute rec in this)
        {
            rec.Name2 = $"{rec.Name}-2";
        }
    }

    [Computed(["Name2"])]
    public void ComputeName3()
    {
        foreach (TestMultipleRecompute rec in this)
        {
            rec.Name3 = $"{rec.Name2}-3";
        }
    }

    [Computed(["Name3"])]
    public void ComputeName4()
    {
        foreach (TestMultipleRecompute rec in this)
        {
            rec.Name4 = $"{rec.Name3}-4";
        }
    }

    [Computed(["Single"])]
    public void ComputeSingle2()
    {
        foreach (TestMultipleRecompute rec in this)
        {
            rec.Single2 = rec.Single;
        }
    }

    [Computed(["Single2"])]
    public void ComputeSingle3()
    {
        foreach (TestMultipleRecompute rec in this)
        {
            rec.Single3 = rec.Single2;
        }
    }
}

[ModelDefinition("test_model_2")]
public class TestModel2 : Model
{
    [FieldDefinition]
    [DefaultValue(nameof(ComputeName), isMethod: true)]
    public string Name { get => Get<string>("Name"); set => Set("Name", value); }
    
    [FieldDefinition]
    [DefaultValue(nameof(ComputeIsPresent), isMethod: true)]
    public bool IsPresent { get => Get<bool>("IsPresent"); set => Set("IsPresent", value); }
    
    [FieldDefinition]
    public TestMultipleRecompute AnotherModel { get => Get<TestMultipleRecompute>("AnotherModel"); set => Set("AnotherModel", value); }

    [Computed(["AnotherModel.Name"])]
    public void ComputeName()
    {
        foreach (TestModel2 rec in this)
        {
            rec.Name = rec.AnotherModel.Ids.Count == 0 ? "Unknown" : rec.AnotherModel.Name;
        }
    }

    [Computed(["AnotherModel"])]
    public void ComputeIsPresent()
    {
        foreach (TestModel2 rec in this)
        {
            rec.IsPresent = rec.AnotherModel.Ids.Count != 0;
        }
    }
}
