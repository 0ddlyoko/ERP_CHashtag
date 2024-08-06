using System.Reflection;
using lib.field;
using lib.model;
using lib.plugin;
using Test.data.models;

namespace Test.plugin;

public class TestPluginManager
{
    private const int TotalNumberOfModels = 5;
    private const int TotalNumberOfModelOverride = 7;
    private Assembly _assembly;
    private PluginManager _pluginManager;
    private APlugin _aPlugin;
    private TestPlugin _plugin; 
    
    public TestPluginManager()
    {
        _assembly = typeof(TestPluginManager).Assembly;
        // _pluginManager = new("");
        _pluginManager.RegisterPlugin(_assembly);
        _aPlugin = _pluginManager.AvailablePlugins.First();
        _plugin = _aPlugin.Plugin as TestPlugin;
    }

    [Fact]
    public void TestPluginLoaded()
    {
        Assert.Equal(1, _pluginManager.AvailablePluginsSize);
        Assert.Equal(_plugin.Id, _aPlugin.Id);
        Assert.Equal(_plugin.Name, _aPlugin.Name);
        Assert.Equal(_plugin.Version, _aPlugin.Version);
        Assert.Empty(_aPlugin.Dependencies);
        Assert.Equal(TotalNumberOfModels, _aPlugin.Models.Count);
        Assert.False(_aPlugin.IsInstalled);
        Assert.Equal(APlugin.PluginState.NotInstalled, _aPlugin.State);
        Assert.Equal(0, _plugin.NumberOfOnStart);
        Assert.Equal(0, _plugin.NumberOfOnEnd);
        
        // Plugin is not installed
        Assert.Equal(0, _pluginManager.PluginsSize);
        Assert.Equal(0, _pluginManager.CommandsSize);
        Assert.Equal(0, _pluginManager.ModelsSize);
        Assert.Equal(0, _pluginManager.TotalModelsSize);
        Assert.Null(_pluginManager.GetInstalledPlugin("test"));
        Assert.False(_pluginManager.IsPluginInstalled("test"));

        // Install plugin
        _pluginManager.InstallPlugin(_aPlugin);
        
        // Plugin is installed
        Assert.True(_aPlugin.IsInstalled);
        Assert.Equal(APlugin.PluginState.Installed, _aPlugin.State);
        Assert.Equal(1, _plugin.NumberOfOnStart);
        Assert.Equal(0, _plugin.NumberOfOnEnd);
        
        Assert.Equal(1, _pluginManager.PluginsSize);
        Assert.Equal(0, _pluginManager.CommandsSize);
        Assert.Equal(TotalNumberOfModels, _pluginManager.ModelsSize);
        Assert.Equal(TotalNumberOfModelOverride, _pluginManager.TotalModelsSize);
        Assert.Equal(_aPlugin, _pluginManager.GetInstalledPlugin("test"));
        Assert.True(_pluginManager.IsPluginInstalled("test"));
        
        Assert.Equal(_aPlugin, _pluginManager.GetPlugin("test"));

        Assert.Equivalent(new[] { _aPlugin }, _pluginManager.PluginsInDependencyOrder);
    }

