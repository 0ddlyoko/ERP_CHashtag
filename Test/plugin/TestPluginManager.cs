using System.Reflection;
using lib.field;
using lib.model;
using lib.plugin;
using Test.data.models;

namespace Test.plugin;

[TestFixture]
public class TestPluginManager
{
    private const int TotalNumberOfModels = 4;
    private const int TotalNumberOfModelOverride = 6;
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
        Assert.That(_aPlugin.Models, Has.Count.EqualTo(TotalNumberOfModels));
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
        Assert.That(_pluginManager.ModelsSize, Is.EqualTo(TotalNumberOfModels), $"Test plugin is installed, and has {TotalNumberOfModels} models");
        Assert.That(_pluginManager.TotalModelsSize, Is.EqualTo(TotalNumberOfModelOverride), $"Test plugin is installed, and has {TotalNumberOfModelOverride} models override");
        Assert.That(_pluginManager.GetInstalledPlugin("test"), Is.EqualTo(_aPlugin));
        Assert.That(_pluginManager.IsPluginInstalled("test"), Is.True);
        
        Assert.That(_pluginManager.GetPlugin("test"), Is.EqualTo(_aPlugin));

        Assert.That(_pluginManager.PluginsInDependencyOrder, Is.EquivalentTo(new[] { _aPlugin }));
    }

    [Test]
    public void TestPluginModels()
    {
        Assert.That(_aPlugin.Models, Has.Count.EqualTo(TotalNumberOfModels));
        Assert.That(_aPlugin.Models, Contains.Key("test_partner"));
        Assert.That(_aPlugin.Models, Contains.Key("test_category"));
        List<PluginModel> testPartnerModels = _aPlugin.Models["test_partner"];
        List<PluginModel> testCategoryModels = _aPlugin.Models["test_category"];
        Assert.That(testPartnerModels, Has.Count.EqualTo(3));
        Assert.That(testCategoryModels, Has.Count.EqualTo(1));
        
        
        // First model
        Assert.That(testPartnerModels[0].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(testPartnerModels[0].Name, Is.EqualTo("test_partner"));
        Assert.That(testPartnerModels[0].Description, Is.EqualTo("Contact"));
        Assert.That(testPartnerModels[0].Fields, Has.Count.EqualTo(9));
        
        // Fields
        Assert.That(testPartnerModels[0].Fields, Contains.Key("Name"));
        Assert.That(testPartnerModels[0].Fields["Name"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(testPartnerModels[0].Fields["Name"].FieldName, Is.EqualTo("Name"));
        Assert.That(testPartnerModels[0].Fields["Name"].FieldType, Is.EqualTo(FieldType.String));
        Assert.That(testPartnerModels[0].Fields["Name"].Name, Is.Null);
        Assert.That(testPartnerModels[0].Fields["Name"].Description, Is.EqualTo("Name of the partner"));
        Assert.That(testPartnerModels[0].Fields["Name"].DefaultComputedMethod, Is.Not.Null);
        Assert.That(testPartnerModels[0].Fields["Name"].DefaultComputedMethod.FieldName, Is.EqualTo("Name"));
        Assert.That(testPartnerModels[0].Fields["Name"].DefaultComputedMethod.DefaultValue, Is.EqualTo("Test"));
        Assert.That(testPartnerModels[0].Fields["Name"].DefaultComputedMethod.ComputedAttribute, Is.Null);
        Assert.That(testPartnerModels[0].Fields["Name"].DefaultComputedMethod.MethodInfo, Is.Null);
        Assert.That(testPartnerModels[0].Fields["Name"].DefaultComputedMethod.IsComputedStatic, Is.False);
        Assert.That(testPartnerModels[0].Fields["Name"].DefaultComputedMethod.IsPresent, Is.True);
        
        Assert.That(testPartnerModels[0].Fields, Contains.Key("Age"));
        Assert.That(testPartnerModels[0].Fields["Age"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(testPartnerModels[0].Fields["Age"].FieldName, Is.EqualTo("Age"));
        Assert.That(testPartnerModels[0].Fields["Age"].FieldType, Is.EqualTo(FieldType.Integer));
        Assert.That(testPartnerModels[0].Fields["Age"].Name, Is.EqualTo("Age 2"));
        Assert.That(testPartnerModels[0].Fields["Age"].Description, Is.EqualTo("Age of the partner"));
        Assert.That(testPartnerModels[0].Fields["Age"].DefaultComputedMethod, Is.Not.Null);
        Assert.That(testPartnerModels[0].Fields["Age"].DefaultComputedMethod.FieldName, Is.EqualTo("Age"));
        Assert.That(testPartnerModels[0].Fields["Age"].DefaultComputedMethod.DefaultValue, Is.EqualTo(42));
        Assert.That(testPartnerModels[0].Fields["Age"].DefaultComputedMethod.ComputedAttribute, Is.Null);
        Assert.That(testPartnerModels[0].Fields["Age"].DefaultComputedMethod.MethodInfo, Is.Null);
        Assert.That(testPartnerModels[0].Fields["Age"].DefaultComputedMethod.IsComputedStatic, Is.False);
        Assert.That(testPartnerModels[0].Fields["Age"].DefaultComputedMethod.IsPresent, Is.True);
        
        Assert.That(testPartnerModels[0].Fields, Contains.Key("Color"));
        Assert.That(testPartnerModels[0].Fields["Color"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(testPartnerModels[0].Fields["Color"].FieldName, Is.EqualTo("Color"));
        Assert.That(testPartnerModels[0].Fields["Color"].FieldType, Is.EqualTo(FieldType.Integer));
        Assert.That(testPartnerModels[0].Fields["Color"].Name, Is.Null);
        Assert.That(testPartnerModels[0].Fields["Color"].Description, Is.Null);
        Assert.That(testPartnerModels[0].Fields["Color"].DefaultComputedMethod, Is.Not.Null);
        Assert.That(testPartnerModels[0].Fields["Color"].DefaultComputedMethod.FieldName, Is.EqualTo("Color"));
        Assert.That(testPartnerModels[0].Fields["Color"].DefaultComputedMethod.DefaultValue, Is.EqualTo("DefaultRandomColor"));
        Assert.That(testPartnerModels[0].Fields["Color"].DefaultComputedMethod.ComputedAttribute, Is.Null);
        Assert.That(testPartnerModels[0].Fields["Color"].DefaultComputedMethod.MethodInfo, Is.Not.Null);
        Assert.That(testPartnerModels[0].Fields["Color"].DefaultComputedMethod.IsComputedStatic, Is.True);
        Assert.That(testPartnerModels[0].Fields["Color"].DefaultComputedMethod.IsPresent, Is.True);
        
        Assert.That(testPartnerModels[0].Fields, Contains.Key("DisplayName"));
        Assert.That(testPartnerModels[0].Fields["DisplayName"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(testPartnerModels[0].Fields["DisplayName"].FieldName, Is.EqualTo("DisplayName"));
        Assert.That(testPartnerModels[0].Fields["DisplayName"].FieldType, Is.EqualTo(FieldType.String));
        Assert.That(testPartnerModels[0].Fields["DisplayName"].Name, Is.Null);
        Assert.That(testPartnerModels[0].Fields["DisplayName"].Description, Is.EqualTo("Name to display of the partner"));
        Assert.That(testPartnerModels[0].Fields["DisplayName"].DefaultComputedMethod, Is.Not.Null);
        Assert.That(testPartnerModels[0].Fields["DisplayName"].DefaultComputedMethod.FieldName, Is.EqualTo("DisplayName"));
        Assert.That(testPartnerModels[0].Fields["DisplayName"].DefaultComputedMethod.DefaultValue, Is.EqualTo("ComputeDisplayName"));
        Assert.That(testPartnerModels[0].Fields["DisplayName"].DefaultComputedMethod.ComputedAttribute, Is.Not.Null);
        Assert.That(testPartnerModels[0].Fields["DisplayName"].DefaultComputedMethod.MethodInfo, Is.Not.Null);
        Assert.That(testPartnerModels[0].Fields["DisplayName"].DefaultComputedMethod.IsComputedStatic, Is.False);
        Assert.That(testPartnerModels[0].Fields["DisplayName"].DefaultComputedMethod.IsPresent, Is.True);
        
        Assert.That(testPartnerModels[0].Fields, Contains.Key("MyDate"));
        Assert.That(testPartnerModels[0].Fields["MyDate"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(testPartnerModels[0].Fields["MyDate"].FieldName, Is.EqualTo("MyDate"));
        Assert.That(testPartnerModels[0].Fields["MyDate"].FieldType, Is.EqualTo(FieldType.Date));
        Assert.That(testPartnerModels[0].Fields["MyDate"].Name, Is.EqualTo("MyDate"));
        Assert.That(testPartnerModels[0].Fields["MyDate"].Description, Is.EqualTo("My Date"));
        Assert.That(testPartnerModels[0].Fields["MyDate"].DefaultComputedMethod, Is.Not.Null);
        Assert.That(testPartnerModels[0].Fields["MyDate"].DefaultComputedMethod.FieldName, Is.EqualTo("MyDate"));
        Assert.That(testPartnerModels[0].Fields["MyDate"].DefaultComputedMethod.DefaultValue, Is.EqualTo("DefaultMyDate"));
        Assert.That(testPartnerModels[0].Fields["MyDate"].DefaultComputedMethod.ComputedAttribute, Is.Null);
        Assert.That(testPartnerModels[0].Fields["MyDate"].DefaultComputedMethod.MethodInfo, Is.Not.Null);
        Assert.That(testPartnerModels[0].Fields["MyDate"].DefaultComputedMethod.IsComputedStatic, Is.True);
        Assert.That(testPartnerModels[0].Fields["MyDate"].DefaultComputedMethod.IsPresent, Is.True);
        
        Assert.That(testPartnerModels[0].Fields, Contains.Key("MyDateTime"));
        Assert.That(testPartnerModels[0].Fields["MyDateTime"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(testPartnerModels[0].Fields["MyDateTime"].FieldName, Is.EqualTo("MyDateTime"));
        Assert.That(testPartnerModels[0].Fields["MyDateTime"].FieldType, Is.EqualTo(FieldType.Datetime));
        Assert.That(testPartnerModels[0].Fields["MyDateTime"].Name, Is.EqualTo("MyTime"));
        Assert.That(testPartnerModels[0].Fields["MyDateTime"].Description, Is.EqualTo("My Date Time"));
        Assert.That(testPartnerModels[0].Fields["MyDateTime"].DefaultComputedMethod, Is.Null);
        
        Assert.That(testPartnerModels[0].Fields, Contains.Key("Category"));
        Assert.That(testPartnerModels[0].Fields["Category"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(testPartnerModels[0].Fields["Category"].FieldName, Is.EqualTo("Category"));
        Assert.That(testPartnerModels[0].Fields["Category"].FieldType, Is.EqualTo(FieldType.ManyToOne));
        Assert.That(testPartnerModels[0].Fields["Category"].Name, Is.EqualTo("Category"));
        Assert.That(testPartnerModels[0].Fields["Category"].Description, Is.EqualTo("Partner's category"));
        Assert.That(testPartnerModels[0].Fields["Category"].DefaultComputedMethod, Is.Null);

        // Second model
        Assert.That(testPartnerModels[1].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(testPartnerModels[1].Name, Is.EqualTo("test_partner"));
        Assert.That(testPartnerModels[1].Description, Is.EqualTo("Contact :D"));
        Assert.That(testPartnerModels[1].Fields, Has.Count.EqualTo(4));
        
        // Fields
        Assert.That(testPartnerModels[1].Fields, Contains.Key("Name"));
        Assert.That(testPartnerModels[1].Fields["Name"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(testPartnerModels[1].Fields["Name"].FieldName, Is.EqualTo("Name"));
        Assert.That(testPartnerModels[1].Fields["Name"].FieldType, Is.EqualTo(FieldType.String));
        Assert.That(testPartnerModels[1].Fields["Name"].Name, Is.Null);
        Assert.That(testPartnerModels[1].Fields["Name"].Description, Is.EqualTo("Not the name of the partner"));
        Assert.That(testPartnerModels[1].Fields["Name"].DefaultComputedMethod, Is.Not.Null);
        Assert.That(testPartnerModels[1].Fields["Name"].DefaultComputedMethod.FieldName, Is.EqualTo("Name"));
        Assert.That(testPartnerModels[1].Fields["Name"].DefaultComputedMethod.DefaultValue, Is.EqualTo("LoL"));
        Assert.That(testPartnerModels[1].Fields["Name"].DefaultComputedMethod.ComputedAttribute, Is.Null);
        Assert.That(testPartnerModels[1].Fields["Name"].DefaultComputedMethod.MethodInfo, Is.Null);
        Assert.That(testPartnerModels[1].Fields["Name"].DefaultComputedMethod.IsComputedStatic, Is.False);
        Assert.That(testPartnerModels[1].Fields["Name"].DefaultComputedMethod.IsPresent, Is.True);
        
        Assert.That(testPartnerModels[1].Fields, Contains.Key("Test"));
        Assert.That(testPartnerModels[1].Fields["Test"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(testPartnerModels[1].Fields["Test"].FieldName, Is.EqualTo("Test"));
        Assert.That(testPartnerModels[1].Fields["Test"].FieldType, Is.EqualTo(FieldType.Integer));
        Assert.That(testPartnerModels[1].Fields["Test"].Name, Is.Null);
        Assert.That(testPartnerModels[1].Fields["Test"].Description, Is.Null);
        Assert.That(testPartnerModels[1].Fields["Test"].DefaultComputedMethod, Is.Null);

        // Third model
        Assert.That(testPartnerModels[2].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(testPartnerModels[2].Name, Is.EqualTo("test_partner"));
        Assert.That(testPartnerModels[2].Description, Is.Null);
        Assert.That(testPartnerModels[2].Fields, Has.Count.EqualTo(4));
        
        // Fields
        Assert.That(testPartnerModels[2].Fields, Contains.Key("Age"));
        Assert.That(testPartnerModels[2].Fields["Age"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(testPartnerModels[2].Fields["Age"].FieldName, Is.EqualTo("Age"));
        Assert.That(testPartnerModels[2].Fields["Age"].FieldType, Is.EqualTo(FieldType.Integer));
        Assert.That(testPartnerModels[2].Fields["Age"].Name, Is.EqualTo("Not his Age"));
        Assert.That(testPartnerModels[2].Fields["Age"].Description, Is.EqualTo("Age of him"));
        Assert.That(testPartnerModels[2].Fields["Age"].DefaultComputedMethod, Is.Not.Null);
        Assert.That(testPartnerModels[2].Fields["Age"].DefaultComputedMethod.FieldName, Is.EqualTo("Age"));
        Assert.That(testPartnerModels[2].Fields["Age"].DefaultComputedMethod.DefaultValue, Is.EqualTo("DefaultAge"));
        Assert.That(testPartnerModels[2].Fields["Age"].DefaultComputedMethod.ComputedAttribute, Is.Null);
        Assert.That(testPartnerModels[2].Fields["Age"].DefaultComputedMethod.MethodInfo, Is.Not.Null);
        Assert.That(testPartnerModels[2].Fields["Age"].DefaultComputedMethod.IsComputedStatic, Is.True);
        Assert.That(testPartnerModels[2].Fields["Age"].DefaultComputedMethod.IsPresent, Is.True);
        
        Assert.That(testPartnerModels[2].Fields, Contains.Key("Test"));
        Assert.That(testPartnerModels[2].Fields["Test"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(testPartnerModels[2].Fields["Test"].FieldName, Is.EqualTo("Test"));
        Assert.That(testPartnerModels[2].Fields["Test"].FieldType, Is.EqualTo(FieldType.Integer));
        Assert.That(testPartnerModels[2].Fields["Test"].Name, Is.Null);
        Assert.That(testPartnerModels[2].Fields["Test"].Description, Is.Null);
        Assert.That(testPartnerModels[2].Fields["Test"].DefaultComputedMethod, Is.Not.Null);
        Assert.That(testPartnerModels[2].Fields["Test"].DefaultComputedMethod.FieldName, Is.EqualTo("Test"));
        Assert.That(testPartnerModels[2].Fields["Test"].DefaultComputedMethod.DefaultValue, Is.EqualTo(30));
        Assert.That(testPartnerModels[2].Fields["Test"].DefaultComputedMethod.ComputedAttribute, Is.Null);
        Assert.That(testPartnerModels[2].Fields["Test"].DefaultComputedMethod.MethodInfo, Is.Null);
        Assert.That(testPartnerModels[2].Fields["Test"].DefaultComputedMethod.IsComputedStatic, Is.False);
        Assert.That(testPartnerModels[2].Fields["Test"].DefaultComputedMethod.IsPresent, Is.True);

        // Category model
        Assert.That(testCategoryModels[0].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(testCategoryModels[0].Name, Is.EqualTo("test_category"));
        Assert.That(testCategoryModels[0].Description, Is.EqualTo("Contact Category"));
        Assert.That(testCategoryModels[0].Fields, Has.Count.EqualTo(4));
        
        // Fields
        Assert.That(testCategoryModels[0].Fields, Contains.Key("Name"));
        Assert.That(testCategoryModels[0].Fields["Name"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(testCategoryModels[0].Fields["Name"].FieldName, Is.EqualTo("Name"));
        Assert.That(testCategoryModels[0].Fields["Name"].FieldType, Is.EqualTo(FieldType.String));
        Assert.That(testCategoryModels[0].Fields["Name"].Name, Is.Null);
        Assert.That(testCategoryModels[0].Fields["Name"].Description, Is.EqualTo("Name of the category"));
        Assert.That(testCategoryModels[0].Fields["Name"].DefaultComputedMethod, Is.Null);
        
        Assert.That(testCategoryModels[0].Fields, Contains.Key("Partners"));
        Assert.That(testCategoryModels[0].Fields["Partners"].Plugin, Is.EqualTo(_aPlugin));
        Assert.That(testCategoryModels[0].Fields["Partners"].FieldName, Is.EqualTo("Partners"));
        Assert.That(testCategoryModels[0].Fields["Partners"].FieldType, Is.EqualTo(FieldType.OneToMany));
        Assert.That(testCategoryModels[0].Fields["Partners"].Name, Is.Null);
        Assert.That(testCategoryModels[0].Fields["Partners"].Description, Is.EqualTo("Partners linked to this category"));
        Assert.That(testCategoryModels[0].Fields["Partners"].DefaultComputedMethod, Is.Null);
        Assert.That(testCategoryModels[0].Fields["Partners"].Type, Is.EqualTo(typeof(TestPartner)));
        Assert.That(testCategoryModels[0].Fields["Partners"].TargetField, Is.EqualTo("Category"));
        Assert.That(testCategoryModels[0].Fields["Partners"].OriginColumnName, Is.Null);
        Assert.That(testCategoryModels[0].Fields["Partners"].TargetColumnName, Is.Null);
    }

    [Test]
    public void TestModelMerges()
    {
        // Install plugin
        _pluginManager.InstallPlugin(_aPlugin);
        
        Assert.That(_pluginManager.ModelsSize, Is.EqualTo(TotalNumberOfModels));
        Assert.That(_pluginManager.TotalModelsSize, Is.EqualTo(TotalNumberOfModelOverride));

        FinalModel partnerModel = _pluginManager.GetFinalModel("test_partner");
        Assert.That(partnerModel, Is.EqualTo(_pluginManager.Models.ToList()[0]));
        Assert.That(partnerModel.Name, Is.EqualTo("test_partner"));
        Assert.That(partnerModel.FirstOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0]));
        Assert.That(partnerModel.Description, Is.EqualTo("Contact :D"));
        Assert.That(partnerModel.AllOccurences, Has.Count.EqualTo(3));
        Assert.That(partnerModel.Fields, Has.Count.EqualTo(10));
        
        // TestPartner
        List<FinalField> partnerFields = partnerModel.Fields.Values.ToList();
        
        Assert.That(partnerFields[0].FieldName, Is.EqualTo("Name"));
        Assert.That(partnerFields[0].FieldType, Is.EqualTo(FieldType.String));
        Assert.That(partnerFields[0].FirstOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["Name"]));
        Assert.That(partnerFields[0].LastOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][1].Fields["Name"]));
        Assert.That(partnerFields[0].AllOccurences, Has.Count.EqualTo(2));
        Assert.That(partnerFields[0].AllOccurences, Is.EquivalentTo(new[] { _aPlugin.Models["test_partner"][0].Fields["Name"], _aPlugin.Models["test_partner"][1].Fields["Name"] }));
        Assert.That(partnerFields[0].Name, Is.EqualTo("Name"));
        Assert.That(partnerFields[0].Description, Is.EqualTo("Not the name of the partner"));
        Assert.That(partnerFields[0].DefaultComputedMethod, Is.Not.Null);
        Assert.That(partnerFields[0].DefaultComputedMethod, Is.EqualTo(_aPlugin.Models["test_partner"][1].Fields["Name"].DefaultComputedMethod));
        Assert.That(partnerFields[0].TreeDependency.Root, Is.EqualTo(partnerFields[0]));
        Assert.That(partnerFields[0].TreeDependency.IsLeaf, Is.False);
        Assert.That(partnerFields[0].TreeDependency.Items, Contains.Key("test_partner.DisplayName"));
        Assert.That(partnerFields[0].TreeDependency.Items, Contains.Value(new TreeNode(partnerFields[3], true)));

        Assert.That(partnerFields[1].FieldName, Is.EqualTo("Age"));
        Assert.That(partnerFields[1].FieldType, Is.EqualTo(FieldType.Integer));
        Assert.That(partnerFields[1].FirstOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["Age"]));
        Assert.That(partnerFields[1].LastOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][2].Fields["Age"]));
        Assert.That(partnerFields[1].AllOccurences, Has.Count.EqualTo(2));
        Assert.That(partnerFields[1].AllOccurences, Is.EquivalentTo(new[] { _aPlugin.Models["test_partner"][0].Fields["Age"], _aPlugin.Models["test_partner"][2].Fields["Age"] }));
        Assert.That(partnerFields[1].Name, Is.EqualTo("Not his Age"));
        Assert.That(partnerFields[1].Description, Is.EqualTo("Age of him"));
        Assert.That(partnerFields[1].DefaultComputedMethod, Is.Not.Null);
        Assert.That(partnerFields[1].DefaultComputedMethod, Is.EqualTo(_aPlugin.Models["test_partner"][2].Fields["Age"].DefaultComputedMethod));
        Assert.That(partnerFields[1].TreeDependency.Root, Is.EqualTo(partnerFields[1]));
        Assert.That(partnerFields[1].TreeDependency.IsLeaf, Is.False);
        Assert.That(partnerFields[1].TreeDependency.Items, Contains.Key("test_partner.DisplayName"));
        Assert.That(partnerFields[1].TreeDependency.Items, Contains.Value(new TreeNode(partnerFields[3], true)));
        
        Assert.That(partnerFields[2].FieldName, Is.EqualTo("Color"));
        Assert.That(partnerFields[2].FieldType, Is.EqualTo(FieldType.Integer));
        Assert.That(partnerFields[2].FirstOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["Color"]));
        Assert.That(partnerFields[2].LastOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["Color"]));
        Assert.That(partnerFields[2].AllOccurences, Has.Count.EqualTo(1));
        Assert.That(partnerFields[2].AllOccurences, Is.EquivalentTo(new[] { _aPlugin.Models["test_partner"][0].Fields["Color"] }));
        Assert.That(partnerFields[2].Name, Is.EqualTo("Color"));
        Assert.That(partnerFields[2].Description, Is.EqualTo("Color"));
        Assert.That(partnerFields[2].DefaultComputedMethod, Is.Not.Null);
        Assert.That(partnerFields[2].DefaultComputedMethod, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["Color"].DefaultComputedMethod));
        Assert.That(partnerFields[2].TreeDependency.Root, Is.EqualTo(partnerFields[2]));
        Assert.That(partnerFields[2].TreeDependency.IsLeaf, Is.True);
        Assert.That(partnerFields[2].TreeDependency.Items, Is.Empty);
        
        Assert.That(partnerFields[3].FieldName, Is.EqualTo("DisplayName"));
        Assert.That(partnerFields[3].FieldType, Is.EqualTo(FieldType.String));
        Assert.That(partnerFields[3].FirstOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["DisplayName"]));
        Assert.That(partnerFields[3].LastOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["DisplayName"]));
        Assert.That(partnerFields[3].AllOccurences, Has.Count.EqualTo(1));
        Assert.That(partnerFields[3].AllOccurences, Is.EquivalentTo(new[] { _aPlugin.Models["test_partner"][0].Fields["DisplayName"] }));
        Assert.That(partnerFields[3].Name, Is.EqualTo("DisplayName"));
        Assert.That(partnerFields[3].Description, Is.EqualTo("Name to display of the partner"));
        Assert.That(partnerFields[3].DefaultComputedMethod, Is.Not.Null);
        Assert.That(partnerFields[3].DefaultComputedMethod, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["DisplayName"].DefaultComputedMethod));
        Assert.That(partnerFields[3].TreeDependency.IsLeaf, Is.True);
        Assert.That(partnerFields[3].TreeDependency.Items, Is.Empty);
        
        Assert.That(partnerFields[4].FieldName, Is.EqualTo("MyDate"));
        Assert.That(partnerFields[4].FieldType, Is.EqualTo(FieldType.Date));
        Assert.That(partnerFields[4].FirstOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["MyDate"]));
        Assert.That(partnerFields[4].LastOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["MyDate"]));
        Assert.That(partnerFields[4].AllOccurences, Has.Count.EqualTo(1));
        Assert.That(partnerFields[4].AllOccurences, Is.EquivalentTo(new[] { _aPlugin.Models["test_partner"][0].Fields["MyDate"] }));
        Assert.That(partnerFields[4].Name, Is.EqualTo("MyDate"));
        Assert.That(partnerFields[4].Description, Is.EqualTo("My Date"));
        Assert.That(partnerFields[4].DefaultComputedMethod, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["MyDate"].DefaultComputedMethod));
        Assert.That(partnerFields[4].TreeDependency.IsLeaf, Is.True);
        Assert.That(partnerFields[4].TreeDependency.Items, Is.Empty);
        
        Assert.That(partnerFields[5].FieldName, Is.EqualTo("MyDateTime"));
        Assert.That(partnerFields[5].FieldType, Is.EqualTo(FieldType.Datetime));
        Assert.That(partnerFields[5].FirstOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["MyDateTime"]));
        Assert.That(partnerFields[5].LastOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["MyDateTime"]));
        Assert.That(partnerFields[5].AllOccurences, Has.Count.EqualTo(1));
        Assert.That(partnerFields[5].AllOccurences, Is.EquivalentTo(new[] { _aPlugin.Models["test_partner"][0].Fields["MyDateTime"] }));
        Assert.That(partnerFields[5].Name, Is.EqualTo("MyTime"));
        Assert.That(partnerFields[5].Description, Is.EqualTo("My Date Time"));
        Assert.That(partnerFields[5].DefaultComputedMethod, Is.Null);
        Assert.That(partnerFields[5].TreeDependency.IsLeaf, Is.True);
        Assert.That(partnerFields[5].TreeDependency.Items, Is.Empty);
        
        Assert.That(partnerFields[6].FieldName, Is.EqualTo("Category"));
        Assert.That(partnerFields[6].FieldType, Is.EqualTo(FieldType.ManyToOne));
        Assert.That(partnerFields[6].FirstOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["Category"]));
        Assert.That(partnerFields[6].LastOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][0].Fields["Category"]));
        Assert.That(partnerFields[6].AllOccurences, Has.Count.EqualTo(1));
        Assert.That(partnerFields[6].AllOccurences, Is.EquivalentTo(new[] { _aPlugin.Models["test_partner"][0].Fields["Category"] }));
        Assert.That(partnerFields[6].Name, Is.EqualTo("Category"));
        Assert.That(partnerFields[6].Description, Is.EqualTo("Partner's category"));
        Assert.That(partnerFields[6].DefaultComputedMethod, Is.Null);
        Assert.That(partnerFields[6].TreeDependency.IsLeaf, Is.True);
        Assert.That(partnerFields[6].TreeDependency.Items, Is.Empty);
        Assert.That(partnerFields[6].TargetField, Is.EqualTo("Partners"));
        Assert.That(partnerFields[6].OriginColumnName, Is.Null);
        Assert.That(partnerFields[6].TargetColumnName, Is.Null);
        
        // 7 & 8 are CreationDate & UpdateDate
        Assert.That(partnerFields[9].FieldName, Is.EqualTo("Test"));
        Assert.That(partnerFields[9].FieldType, Is.EqualTo(FieldType.Integer));
        Assert.That(partnerFields[9].FirstOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][1].Fields["Test"]));
        Assert.That(partnerFields[9].LastOccurence, Is.EqualTo(_aPlugin.Models["test_partner"][2].Fields["Test"]));
        Assert.That(partnerFields[9].AllOccurences, Has.Count.EqualTo(2));
        Assert.That(partnerFields[9].AllOccurences, Is.EquivalentTo(new[] { _aPlugin.Models["test_partner"][1].Fields["Test"], _aPlugin.Models["test_partner"][2].Fields["Test"] }));
        Assert.That(partnerFields[9].Name, Is.EqualTo("Test"));
        Assert.That(partnerFields[9].Description, Is.EqualTo("Test"));
        Assert.That(partnerFields[9].DefaultComputedMethod, Is.Not.Null);
        Assert.That(partnerFields[9].DefaultComputedMethod, Is.EqualTo(_aPlugin.Models["test_partner"][2].Fields["Test"].DefaultComputedMethod));
        Assert.That(partnerFields[9].TreeDependency.IsLeaf, Is.True);
        Assert.That(partnerFields[9].TreeDependency.Items, Is.Empty);

        
        FinalModel categoryModel = _pluginManager.GetFinalModel("test_category");
        Assert.That(categoryModel, Is.EqualTo(_pluginManager.Models.ToList()[1]));
        Assert.That(categoryModel.Name, Is.EqualTo("test_category"));
        Assert.That(categoryModel.FirstOccurence, Is.EqualTo(_aPlugin.Models["test_category"][0]));
        Assert.That(categoryModel.Description, Is.EqualTo("Contact Category"));
        Assert.That(categoryModel.AllOccurences, Has.Count.EqualTo(1));
        Assert.That(categoryModel.Fields, Has.Count.EqualTo(4));
        
        // TestCategory
        List<FinalField> categoryFields = categoryModel.Fields.Values.ToList();
        
        Assert.That(categoryFields[0].FieldName, Is.EqualTo("Name"));
        Assert.That(categoryFields[0].FieldType, Is.EqualTo(FieldType.String));
        Assert.That(categoryFields[0].FirstOccurence, Is.EqualTo(_aPlugin.Models["test_category"][0].Fields["Name"]));
        Assert.That(categoryFields[0].LastOccurence, Is.EqualTo(_aPlugin.Models["test_category"][0].Fields["Name"]));
        Assert.That(categoryFields[0].AllOccurences, Has.Count.EqualTo(1));
        Assert.That(categoryFields[0].AllOccurences, Is.EquivalentTo(new[] { _aPlugin.Models["test_category"][0].Fields["Name"] }));
        Assert.That(categoryFields[0].Name, Is.EqualTo("Name"));
        Assert.That(categoryFields[0].Description, Is.EqualTo("Name of the category"));
        Assert.That(categoryFields[0].DefaultComputedMethod, Is.Null);
        Assert.That(categoryFields[0].TreeDependency.IsLeaf, Is.True);
        Assert.That(categoryFields[0].TreeDependency.Items, Is.Empty);
        
        Assert.That(categoryFields[1].FieldName, Is.EqualTo("Partners"));
        Assert.That(categoryFields[1].FieldType, Is.EqualTo(FieldType.OneToMany));
        Assert.That(categoryFields[1].FirstOccurence, Is.EqualTo(_aPlugin.Models["test_category"][0].Fields["Partners"]));
        Assert.That(categoryFields[1].LastOccurence, Is.EqualTo(_aPlugin.Models["test_category"][0].Fields["Partners"]));
        Assert.That(categoryFields[1].AllOccurences, Has.Count.EqualTo(1));
        Assert.That(categoryFields[1].AllOccurences, Is.EquivalentTo(new[] { _aPlugin.Models["test_category"][0].Fields["Partners"] }));
        Assert.That(categoryFields[1].Name, Is.EqualTo("Partners"));
        Assert.That(categoryFields[1].Description, Is.EqualTo("Partners linked to this category"));
        Assert.That(categoryFields[1].DefaultComputedMethod, Is.Null);
        Assert.That(categoryFields[1].TreeDependency.IsLeaf, Is.True);
        Assert.That(categoryFields[1].TreeDependency.Items, Is.Empty);
        Assert.That(categoryFields[1].TargetField, Is.EqualTo("Category"));
        Assert.That(categoryFields[1].OriginColumnName, Is.Null);
        Assert.That(categoryFields[1].TargetColumnName, Is.Null);
    }
}
