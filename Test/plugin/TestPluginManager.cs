using System.Reflection;
using lib.field;
using lib.model;
using lib.plugin;

namespace Test.plugin;

[TestFixture]
public class TestPluginManager
{
    private Assembly _assembly;
    private PluginManager _pluginManager;
    private APlugin _aPlugin;
    private TestPlugin _plugin; 
    
    [SetUp]
    public void Setup()
    {
        _assembly = typeof(TestPluginManager).Assembly;
        _pluginManager = new("");
        _pluginManager.RegisterPlugin(_assembly);
        _aPlugin = _pluginManager.AvailablePlugins.First();
        _plugin = _aPlugin.Plugin as TestPlugin;
    }

    [Test]
    public void TestPluginLoaded()
    {
        Assert.That(_pluginManager.AvailablePluginsSize, Is.EqualTo(1), "Only test plugin should be available");
        Assert.That(_aPlugin.Id, Is.EqualTo(_plugin.Id));
        Assert.That(_aPlugin.Name, Is.EqualTo(_plugin.Name));
        Assert.That(_aPlugin.Version, Is.EqualTo(_plugin.Version));
        Assert.That(_aPlugin.Dependencies, Is.Empty);
        Assert.That(_aPlugin.Models, Has.Count.EqualTo(1));
        Assert.That(_aPlugin.IsInstalled, Is.False);
        Assert.That(_aPlugin.State, Is.EqualTo(APlugin.PluginState.NotInstalled));
        Assert.That(_plugin.NumberOfOnStart, Is.EqualTo(0));
        Assert.That(_plugin.NumberOfOnEnd, Is.EqualTo(0));
        
        // Plugin is not installed
        Assert.That(_pluginManager.PluginsSize, Is.EqualTo(0), "Test plugin is not installed");
        Assert.That(_pluginManager.CommandsSize, Is.EqualTo(0), "Test plugin is not installed");
        Assert.That(_pluginManager.ModelsSize, Is.EqualTo(0), "Test plugin is not installed");
        Assert.That(_pluginManager.TotalModelsSize, Is.EqualTo(0), "Test plugin is not installed");
        Assert.That(_pluginManager.GetInstalledPlugin("test"), Is.Null);
        Assert.That(_pluginManager.IsPluginInstalled("test"), Is.False);

        // Install plugin
        _pluginManager.InstallPlugin(_aPlugin);
        
        // Plugin is installed
        Assert.That(_aPlugin.IsInstalled, Is.True);
        Assert.That(_aPlugin.State, Is.EqualTo(APlugin.PluginState.Installed));
        Assert.That(_plugin.NumberOfOnStart, Is.EqualTo(1));
        Assert.That(_plugin.NumberOfOnEnd, Is.EqualTo(0));
        
        Assert.That(_pluginManager.PluginsSize, Is.EqualTo(1), "Test plugin is installed");
        Assert.That(_pluginManager.CommandsSize, Is.EqualTo(0), "Test plugin is installed");
        Assert.That(_pluginManager.ModelsSize, Is.EqualTo(1), "Test plugin is installed, and has only 1 model");
        Assert.That(_pluginManager.TotalModelsSize, Is.EqualTo(3), "Test plugin is installed, and has 3 model override");
        Assert.That(_pluginManager.GetInstalledPlugin("test"), Is.EqualTo(_aPlugin));
        Assert.That(_pluginManager.IsPluginInstalled("test"), Is.True);
        
        Assert.That(_pluginManager.GetPlugin("test"), Is.EqualTo(_aPlugin));

        Assert.That(_pluginManager.PluginsInDependencyOrder, Is.EquivalentTo(new[] { _aPlugin }));
    }