    [Fact]
    public void TestPluginModels()
    {
        Assert.Equal(TotalNumberOfModels, _aPlugin.Models.Count);
        Assert.Contains("test_partner", _aPlugin.Models.Keys);
        Assert.Contains("test_category", _aPlugin.Models.Keys);
        List<PluginModel> testPartnerModels = _aPlugin.Models["test_partner"];
        List<PluginModel> testCategoryModels = _aPlugin.Models["test_category"];
        Assert.Equal(3, testPartnerModels.Count);
        Assert.Single(testCategoryModels);
        
        
        // First model
        Assert.Equal(_aPlugin, testPartnerModels[0].Plugin);
        Assert.Equal("test_partner", testPartnerModels[0].Name);
        Assert.Equal("Contact", testPartnerModels[0].Description);
        Assert.Equal(10, testPartnerModels[0].Fields.Count);
        
        // Fields
        Assert.Contains("Name", testPartnerModels[0].Fields.Keys);
        Assert.Equal(_aPlugin, testPartnerModels[0].Fields["Name"].Plugin);
        Assert.Equal("Name", testPartnerModels[0].Fields["Name"].FieldName);
        Assert.Equal(FieldType.String, testPartnerModels[0].Fields["Name"].FieldType);
        Assert.Null(testPartnerModels[0].Fields["Name"].Name);
        Assert.Equal("Name of the partner", testPartnerModels[0].Fields["Name"].Description);
        
        var nameComputedMethod = testPartnerModels[0].Fields["Name"].DefaultComputedMethod;
        Assert.NotNull(nameComputedMethod);
        Assert.Equal("Name", nameComputedMethod.FieldName);
        Assert.Equal("Test", nameComputedMethod.DefaultValue);
        Assert.Null(nameComputedMethod.ComputedAttribute);
        Assert.Null(nameComputedMethod.MethodInfo);
        Assert.False(nameComputedMethod.IsComputedStatic);
        Assert.True(nameComputedMethod.IsPresent);
        
        var ageComputedMethod = testPartnerModels[0].Fields["Age"].DefaultComputedMethod;
        Assert.Contains("Age", testPartnerModels[0].Fields.Keys);
        Assert.Equal(_aPlugin, testPartnerModels[0].Fields["Age"].Plugin);
        Assert.Equal("Age", testPartnerModels[0].Fields["Age"].FieldName);
        Assert.Equal(FieldType.Integer, testPartnerModels[0].Fields["Age"].FieldType);
        Assert.Equal("Age 2", testPartnerModels[0].Fields["Age"].Name);
        Assert.Equal("Age of the partner", testPartnerModels[0].Fields["Age"].Description);
        Assert.NotNull(ageComputedMethod);
        Assert.Equal("Age", ageComputedMethod.FieldName);
        Assert.Equal(42, ageComputedMethod.DefaultValue);
        Assert.Null(ageComputedMethod.ComputedAttribute);
        Assert.Null(ageComputedMethod.MethodInfo);
        Assert.False(ageComputedMethod.IsComputedStatic);
        Assert.True(ageComputedMethod.IsPresent);
        
        var colorComputedMethod = testPartnerModels[0].Fields["Color"].DefaultComputedMethod;
        Assert.Contains("Color", testPartnerModels[0].Fields.Keys);
        Assert.Equal(_aPlugin, testPartnerModels[0].Fields["Color"].Plugin);
        Assert.Equal("Color", testPartnerModels[0].Fields["Color"].FieldName);
        Assert.Equal(FieldType.Integer, testPartnerModels[0].Fields["Color"].FieldType);
        Assert.Null(testPartnerModels[0].Fields["Color"].Name);
        Assert.Null(testPartnerModels[0].Fields["Color"].Description);
        Assert.NotNull(colorComputedMethod);
        Assert.Equal("Color", colorComputedMethod.FieldName);
        Assert.Equal("DefaultRandomColor", colorComputedMethod.DefaultValue);
        Assert.Null(colorComputedMethod.ComputedAttribute);
        Assert.NotNull(colorComputedMethod.MethodInfo);
        Assert.True(colorComputedMethod.IsComputedStatic);
        Assert.True(colorComputedMethod.IsPresent);
        
        var displayNameComputedMethod = testPartnerModels[0].Fields["DisplayName"].DefaultComputedMethod;
        Assert.Contains("DisplayName", testPartnerModels[0].Fields.Keys);
        Assert.Equal(_aPlugin, testPartnerModels[0].Fields["DisplayName"].Plugin);
        Assert.Equal("DisplayName", testPartnerModels[0].Fields["DisplayName"].FieldName);
        Assert.Equal(FieldType.String, testPartnerModels[0].Fields["DisplayName"].FieldType);
        Assert.Null(testPartnerModels[0].Fields["DisplayName"].Name);
        Assert.Equal("Name to display of the partner", testPartnerModels[0].Fields["DisplayName"].Description);
        Assert.NotNull(displayNameComputedMethod);
        Assert.Equal("DisplayName", displayNameComputedMethod.FieldName);
        Assert.Equal("ComputeDisplayName", displayNameComputedMethod.DefaultValue);
        Assert.NotNull(displayNameComputedMethod.ComputedAttribute);
        Assert.NotNull(displayNameComputedMethod.MethodInfo);
        Assert.False(displayNameComputedMethod.IsComputedStatic);
        Assert.True(displayNameComputedMethod.IsPresent);
        
        var myDateComputedMethod = testPartnerModels[0].Fields["MyDate"].DefaultComputedMethod;
        Assert.Contains("MyDate", testPartnerModels[0].Fields.Keys);
        Assert.Equal(_aPlugin, testPartnerModels[0].Fields["MyDate"].Plugin);
        Assert.Equal("MyDate", testPartnerModels[0].Fields["MyDate"].FieldName);
        Assert.Equal(FieldType.Date, testPartnerModels[0].Fields["MyDate"].FieldType);
        Assert.Equal("MyDate", testPartnerModels[0].Fields["MyDate"].Name);
        Assert.Equal("My Date", testPartnerModels[0].Fields["MyDate"].Description);
        Assert.NotNull(myDateComputedMethod);
        Assert.Equal("MyDate", myDateComputedMethod.FieldName);
        Assert.Equal("DefaultMyDate", myDateComputedMethod.DefaultValue);
        Assert.Null(myDateComputedMethod.ComputedAttribute);
        Assert.NotNull(myDateComputedMethod.MethodInfo);
        Assert.True(myDateComputedMethod.IsComputedStatic);
        Assert.True(myDateComputedMethod.IsPresent);
        
        Assert.Contains("MyDateTime", testPartnerModels[0].Fields.Keys);
        Assert.Equal(_aPlugin, testPartnerModels[0].Fields["MyDateTime"].Plugin);
        Assert.Equal("MyDateTime", testPartnerModels[0].Fields["MyDateTime"].FieldName);
        Assert.Equal(FieldType.Datetime, testPartnerModels[0].Fields["MyDateTime"].FieldType);
        Assert.Equal("MyTime", testPartnerModels[0].Fields["MyDateTime"].Name);
        Assert.Equal("My Date Time", testPartnerModels[0].Fields["MyDateTime"].Description);
        Assert.Null(testPartnerModels[0].Fields["MyDateTime"].DefaultComputedMethod);
        
        Assert.Contains("Category", testPartnerModels[0].Fields.Keys);
        Assert.Equal(_aPlugin, testPartnerModels[0].Fields["Category"].Plugin);
        Assert.Equal("Category", testPartnerModels[0].Fields["Category"].FieldName);
        Assert.Equal(FieldType.ManyToOne, testPartnerModels[0].Fields["Category"].FieldType);
        Assert.Equal("Category", testPartnerModels[0].Fields["Category"].Name);
        Assert.Equal("Partner's category", testPartnerModels[0].Fields["Category"].Description);
        Assert.Null(testPartnerModels[0].Fields["Category"].DefaultComputedMethod);

        // Second model
        Assert.Equal(_aPlugin, testPartnerModels[1].Plugin);
        Assert.Equal("test_partner", testPartnerModels[1].Name);
        Assert.Equal("Contact :D", testPartnerModels[1].Description);
        Assert.Equal(5, testPartnerModels[1].Fields.Count);
        
        // Fields
        nameComputedMethod = testPartnerModels[1].Fields["Name"].DefaultComputedMethod;
        Assert.Contains("Name", testPartnerModels[1].Fields.Keys);
        Assert.Equal(_aPlugin, testPartnerModels[1].Fields["Name"].Plugin);
        Assert.Equal("Name", testPartnerModels[1].Fields["Name"].FieldName);
        Assert.Equal(FieldType.String, testPartnerModels[1].Fields["Name"].FieldType);
        Assert.Null(testPartnerModels[1].Fields["Name"].Name);
        Assert.Equal("Not the name of the partner", testPartnerModels[1].Fields["Name"].Description);
        Assert.NotNull(nameComputedMethod);
        Assert.Equal("Name", nameComputedMethod.FieldName);
        Assert.Equal("LoL", nameComputedMethod.DefaultValue);
        Assert.Null(nameComputedMethod.ComputedAttribute);
        Assert.Null(nameComputedMethod.MethodInfo);
        Assert.False(nameComputedMethod.IsComputedStatic);
        Assert.True(nameComputedMethod.IsPresent);
        
        Assert.Contains("Test", testPartnerModels[1].Fields.Keys);
        Assert.Equal(_aPlugin, testPartnerModels[1].Fields["Test"].Plugin);
        Assert.Equal("Test", testPartnerModels[1].Fields["Test"].FieldName);
        Assert.Equal(FieldType.Integer, testPartnerModels[1].Fields["Test"].FieldType);
        Assert.Null(testPartnerModels[1].Fields["Test"].Name);
        Assert.Null(testPartnerModels[1].Fields["Test"].Description);
        Assert.Null(testPartnerModels[1].Fields["Test"].DefaultComputedMethod);

        // Third model
        Assert.Equal(_aPlugin, testPartnerModels[2].Plugin);
        Assert.Equal("test_partner", testPartnerModels[2].Name);
        Assert.Null(testPartnerModels[2].Description);
        Assert.Equal(5, testPartnerModels[2].Fields.Count);
        
        // Fields
        ageComputedMethod = testPartnerModels[2].Fields["Age"].DefaultComputedMethod;
        Assert.Contains("Age", testPartnerModels[2].Fields.Keys);
        Assert.Equal(_aPlugin, testPartnerModels[2].Fields["Age"].Plugin);
        Assert.Equal("Age", testPartnerModels[2].Fields["Age"].FieldName);
        Assert.Equal(FieldType.Integer, testPartnerModels[2].Fields["Age"].FieldType);
        Assert.Equal("Not his Age", testPartnerModels[2].Fields["Age"].Name);
        Assert.Equal("Age of him", testPartnerModels[2].Fields["Age"].Description);
        Assert.NotNull(ageComputedMethod);
        Assert.Equal("Age", ageComputedMethod.FieldName);
        Assert.Equal("DefaultAge", ageComputedMethod.DefaultValue);
        Assert.Null(ageComputedMethod.ComputedAttribute);
        Assert.NotNull(ageComputedMethod.MethodInfo);
        Assert.True(ageComputedMethod.IsComputedStatic);
        Assert.True(ageComputedMethod.IsPresent);
        
        var testComputedMethod = testPartnerModels[2].Fields["Test"].DefaultComputedMethod;
        Assert.Contains("Test", testPartnerModels[2].Fields.Keys);
        Assert.Equal(_aPlugin, testPartnerModels[2].Fields["Test"].Plugin);
        Assert.Equal("Test", testPartnerModels[2].Fields["Test"].FieldName);
        Assert.Equal(FieldType.Integer, testPartnerModels[2].Fields["Test"].FieldType);
        Assert.Null(testPartnerModels[2].Fields["Test"].Name);
        Assert.Null(testPartnerModels[2].Fields["Test"].Description);
        Assert.NotNull(testComputedMethod);
        Assert.Equal("Test", testComputedMethod.FieldName);
        Assert.Equal(30, testComputedMethod.DefaultValue);
        Assert.Null(testComputedMethod.ComputedAttribute);
        Assert.Null(testComputedMethod.MethodInfo);
        Assert.False(testComputedMethod.IsComputedStatic);
        Assert.True(testComputedMethod.IsPresent);

        // Category model
        Assert.Equal(_aPlugin, testCategoryModels[0].Plugin);
        Assert.Equal("test_category", testCategoryModels[0].Name);
        Assert.Equal("Contact Category", testCategoryModels[0].Description);
        Assert.Equal(5, testCategoryModels[0].Fields.Count);
        
        // Fields
        Assert.Contains("Name", testCategoryModels[0].Fields.Keys);
        Assert.Equal(_aPlugin, testCategoryModels[0].Fields["Name"].Plugin);
        Assert.Equal("Name", testCategoryModels[0].Fields["Name"].FieldName);
        Assert.Equal(FieldType.String, testCategoryModels[0].Fields["Name"].FieldType);
        Assert.Null(testCategoryModels[0].Fields["Name"].Name);
        Assert.Equal("Name of the category", testCategoryModels[0].Fields["Name"].Description);
        Assert.Null(testCategoryModels[0].Fields["Name"].DefaultComputedMethod);
        
        Assert.Contains("Partners", testCategoryModels[0].Fields.Keys);
        Assert.Equal(_aPlugin, testCategoryModels[0].Fields["Partners"].Plugin);
        Assert.Equal("Partners", testCategoryModels[0].Fields["Partners"].FieldName);
        Assert.Equal(FieldType.OneToMany, testCategoryModels[0].Fields["Partners"].FieldType);
        Assert.Null(testCategoryModels[0].Fields["Partners"].Name);
        Assert.Equal("Partners linked to this category", testCategoryModels[0].Fields["Partners"].Description);
        Assert.Null(testCategoryModels[0].Fields["Partners"].DefaultComputedMethod);
        Assert.Equal(typeof(TestPartner), testCategoryModels[0].Fields["Partners"].Type);
        Assert.Equal("Category", testCategoryModels[0].Fields["Partners"].TargetField);
        Assert.Null(testCategoryModels[0].Fields["Partners"].OriginColumnName);
        Assert.Null(testCategoryModels[0].Fields["Partners"].TargetColumnName);
    }

