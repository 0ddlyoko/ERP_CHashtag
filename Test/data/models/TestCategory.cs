using lib.field.attributes;
using lib.model;

namespace Test.data.models;

[ModelDefinition("test_category", Description = "Contact Category")]
public class TestCategory: Model
{
    [FieldDefinition(Description = "Name of the category")]
    public string Name { get => Get<string>("Name"); set => Set("Name", value); }
    
}