    [Test]
    public void TestPluginModels()
    {
        Assert.That(_aPlugin.Models, Has.Count.EqualTo(1));
        Assert.That(_aPlugin.Models, Contains.Key("test_partner"));
        List<PluginModel> pluginModels = _aPlugin.Models["test_partner"];
        Assert.That(pluginModels, Has.Count.EqualTo(3));
        
        
        // First model
        Assert.That(pluginModels[0].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(pluginModels[0].Name, Is.EqualTo("test_partner"));
        Assert.That(pluginModels[0].Description, Is.EqualTo("Contact"));
        Assert.That(pluginModels[0].Fields, Has.Count.EqualTo(8));
        
        // Fields
        Assert.That(pluginModels[0].Fields, Contains.Key("Name"));
        Assert.That(pluginModels[0].Fields["Name"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(pluginModels[0].Fields["Name"].FieldName, Is.EqualTo("Name"));
        Assert.That(pluginModels[0].Fields["Name"].Name, Is.Null);
        Assert.That(pluginModels[0].Fields["Name"].Description, Is.EqualTo("Name of the partner"));
        Assert.That(pluginModels[0].Fields["Name"].DefaultComputedMethod, Is.Not.Null);
        Assert.That(pluginModels[0].Fields["Name"].DefaultComputedMethod.FieldName, Is.EqualTo("Name"));
        Assert.That(pluginModels[0].Fields["Name"].DefaultComputedMethod.DefaultValue, Is.EqualTo("Test"));
        Assert.That(pluginModels[0].Fields["Name"].DefaultComputedMethod.ComputedAttribute, Is.Null);
        Assert.That(pluginModels[0].Fields["Name"].DefaultComputedMethod.MethodInfo, Is.Null);
        Assert.That(pluginModels[0].Fields["Name"].DefaultComputedMethod.IsComputedStatic, Is.False);
        Assert.That(pluginModels[0].Fields["Name"].DefaultComputedMethod.IsPresent, Is.True);
        
        Assert.That(pluginModels[0].Fields, Contains.Key("Age"));
        Assert.That(pluginModels[0].Fields["Age"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(pluginModels[0].Fields["Age"].FieldName, Is.EqualTo("Age"));
        Assert.That(pluginModels[0].Fields["Age"].Name, Is.EqualTo("Age 2"));
        Assert.That(pluginModels[0].Fields["Age"].Description, Is.EqualTo("Age of the partner"));
        Assert.That(pluginModels[0].Fields["Age"].DefaultComputedMethod, Is.Not.Null);
        Assert.That(pluginModels[0].Fields["Age"].DefaultComputedMethod.FieldName, Is.EqualTo("Age"));
        Assert.That(pluginModels[0].Fields["Age"].DefaultComputedMethod.DefaultValue, Is.EqualTo(42));
        Assert.That(pluginModels[0].Fields["Age"].DefaultComputedMethod.ComputedAttribute, Is.Null);
        Assert.That(pluginModels[0].Fields["Age"].DefaultComputedMethod.MethodInfo, Is.Null);
        Assert.That(pluginModels[0].Fields["Age"].DefaultComputedMethod.IsComputedStatic, Is.False);
        Assert.That(pluginModels[0].Fields["Age"].DefaultComputedMethod.IsPresent, Is.True);
        
        Assert.That(pluginModels[0].Fields, Contains.Key("Color"));
        Assert.That(pluginModels[0].Fields["Color"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(pluginModels[0].Fields["Color"].FieldName, Is.EqualTo("Color"));
        Assert.That(pluginModels[0].Fields["Color"].Name, Is.Null);
        Assert.That(pluginModels[0].Fields["Color"].Description, Is.Null);
        Assert.That(pluginModels[0].Fields["Color"].DefaultComputedMethod, Is.Not.Null);
        Assert.That(pluginModels[0].Fields["Color"].DefaultComputedMethod.FieldName, Is.EqualTo("Color"));
        Assert.That(pluginModels[0].Fields["Color"].DefaultComputedMethod.DefaultValue, Is.EqualTo("DefaultRandomColor"));
        Assert.That(pluginModels[0].Fields["Color"].DefaultComputedMethod.ComputedAttribute, Is.Null);
        Assert.That(pluginModels[0].Fields["Color"].DefaultComputedMethod.MethodInfo, Is.Not.Null);
        Assert.That(pluginModels[0].Fields["Color"].DefaultComputedMethod.IsComputedStatic, Is.True);
        Assert.That(pluginModels[0].Fields["Color"].DefaultComputedMethod.IsPresent, Is.True);
        
        Assert.That(pluginModels[0].Fields, Contains.Key("DisplayName"));
        Assert.That(pluginModels[0].Fields["DisplayName"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(pluginModels[0].Fields["DisplayName"].FieldName, Is.EqualTo("DisplayName"));
        Assert.That(pluginModels[0].Fields["DisplayName"].Name, Is.Null);
        Assert.That(pluginModels[0].Fields["DisplayName"].Description, Is.EqualTo("Name to display of the partner"));
        Assert.That(pluginModels[0].Fields["DisplayName"].DefaultComputedMethod, Is.Not.Null);
        Assert.That(pluginModels[0].Fields["DisplayName"].DefaultComputedMethod.FieldName, Is.EqualTo("DisplayName"));
        Assert.That(pluginModels[0].Fields["DisplayName"].DefaultComputedMethod.DefaultValue, Is.EqualTo("ComputeDisplayName"));
        Assert.That(pluginModels[0].Fields["DisplayName"].DefaultComputedMethod.ComputedAttribute, Is.Not.Null);
        Assert.That(pluginModels[0].Fields["DisplayName"].DefaultComputedMethod.MethodInfo, Is.Not.Null);
        Assert.That(pluginModels[0].Fields["DisplayName"].DefaultComputedMethod.IsComputedStatic, Is.False);
        Assert.That(pluginModels[0].Fields["DisplayName"].DefaultComputedMethod.IsPresent, Is.True);
        
        Assert.That(pluginModels[0].Fields, Contains.Key("MyDate"));
        Assert.That(pluginModels[0].Fields["MyDate"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(pluginModels[0].Fields["MyDate"].FieldName, Is.EqualTo("MyDate"));
        Assert.That(pluginModels[0].Fields["MyDate"].Name, Is.EqualTo("MyDate"));
        Assert.That(pluginModels[0].Fields["MyDate"].Description, Is.EqualTo("My Date"));
        Assert.That(pluginModels[0].Fields["MyDate"].DefaultComputedMethod, Is.Not.Null);
        Assert.That(pluginModels[0].Fields["MyDate"].DefaultComputedMethod.FieldName, Is.EqualTo("MyDate"));
        Assert.That(pluginModels[0].Fields["MyDate"].DefaultComputedMethod.DefaultValue, Is.EqualTo("DefaultMyDate"));
        Assert.That(pluginModels[0].Fields["MyDate"].DefaultComputedMethod.ComputedAttribute, Is.Null);
        Assert.That(pluginModels[0].Fields["MyDate"].DefaultComputedMethod.MethodInfo, Is.Not.Null);
        Assert.That(pluginModels[0].Fields["MyDate"].DefaultComputedMethod.IsComputedStatic, Is.True);
        Assert.That(pluginModels[0].Fields["MyDate"].DefaultComputedMethod.IsPresent, Is.True);
        
        Assert.That(pluginModels[0].Fields, Contains.Key("MyDate"));
        Assert.That(pluginModels[0].Fields["MyDateTime"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(pluginModels[0].Fields["MyDateTime"].FieldName, Is.EqualTo("MyDateTime"));
        Assert.That(pluginModels[0].Fields["MyDateTime"].Name, Is.EqualTo("MyTime"));
        Assert.That(pluginModels[0].Fields["MyDateTime"].Description, Is.EqualTo("My Date Time"));
        Assert.That(pluginModels[0].Fields["MyDateTime"].DefaultComputedMethod, Is.Null);

        // Second model
        Assert.That(pluginModels[1].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(pluginModels[1].Name, Is.EqualTo("test_partner"));
        Assert.That(pluginModels[1].Description, Is.EqualTo("Contact :D"));
        Assert.That(pluginModels[1].Fields, Has.Count.EqualTo(4));
        
        // Fields
        Assert.That(pluginModels[1].Fields, Contains.Key("Name"));
        Assert.That(pluginModels[1].Fields["Name"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(pluginModels[1].Fields["Name"].FieldName, Is.EqualTo("Name"));
        Assert.That(pluginModels[1].Fields["Name"].Name, Is.Null);
        Assert.That(pluginModels[1].Fields["Name"].Description, Is.EqualTo("Not the name of the partner"));
        Assert.That(pluginModels[1].Fields["Name"].DefaultComputedMethod, Is.Not.Null);
        Assert.That(pluginModels[1].Fields["Name"].DefaultComputedMethod.FieldName, Is.EqualTo("Name"));
        Assert.That(pluginModels[1].Fields["Name"].DefaultComputedMethod.DefaultValue, Is.EqualTo("LoL"));
        Assert.That(pluginModels[1].Fields["Name"].DefaultComputedMethod.ComputedAttribute, Is.Null);
        Assert.That(pluginModels[1].Fields["Name"].DefaultComputedMethod.MethodInfo, Is.Null);
        Assert.That(pluginModels[1].Fields["Name"].DefaultComputedMethod.IsComputedStatic, Is.False);
        Assert.That(pluginModels[1].Fields["Name"].DefaultComputedMethod.IsPresent, Is.True);
        
        Assert.That(pluginModels[1].Fields, Contains.Key("Test"));
        Assert.That(pluginModels[1].Fields["Test"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(pluginModels[1].Fields["Test"].FieldName, Is.EqualTo("Test"));
        Assert.That(pluginModels[1].Fields["Test"].Name, Is.Null);
        Assert.That(pluginModels[1].Fields["Test"].Description, Is.Null);
        Assert.That(pluginModels[1].Fields["Test"].DefaultComputedMethod, Is.Null);

        // Third model
        Assert.That(pluginModels[2].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(pluginModels[2].Name, Is.EqualTo("test_partner"));
        Assert.That(pluginModels[2].Description, Is.Null);
        Assert.That(pluginModels[2].Fields, Has.Count.EqualTo(4));
        
        // Fields
        Assert.That(pluginModels[2].Fields, Contains.Key("Age"));
        Assert.That(pluginModels[2].Fields["Age"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(pluginModels[2].Fields["Age"].FieldName, Is.EqualTo("Age"));
        Assert.That(pluginModels[2].Fields["Age"].Name, Is.EqualTo("Not his Age"));
        Assert.That(pluginModels[2].Fields["Age"].Description, Is.EqualTo("Age of him"));
        Assert.That(pluginModels[2].Fields["Age"].DefaultComputedMethod, Is.Not.Null);
        Assert.That(pluginModels[2].Fields["Age"].DefaultComputedMethod.FieldName, Is.EqualTo("Age"));
        Assert.That(pluginModels[2].Fields["Age"].DefaultComputedMethod.DefaultValue, Is.EqualTo("DefaultAge"));
        Assert.That(pluginModels[2].Fields["Age"].DefaultComputedMethod.ComputedAttribute, Is.Null);
        Assert.That(pluginModels[2].Fields["Age"].DefaultComputedMethod.MethodInfo, Is.Not.Null);
        Assert.That(pluginModels[2].Fields["Age"].DefaultComputedMethod.IsComputedStatic, Is.True);
        Assert.That(pluginModels[2].Fields["Age"].DefaultComputedMethod.IsPresent, Is.True);
        
        Assert.That(pluginModels[2].Fields, Contains.Key("Test"));
        Assert.That(pluginModels[2].Fields["Test"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(pluginModels[2].Fields["Test"].FieldName, Is.EqualTo("Test"));
        Assert.That(pluginModels[2].Fields["Test"].Name, Is.Null);
        Assert.That(pluginModels[2].Fields["Test"].Description, Is.Null);
        Assert.That(pluginModels[2].Fields["Test"].DefaultComputedMethod, Is.Not.Null);
        Assert.That(pluginModels[2].Fields["Test"].DefaultComputedMethod.FieldName, Is.EqualTo("Test"));
        Assert.That(pluginModels[2].Fields["Test"].DefaultComputedMethod.DefaultValue, Is.EqualTo(30));
        Assert.That(pluginModels[2].Fields["Test"].DefaultComputedMethod.ComputedAttribute, Is.Null);
        Assert.That(pluginModels[2].Fields["Test"].DefaultComputedMethod.MethodInfo, Is.Null);
        Assert.That(pluginModels[2].Fields["Test"].DefaultComputedMethod.IsComputedStatic, Is.False);
        Assert.That(pluginModels[2].Fields["Test"].DefaultComputedMethod.IsPresent, Is.True);
    }

    [Test]
    public void TestModelMerges()
    {
        // Install plugin
        _pluginManager.InstallPlugin(_aPlugin);
        
        Assert.That(_pluginManager.ModelsSize, Is.EqualTo(1));
        Assert.That(_pluginManager.TotalModelsSize, Is.EqualTo(3));
        Assert.That(_pluginManager.ModelsSize, Is.EqualTo(1));
        Assert.That(_pluginManager.TotalModelsSize, Is.EqualTo(3));

        FinalModel model = _pluginManager.GetFinalModel("test_partner");
        Assert.That(model, Is.EqualTo(_pluginManager.Models.First()));
        Assert.That(model.Name, Is.EqualTo("test_partner"));
        Assert.That(model.FirstOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0]));
        Assert.That(model.Description, Is.EqualTo("Contact :D"));
        Assert.That(model.AllOccurences, Has.Count.EqualTo(3));
        Assert.That(model.Fields, Has.Count.EqualTo(9));

        List<FinalField> finalFields = model.Fields.Values.ToList();
        Assert.That(finalFields[0].FieldName, Is.EqualTo("Name"));
        Assert.That(finalFields[0].FieldType, Is.EqualTo(FieldType.String));
        Assert.That(finalFields[0].FirstOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["Name"]));
        Assert.That(finalFields[0].LastOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][1].Fields["Name"]));
        Assert.That(finalFields[0].AllOccurences, Has.Count.EqualTo(2));
        Assert.That(finalFields[0].AllOccurences, Is.EquivalentTo(new[] { _aPlugin.Models["test_partner"][0].Fields["Name"], _aPlugin.Models["test_partner"][1].Fields["Name"] }));
        Assert.That(finalFields[0].Name, Is.EqualTo("Name"));
        Assert.That(finalFields[0].Description, Is.EqualTo("Not the name of the partner"));
        Assert.That(finalFields[0].DefaultComputedMethod, Is.Not.Null);
        Assert.That(finalFields[0].DefaultComputedMethod, Is.EqualTo(_aPlugin.Models["test_partner"][1].Fields["Name"].DefaultComputedMethod));
        Assert.That(finalFields[0].InverseCompute, Has.Count.EqualTo(1));
        Assert.That(finalFields[0].InverseCompute[0], Is.EqualTo(finalFields[3]));

        Assert.That(finalFields[1].FieldName, Is.EqualTo("Age"));
        Assert.That(finalFields[1].FieldType, Is.EqualTo(FieldType.Integer));
        Assert.That(finalFields[1].FirstOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["Age"]));
        Assert.That(finalFields[1].LastOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][2].Fields["Age"]));
        Assert.That(finalFields[1].AllOccurences, Has.Count.EqualTo(2));
        Assert.That(finalFields[1].AllOccurences, Is.EquivalentTo(new[] { _aPlugin.Models["test_partner"][0].Fields["Age"], _aPlugin.Models["test_partner"][2].Fields["Age"] }));
        Assert.That(finalFields[1].Name, Is.EqualTo("Not his Age"));
        Assert.That(finalFields[1].Description, Is.EqualTo("Age of him"));
        Assert.That(finalFields[1].DefaultComputedMethod, Is.Not.Null);
        Assert.That(finalFields[1].DefaultComputedMethod, Is.EqualTo(_aPlugin.Models["test_partner"][2].Fields["Age"].DefaultComputedMethod));
        Assert.That(finalFields[1].InverseCompute, Has.Count.EqualTo(1));
        Assert.That(finalFields[1].InverseCompute[0], Is.EqualTo(finalFields[3]));
        
        Assert.That(finalFields[2].FieldName, Is.EqualTo("Color"));
        Assert.That(finalFields[2].FieldType, Is.EqualTo(FieldType.Integer));
        Assert.That(finalFields[2].FirstOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["Color"]));
        Assert.That(finalFields[2].LastOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["Color"]));
        Assert.That(finalFields[2].AllOccurences, Has.Count.EqualTo(1));
        Assert.That(finalFields[2].AllOccurences, Is.EquivalentTo(new[] { _aPlugin.Models["test_partner"][0].Fields["Color"] }));
        Assert.That(finalFields[2].Name, Is.EqualTo("Color"));
        Assert.That(finalFields[2].Description, Is.EqualTo("Color"));
        Assert.That(finalFields[2].DefaultComputedMethod, Is.Not.Null);
        Assert.That(finalFields[2].DefaultComputedMethod, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["Color"].DefaultComputedMethod));
        Assert.That(finalFields[2].InverseCompute, Is.Empty);
        
        Assert.That(finalFields[3].FieldName, Is.EqualTo("DisplayName"));
        Assert.That(finalFields[3].FieldType, Is.EqualTo(FieldType.String));
        Assert.That(finalFields[3].FirstOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["DisplayName"]));
        Assert.That(finalFields[3].LastOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["DisplayName"]));
        Assert.That(finalFields[3].AllOccurences, Has.Count.EqualTo(1));
        Assert.That(finalFields[3].AllOccurences, Is.EquivalentTo(new[] { _aPlugin.Models["test_partner"][0].Fields["DisplayName"] }));
        Assert.That(finalFields[3].Name, Is.EqualTo("DisplayName"));
        Assert.That(finalFields[3].Description, Is.EqualTo("Name to display of the partner"));
        Assert.That(finalFields[3].DefaultComputedMethod, Is.Not.Null);
        Assert.That(finalFields[3].DefaultComputedMethod, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["DisplayName"].DefaultComputedMethod));
        Assert.That(finalFields[3].InverseCompute, Is.Empty);
        
        Assert.That(finalFields[4].FieldName, Is.EqualTo("MyDate"));
        Assert.That(finalFields[4].FieldType, Is.EqualTo(FieldType.Date));
        Assert.That(finalFields[4].FirstOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["MyDate"]));
        Assert.That(finalFields[4].LastOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["MyDate"]));
        Assert.That(finalFields[4].AllOccurences, Has.Count.EqualTo(1));
        Assert.That(finalFields[4].AllOccurences, Is.EquivalentTo(new[] { _aPlugin.Models["test_partner"][0].Fields["MyDate"] }));
        Assert.That(finalFields[4].Name, Is.EqualTo("MyDate"));
        Assert.That(finalFields[4].Description, Is.EqualTo("My Date"));
        Assert.That(finalFields[4].DefaultComputedMethod, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["MyDate"].DefaultComputedMethod));
        Assert.That(finalFields[4].InverseCompute, Is.Empty);
        
        Assert.That(finalFields[5].FieldName, Is.EqualTo("MyDateTime"));
        Assert.That(finalFields[5].FieldType, Is.EqualTo(FieldType.Datetime));
        Assert.That(finalFields[5].FirstOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["MyDateTime"]));
        Assert.That(finalFields[5].LastOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["MyDateTime"]));
        Assert.That(finalFields[5].AllOccurences, Has.Count.EqualTo(1));
        Assert.That(finalFields[5].AllOccurences, Is.EquivalentTo(new[] { _aPlugin.Models["test_partner"][0].Fields["MyDateTime"] }));
        Assert.That(finalFields[5].Name, Is.EqualTo("MyTime"));
        Assert.That(finalFields[5].Description, Is.EqualTo("My Date Time"));
        Assert.That(finalFields[5].DefaultComputedMethod, Is.Null);
        Assert.That(finalFields[5].InverseCompute, Is.Empty);
        
        // 6 & 7 are CreationDate & UpdateDate
        Assert.That(finalFields[8].FieldName, Is.EqualTo("Test"));
        Assert.That(finalFields[8].FieldType, Is.EqualTo(FieldType.Integer));
        Assert.That(finalFields[8].FirstOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][1].Fields["Test"]));
        Assert.That(finalFields[8].LastOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][2].Fields["Test"]));
        Assert.That(finalFields[8].AllOccurences, Has.Count.EqualTo(2));
        Assert.That(finalFields[8].AllOccurences, Is.EquivalentTo(new[] { _aPlugin.Models["test_partner"][1].Fields["Test"], _aPlugin.Models["test_partner"][2].Fields["Test"] }));
        Assert.That(finalFields[8].Name, Is.EqualTo("Test"));
        Assert.That(finalFields[8].Description, Is.EqualTo("Test"));
        Assert.That(finalFields[8].DefaultComputedMethod, Is.Not.Null);
        Assert.That(finalFields[8].DefaultComputedMethod, Is.EqualTo(_aPlugin.Models["test_partner"][2].Fields["Test"].DefaultComputedMethod));
        Assert.That(finalFields[8].InverseCompute, Is.Empty);
        
    }
}
