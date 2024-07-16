using lib.field.attributes;
using lib.model;

namespace Test.data.models;

[ModelDefinition(("test_limited_partner"))]
public class TestLimitedPartner: Model
{
    [FieldDefinition]
    public string Name { get => Get<string>("Name"); set => Set("Name", value); }
    
    [FieldDefinition(Name = "State", Description = "Partner's State")]
    [DefaultValue("free")]
    [Selection("blocked", "Blocked")]
    [Selection("managed", "Managed")]
    [Selection("free", "Free")]
    public string State { get => Get<string>("State"); set => Set("State", value); }
    
    [FieldDefinition]
    public int Limit { get => Get<int>("Limit"); set => Set("Limit", value); }
    
    [FieldDefinition]
    public int CurrentMoney { get => Get<int>("CurrentMoney"); set => Set("CurrentMoney", value); }
    
    [FieldDefinition]
    [DefaultValue(nameof(ComputeIsLimited), isMethod: true)]
    public bool IsLimited { get => Get<bool>("IsLimited"); set => Set("IsLimited", value); }

    [Computed(["State", "Limit", "CurrentMoney"])]
    public void ComputeIsLimited()
    {
        foreach (TestLimitedPartner limitedPartner in this)
        {
            limitedPartner.IsLimited = limitedPartner.State switch
            {
                "blocked" => true,
                "free" => false,
                _ => limitedPartner.Limit != 0 && limitedPartner.CurrentMoney > limitedPartner.Limit
            };
        }
    }
}