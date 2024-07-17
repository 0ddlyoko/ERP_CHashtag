using lib.field.attributes;
using lib.model;

namespace main.models;

[ModelDefinition("module", Description = "Represent a single module")]
public class Module: Model
{
    [FieldDefinition(Description = "Name of the module")]
    public string Name { get => Get<string>("Name"); set => Set("Name", value); }
    
    [FieldDefinition(Description = "Description of the module")]
    public string Description { get => Get<string>("Description"); set => Set("Description", value); }
    
    [FieldDefinition(Description = "Installed version of the module")]
    public string? InstalledVersion { get => Get<string?>("InstalledVersion"); set => Set("InstalledVersion", value); }
    
    [FieldDefinition(Description = "Latest version of the module")]
    public string LatestVersion { get => Get<string>("LatestVersion"); set => Set("LatestVersion", value); }
    
    [FieldDefinition(Description = "Status of the module")]
    [DefaultValue("uninstalled")]
    [Selection("uninstalled", "Uninstalled")]
    [Selection("to_install", "To Install")]
    [Selection("installed", "Installed")]
    [Selection("to_uninstall", "To Uninstall")]
    public string Status { get => Get<string>("Status"); set => Set("Status", value); }
}