    [Fact]
    public void TestModelMerges()
    {
        // Install plugin
        _pluginManager.InstallPlugin(_aPlugin);
        
        Assert.Equal(TotalNumberOfModels, _pluginManager.ModelsSize);
        Assert.Equal(TotalNumberOfModelOverride, _pluginManager.TotalModelsSize);

        FinalModel partnerModel = _pluginManager.GetFinalModel("test_partner");
        Assert.Equal(_pluginManager.Models.ToList()[0], partnerModel);
        Assert.Equal("test_partner", partnerModel.Name);
        Assert.Equal(_aPlugin.Models["test_partner"][0], partnerModel.FirstOccurence);
        Assert.Equal("Contact :D", partnerModel.Description);
        Assert.Equal(3, partnerModel.AllOccurences.Count);
        Assert.Equal(11, partnerModel.Fields.Count);
        
        // TestPartner
        List<FinalField> partnerFields = partnerModel.Fields.Values.ToList();
        
        Assert.Equal("Name", partnerFields[0].FieldName);
        Assert.Equal(FieldType.String, partnerFields[0].FieldType);
        Assert.Equal(_aPlugin.Models["test_partner"][0].Fields["Name"], partnerFields[0].FirstOccurence);
        Assert.Equal(_aPlugin.Models["test_partner"][1].Fields["Name"], partnerFields[0].LastOccurence);
        Assert.Equal(2, partnerFields[0].AllOccurences.Count);
        Assert.Equivalent(new[] { _aPlugin.Models["test_partner"][0].Fields["Name"], _aPlugin.Models["test_partner"][1].Fields["Name"] }, partnerFields[0].AllOccurences);
        Assert.Equal("Name", partnerFields[0].Name);
        Assert.Equal("Not the name of the partner", partnerFields[0].Description);
        Assert.NotNull(partnerFields[0].DefaultComputedMethod);
        Assert.Equal(_aPlugin.Models["test_partner"][1].Fields["Name"].DefaultComputedMethod, partnerFields[0].DefaultComputedMethod);
        Assert.Equal(partnerFields[0], partnerFields[0].TreeDependency.Root);
        Assert.False(partnerFields[0].TreeDependency.IsLeaf);
        Assert.Contains("test_partner.DisplayName", partnerFields[0].TreeDependency.Items.Keys);
        Assert.Contains(new TreeNode(partnerFields[3], true), partnerFields[0].TreeDependency.Items.Values);

        Assert.Equal("Age", partnerFields[1].FieldName);
        Assert.Equal(FieldType.Integer, partnerFields[1].FieldType);
        Assert.Equal(_aPlugin.Models["test_partner"][0].Fields["Age"], partnerFields[1].FirstOccurence);
        Assert.Equal(_aPlugin.Models["test_partner"][2].Fields["Age"], partnerFields[1].LastOccurence);
        Assert.Equal(2, partnerFields[1].AllOccurences.Count);
        Assert.Equivalent(new[] { _aPlugin.Models["test_partner"][0].Fields["Age"], _aPlugin.Models["test_partner"][2].Fields["Age"] }, partnerFields[1].AllOccurences);
        Assert.Equal("Not his Age", partnerFields[1].Name);
        Assert.Equal("Age of him", partnerFields[1].Description);
        Assert.NotNull(partnerFields[1].DefaultComputedMethod);
        Assert.Equal(_aPlugin.Models["test_partner"][2].Fields["Age"].DefaultComputedMethod, partnerFields[1].DefaultComputedMethod);
        Assert.Equal(partnerFields[1], partnerFields[1].TreeDependency.Root);
        Assert.False(partnerFields[1].TreeDependency.IsLeaf);
        Assert.Contains("test_partner.DisplayName", partnerFields[1].TreeDependency.Items.Keys);
        Assert.Contains(new TreeNode(partnerFields[3], true), partnerFields[1].TreeDependency.Items.Values);
        
        Assert.Equal("Color", partnerFields[2].FieldName);
        Assert.Equal(FieldType.Integer, partnerFields[2].FieldType);
        Assert.Equal(_aPlugin.Models["test_partner"][0].Fields["Color"], partnerFields[2].FirstOccurence);
        Assert.Equal(_aPlugin.Models["test_partner"][0].Fields["Color"], partnerFields[2].LastOccurence);
        Assert.Single(partnerFields[2].AllOccurences);
        Assert.Equivalent(new[] { _aPlugin.Models["test_partner"][0].Fields["Color"] }, partnerFields[2].AllOccurences);
        Assert.Equal("Color", partnerFields[2].Name);
        Assert.Equal("Color", partnerFields[2].Description);
        Assert.NotNull(partnerFields[2].DefaultComputedMethod);
        Assert.Equal(_aPlugin.Models["test_partner"][0].Fields["Color"].DefaultComputedMethod, partnerFields[2].DefaultComputedMethod);
        Assert.Equal(partnerFields[2], partnerFields[2].TreeDependency.Root);
        Assert.True(partnerFields[2].TreeDependency.IsLeaf);
        Assert.Empty(partnerFields[2].TreeDependency.Items);
        
        Assert.Equal("DisplayName", partnerFields[3].FieldName);
        Assert.Equal(FieldType.String, partnerFields[3].FieldType);
        Assert.Equal(_aPlugin.Models["test_partner"][0].Fields["DisplayName"], partnerFields[3].FirstOccurence);
        Assert.Equal(_aPlugin.Models["test_partner"][0].Fields["DisplayName"], partnerFields[3].LastOccurence);
        Assert.Single(partnerFields[3].AllOccurences);
        Assert.Equivalent(new[] { _aPlugin.Models["test_partner"][0].Fields["DisplayName"] }, partnerFields[3].AllOccurences);
        Assert.Equal("DisplayName", partnerFields[3].Name);
        Assert.Equal("Name to display of the partner", partnerFields[3].Description);
        Assert.NotNull(partnerFields[3].DefaultComputedMethod);
        Assert.Equal(_aPlugin.Models["test_partner"][0].Fields["DisplayName"].DefaultComputedMethod, partnerFields[3].DefaultComputedMethod);
        Assert.True(partnerFields[3].TreeDependency.IsLeaf);
        Assert.Empty(partnerFields[3].TreeDependency.Items);
        
        Assert.Equal("MyDate", partnerFields[4].FieldName);
        Assert.Equal(FieldType.Date, partnerFields[4].FieldType);
        Assert.Equal(_aPlugin.Models["test_partner"][0].Fields["MyDate"], partnerFields[4].FirstOccurence);
        Assert.Equal(_aPlugin.Models["test_partner"][0].Fields["MyDate"], partnerFields[4].LastOccurence);
        Assert.Single(partnerFields[4].AllOccurences);
        Assert.Equivalent(new[] { _aPlugin.Models["test_partner"][0].Fields["MyDate"] }, partnerFields[4].AllOccurences);
        Assert.Equal("MyDate", partnerFields[4].Name);
        Assert.Equal("My Date", partnerFields[4].Description);
        Assert.Equal(_aPlugin.Models["test_partner"][0].Fields["MyDate"].DefaultComputedMethod, partnerFields[4].DefaultComputedMethod);
        Assert.True(partnerFields[4].TreeDependency.IsLeaf);
        Assert.Empty(partnerFields[4].TreeDependency.Items);
        
        Assert.Equal("MyDateTime", partnerFields[5].FieldName);
        Assert.Equal(FieldType.Datetime, partnerFields[5].FieldType);
        Assert.Equal(_aPlugin.Models["test_partner"][0].Fields["MyDateTime"], partnerFields[5].FirstOccurence);
        Assert.Equal(_aPlugin.Models["test_partner"][0].Fields["MyDateTime"], partnerFields[5].LastOccurence);
        Assert.Single(partnerFields[5].AllOccurences);
        Assert.Equivalent(new[] { _aPlugin.Models["test_partner"][0].Fields["MyDateTime"] }, partnerFields[5].AllOccurences);
        Assert.Equal("MyTime", partnerFields[5].Name);
        Assert.Equal("My Date Time", partnerFields[5].Description);
        Assert.Null(partnerFields[5].DefaultComputedMethod);
        Assert.True(partnerFields[5].TreeDependency.IsLeaf);
        Assert.Empty(partnerFields[5].TreeDependency.Items);
        
        Assert.Equal("Category", partnerFields[6].FieldName);
        Assert.Equal(FieldType.ManyToOne, partnerFields[6].FieldType);
        Assert.Equal(_aPlugin.Models["test_partner"][0].Fields["Category"], partnerFields[6].FirstOccurence);
        Assert.Equal(_aPlugin.Models["test_partner"][0].Fields["Category"], partnerFields[6].LastOccurence);
        Assert.Single(partnerFields[6].AllOccurences);
        Assert.Equivalent(new[] { _aPlugin.Models["test_partner"][0].Fields["Category"] }, partnerFields[6].AllOccurences);
        Assert.Equal("Category", partnerFields[6].Name);
        Assert.Equal("Partner's category", partnerFields[6].Description);
        Assert.Null(partnerFields[6].DefaultComputedMethod);
        Assert.True(partnerFields[6].TreeDependency.IsLeaf);
        Assert.Empty(partnerFields[6].TreeDependency.Items);
        Assert.Equal("Partners", partnerFields[6].TargetField);
        Assert.Null(partnerFields[6].OriginColumnName);
        Assert.Null(partnerFields[6].TargetColumnName);
        
        // 7, 8 & 9 are Id, CreationDate, UpdateDate
        Assert.Equal("Test", partnerFields[10].FieldName);
        Assert.Equal(FieldType.Integer, partnerFields[10].FieldType);
        Assert.Equal(_aPlugin.Models["test_partner"][1].Fields["Test"], partnerFields[10].FirstOccurence);
        Assert.Equal(_aPlugin.Models["test_partner"][2].Fields["Test"], partnerFields[10].LastOccurence);
        Assert.Equal(2, partnerFields[10].AllOccurences.Count);
        Assert.Equivalent(new[] { _aPlugin.Models["test_partner"][1].Fields["Test"], _aPlugin.Models["test_partner"][2].Fields["Test"] }, partnerFields[10].AllOccurences);
        Assert.Equal("Test", partnerFields[10].Name);
        Assert.Equal("Test", partnerFields[10].Description);
        Assert.NotNull(partnerFields[10].DefaultComputedMethod);
        Assert.Equal(_aPlugin.Models["test_partner"][2].Fields["Test"].DefaultComputedMethod, partnerFields[10].DefaultComputedMethod);
        Assert.True(partnerFields[10].TreeDependency.IsLeaf);
        Assert.Empty(partnerFields[10].TreeDependency.Items);

        
        FinalModel categoryModel = _pluginManager.GetFinalModel("test_category");
        Assert.Equal(_pluginManager.Models.ToList()[1], categoryModel);
        Assert.Equal("test_category", categoryModel.Name);
        Assert.Equal(_aPlugin.Models["test_category"][0], categoryModel.FirstOccurence);
        Assert.Equal("Contact Category", categoryModel.Description);
        Assert.Single(categoryModel.AllOccurences);
        Assert.Equal(5, categoryModel.Fields.Count);
        
        // TestCategory
        List<FinalField> categoryFields = categoryModel.Fields.Values.ToList();
        
        Assert.Equal("Name", categoryFields[0].FieldName);
        Assert.Equal(FieldType.String, categoryFields[0].FieldType);
        Assert.Equal(_aPlugin.Models["test_category"][0].Fields["Name"], categoryFields[0].FirstOccurence);
        Assert.Equal(_aPlugin.Models["test_category"][0].Fields["Name"], categoryFields[0].LastOccurence);
        Assert.Single(categoryFields[0].AllOccurences);
        Assert.Equivalent(new[] { _aPlugin.Models["test_category"][0].Fields["Name"] }, categoryFields[0].AllOccurences);
        Assert.Equal("Name", categoryFields[0].Name);
        Assert.Equal("Name of the category", categoryFields[0].Description);
        Assert.Null(categoryFields[0].DefaultComputedMethod);
        Assert.True(categoryFields[0].TreeDependency.IsLeaf);
        Assert.Empty(categoryFields[0].TreeDependency.Items);
        
        Assert.Equal("Partners", categoryFields[1].FieldName);
        Assert.Equal(FieldType.OneToMany, categoryFields[1].FieldType);
        Assert.Equal(_aPlugin.Models["test_category"][0].Fields["Partners"], categoryFields[1].FirstOccurence);
        Assert.Equal(_aPlugin.Models["test_category"][0].Fields["Partners"], categoryFields[1].LastOccurence);
        Assert.Single(categoryFields[1].AllOccurences);
        Assert.Equivalent(new[] { _aPlugin.Models["test_category"][0].Fields["Partners"] }, categoryFields[1].AllOccurences);
        Assert.Equal("Partners", categoryFields[1].Name);
        Assert.Equal("Partners linked to this category", categoryFields[1].Description);
        Assert.Null(categoryFields[1].DefaultComputedMethod);
        Assert.True(categoryFields[1].TreeDependency.IsLeaf);
        Assert.Empty(categoryFields[1].TreeDependency.Items);
        Assert.Equal("Category", categoryFields[1].TargetField);
        Assert.Null(categoryFields[1].OriginColumnName);
        Assert.Null(categoryFields[1].TargetColumnName);
    }
}
