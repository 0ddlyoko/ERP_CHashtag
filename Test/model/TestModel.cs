using System.Reflection;
using lib;
using lib.cache;
using lib.field;
using lib.model;
using lib.plugin;
using Test.data.models;
using Environment = lib.Environment;

namespace Test.model;

[TestFixture]
public class TestModel
{
    private Assembly _assembly;
    private PluginManager _pluginManager;
    private APlugin _aPlugin;
    private TestPlugin _plugin;
    private Environment _env;

    [SetUp]
    public void Setup()
    {
        _assembly = typeof(TestModel).Assembly;
        // _pluginManager = new("");
        _pluginManager.RegisterPlugin(_assembly);
        _aPlugin = _pluginManager.AvailablePlugins.First();
        _pluginManager.InstallPlugin(_aPlugin);
        _plugin = _aPlugin.Plugin as TestPlugin;
        _env = new Environment(_pluginManager);
    }

    [Test]
    public void TestCreateAndGetModel()
    {
        Assert.Throws<InvalidOperationException>(() => _env.Create<TestPartner>([]), "No value given to create method");
        
        TestPartner partner = _env.Create<TestPartner>([[]]);
        Assert.That(partner, Is.Not.Null);
        Assert.That(partner.Id, Is.EqualTo(1));
        Assert.That(partner.Env, Is.Not.Null);
        
        TestPartner partner2 = _env.Create<TestPartner>([[]]);
        Assert.That(partner2, Is.Not.Null);
        Assert.That(partner2.Id, Is.EqualTo(2));
        Assert.That(partner2.Env, Is.Not.Null);
        
        Assert.That(_env.Get<TestPartner>(1).Id, Is.EqualTo(1));
        TestPartner partner3 = _env.Get<TestPartner>(3);
        Assert.That(partner3, Is.Not.Null, "Retrieving a record that does not exist do not throw an error");
    }

    [Test]
    public void TestUpdate()
    {
        TestPartner partner = _env.Create<TestPartner>([[]]);
        TestPartner2 partner2 = partner.Transform<TestPartner2>();
        
        partner.Update(new Dictionary<string, object?>
        {
            {"Name", "0ddlyoko"},
            {"Age", 54},
        });
        
        Assert.That(partner.Name, Is.EqualTo("0ddlyoko"), "Name should be 0ddlyoko");
        Assert.That(partner.Age, Is.EqualTo(54), "Age should be 54");
        Assert.That(partner2.Name, Is.EqualTo("0ddlyoko"), "Name should be 0ddlyoko");

        partner.Name = "1ddlyoko";
        partner.Update(new Dictionary<string, object?>
        {
            {"Age", 60},
        });
        Assert.That(partner.Name, Is.EqualTo("1ddlyoko"), "Entering a value then calling Update should work");
        Assert.That(partner.Age, Is.EqualTo(60), "Age should be 54");
        Assert.That(partner2.Name, Is.EqualTo("1ddlyoko"), "Entering a value then calling Update should work");
    }

    [Test]
    public void TestDefaultValue()
    {
        TestPartner partner = _env.Create<TestPartner>([[]]);
        TestPartner2 partner2 = partner.Transform<TestPartner2>();
        TestPartner3 partner3 = partner.Transform<TestPartner3>();
        
        Assert.That(partner.Name, Is.EqualTo("LoL"), "Name is overriden by TestPartner2");
        Assert.That(partner2.Name, Is.EqualTo("LoL"), "Name is overriden by TestPartner2");
        
        Assert.That(partner.Age, Is.EqualTo(70), "Age is overriden by TestPartner3");
        Assert.That(partner3.Age, Is.EqualTo(70), "Age is overriden by TestPartner3");
        
        Assert.That(partner2.Test, Is.EqualTo(30), "Test is overriden by TestPartner3");
        Assert.That(partner3.Test, Is.EqualTo(30), "Test is overriden by TestPartner3");

        TestPartner newPartner = _env.Create<TestPartner>([new Dictionary<string, object?>
        {
            {"Age", 100},
        }]);
    
        Assert.That(newPartner.Age, Is.EqualTo(100), "Default value should prioritize given values");}

    [Test]
    public void TestCompute()
    {
        TestPartner partner = _env.Create<TestPartner>([[]]);
        FinalModel finalModel = _env.PluginManager.GetFinalModel(partner.ModelName);
        CachedModel cachedModel = _env.Cache.CachedModels[partner.ModelName][partner.Id];
        TestPartner2 partner2 = partner.Transform<TestPartner2>();
        TestPartner3 partner3 = partner.Transform<TestPartner3>();
        Assert.Multiple(() =>
        {
            // DisplayName shouldn't be already computed
            Assert.That(cachedModel.Fields["DisplayName"].Value, Is.Null);
            Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["DisplayName"], partner.Id), Is.True);
            // Assert.That(cachedModel.Fields["DisplayName"].Dirty, Is.True);
            // Assert.That(cachedModel.Dirty, Is.True);
        });

        // Now that we access to DisplayName, it should be computed
        Assert.That(partner.DisplayName, Is.EqualTo("Name: LoL, Age: 70"));
        Assert.Multiple(() =>
        {
            Assert.That(cachedModel.Fields["DisplayName"].Value, Is.EqualTo("Name: LoL, Age: 70"));
            Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["DisplayName"], partner.Id), Is.False);
            // Assert.That(cachedModel.Fields["DisplayName"].Dirty, Is.True);
            // Assert.That(cachedModel.Dirty, Is.True);
        });
        
        partner.Name = "0ddlyoko";
        partner.Age = 42;
        Assert.Multiple(() =>
        {
            // Modifying fields should not trigger the compute, but should set the flag to true
            Assert.That(cachedModel.Fields["DisplayName"].Value, Is.EqualTo("Name: LoL, Age: 70"));
            Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["DisplayName"], partner.Id), Is.True);
            // Assert.That(cachedModel.Fields["DisplayName"].Dirty, Is.True);
            // Assert.That(cachedModel.Dirty, Is.True);
        });
        Assert.Multiple(() =>
        {
            // Accessing again to DisplayName should compute it as ToRecompute = true
            Assert.That(partner.DisplayName, Is.EqualTo("Name: 0ddlyoko, Age: 42"), "We should recompute the method");
            Assert.That(cachedModel.Fields["DisplayName"].Value, Is.EqualTo("Name: 0ddlyoko, Age: 42"));
            Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["DisplayName"], partner.Id), Is.False);
            // Assert.That(cachedModel.Fields["DisplayName"].Dirty, Is.True);
            // Assert.That(cachedModel.Dirty, Is.True);
        });
        partner2.Name = "1ddlyoko";
        Assert.That(partner.DisplayName, Is.EqualTo("Name: 1ddlyoko, Age: 42"), "Modifying a field from a child model should recompute the method");
        
        partner3.Update(new Dictionary<string, object?>
        {
            {"Age", 54},
        });
        Assert.That(partner.DisplayName, Is.EqualTo("Name: 1ddlyoko, Age: 54"), "Modifying a field from a child model should recompute the method");
    }

    [Test]
    public void TestRecomputeNotDoneIfFieldIsSet()
    {
        TestPartner partner = _env.Create<TestPartner>([[]]);
        FinalModel finalModel = _env.PluginManager.GetFinalModel(partner.ModelName);

        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["DisplayName"], partner.Id), Is.True);
        partner.Name = "Test";
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["DisplayName"], partner.Id), Is.True);
        partner.DisplayName = "My Own display name";
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["DisplayName"], partner.Id), Is.False);
    }

    [Test]
    public void TestRecomputeNotDoneIfUpdateIsCalledWithComputedField()
    {
        TestPartner partner = _env.Create<TestPartner>([[]]);
        FinalModel finalModel = _env.PluginManager.GetFinalModel(partner.ModelName);

        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["DisplayName"], partner.Id), Is.True);
        partner.Update(new Dictionary<string, object?>
        {
            {"Name", "0ddlyoko"},
            {"DisplayName", "My Own Display Name"},
            {"Age", 54},
        });
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["DisplayName"], partner.Id), Is.False);
    }

    [Test]
    public void TestUpdateDate()
    {
        DateTime fakeTime = new DateTime(1998, 7, 21);
        TestPartner partner;
        using (new DateTimeProvider.DateTimeProviderContext(fakeTime))
        {
            partner = _env.Create<TestPartner>([[]]);
            Assert.That(partner.CreationDate, Is.EqualTo(fakeTime));
            Assert.That(partner.UpdateDate, Is.EqualTo(fakeTime));
        }

        TestPartner2 partner2 = partner.Transform<TestPartner2>();
        DateTime fakeTime2 = new DateTime(1998, 7, 22);
        using (new DateTimeProvider.DateTimeProviderContext(fakeTime2))
        {
            partner.Name = "0ddlyoko";
            Assert.That(partner.CreationDate, Is.EqualTo(fakeTime), "Creation date shouldn't change at all");
            Assert.That(partner.UpdateDate, Is.EqualTo(fakeTime2), "Update date should have changed");
            Assert.That(partner2.CreationDate, Is.EqualTo(fakeTime), "Creation date shouldn't change at all");
            Assert.That(partner2.UpdateDate, Is.EqualTo(fakeTime2), "Update date should have changed");
        }
    }

    [Test]
    public void TestDate()
    {
        DateTime fakeTime = new DateTime(1998, 7, 21, 12, 0, 0);
        DateTime fakeTime2 = new DateTime(1998, 7, 21, 13, 0, 0);
        DateTime fakeDate = fakeTime.Date;
        using (new DateTimeProvider.DateTimeProviderContext(fakeTime))
        {
            TestPartner partner = _env.Create<TestPartner>([[]]);
            Assert.That(partner.MyDate, Is.EqualTo(fakeDate), "Date field should always be a date");

            partner.Update(new Dictionary<string, object?>
            {
                {"MyDate", fakeTime},
                {"MyDateTime", fakeTime},
            });
            
            Assert.That(partner.MyDate, Is.EqualTo(fakeDate), "Saving should update the datetime into a date");
            Assert.That(partner.MyDateTime, Is.EqualTo(fakeTime), "This datetime shouldn't change as it's not a date");

            partner.Update(new Dictionary<string, object?>
            {
                {"MyDate", fakeTime2},
                {"MyDateTime", fakeTime2},
            });
            
            Assert.That(partner.MyDate, Is.EqualTo(fakeDate), "Saving should update the datetime into a date");
            Assert.That(partner.MyDateTime, Is.EqualTo(fakeTime2), "This datetime shouldn't change as it's not a date");
        }
    }

    [Test]
    public void TestMultiplePartner()
    {
        TestPartner partner = _env.Create<TestPartner>([[], []]);
        Assert.That(partner.Ids, Has.Count.EqualTo(2));
        Assert.Throws<InvalidOperationException>(() => { _ = partner.Name; }, "Retrieving a field from a recordset should throw an exception");
        
        TestPartner2 partner2 = partner.Transform<TestPartner2>();
        Assert.That(partner2.Ids, Has.Count.EqualTo(2));

        TestPartner partnerWithOnlyFirstId = _env.Get<TestPartner>(partner.Ids[0]);
        partner.Name = "New Name";
        Assert.That(partnerWithOnlyFirstId.Name, Is.EqualTo("New Name"));
    }

    [Test]
    public void TestPartnerToCategory()
    {
        TestPartner partner = _env.Create<TestPartner>([[]]);
        TestCategory category = _env.Create<TestCategory>([[]]);
        CachedModel cachedPartnerModel = _env.Cache.CachedModels[partner.ModelName][partner.Id];
        CachedModel cachedCategoryModel = _env.Cache.CachedModels[category.ModelName][category.Id];

        Assert.That(cachedPartnerModel.Fields, Contains.Key("Category"));
        Assert.That(cachedPartnerModel.Fields["Category"].Value, Is.Null);
        
        Assert.That(cachedCategoryModel.Fields, Contains.Key("Partners"));
        Assert.That(cachedCategoryModel.Fields["Partners"].Value, Is.Not.Null, "A OneToMany field shouldn't have a null value in the cache");
        Assert.That(cachedCategoryModel.Fields["Partners"].Value, Is.TypeOf<List<int>>(), "A OneToMany field should have a list of int in the cache");
        
        partner.Category = category;
        
        Assert.That(cachedPartnerModel.Fields["Category"].Value, Is.Not.Null);
        Assert.That(cachedPartnerModel.Fields["Category"].Value, Is.EqualTo(category.Id));
        Assert.That(cachedCategoryModel.Fields["Partners"].Value, Is.TypeOf<List<int>>());
        Assert.That(cachedCategoryModel.Fields["Partners"].Value, Is.EqualTo(new List<int> { partner.Id }));

        Assert.That(category.Partners, Is.Not.Null, "Setting partner's category should be reflected in the category as it's a OneToMany");
        Assert.That(category.Partners.Ids, Has.Count.EqualTo(1), "Setting partner's category should be reflected in the category as it's a OneToMany");
        Assert.That(category.Partners.Ids[0], Is.EqualTo(partner.Id), "Setting partner's category should be reflected in the category as it's a OneToMany");
        
        // Remove the category
        partner.Category = null;
        
        Assert.That(cachedPartnerModel.Fields["Category"].Value, Is.Null);
        Assert.That(cachedCategoryModel.Fields["Partners"].Value, Is.TypeOf<List<int>>());
        Assert.That(cachedCategoryModel.Fields["Partners"].Value, Is.Empty);
        
        // Now, check if we can modify it from the category
        category.Partners = partner;
        
        Assert.That(cachedPartnerModel.Fields["Category"].Value, Is.Not.Null);
        Assert.That(cachedPartnerModel.Fields["Category"].Value, Is.EqualTo(category.Id));
        Assert.That(cachedCategoryModel.Fields["Partners"].Value, Is.TypeOf<List<int>>());
        Assert.That(cachedCategoryModel.Fields["Partners"].Value, Is.EqualTo(new[] { partner.Id }));

        Assert.That(category.Partners, Is.Not.Null, "Setting partner's category should be reflected in the category as it's a OneToMany");
        Assert.That(category.Partners.Ids, Has.Count.EqualTo(1), "Setting partner's category should be reflected in the category as it's a OneToMany");
        Assert.That(category.Partners.Ids[0], Is.EqualTo(partner.Id), "Setting partner's category should be reflected in the category as it's a OneToMany");
    }

    [Test]
    public void TestMultipleRecompute()
    {
        TestMultipleRecompute model = _env.Create<TestMultipleRecompute>([[]]);
        FinalModel finalModel = _env.PluginManager.GetFinalModel(model.ModelName);
        
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name"], model.Id), Is.False);
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name2"], model.Id), Is.True);
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name3"], model.Id), Is.True);
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name4"], model.Id), Is.True);
        
        Assert.That(model.Name, Is.EqualTo("0ddlyoko"));
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name"], model.Id), Is.False);
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name2"], model.Id), Is.True);
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name3"], model.Id), Is.True);
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name4"], model.Id), Is.True);
        
        Assert.That(model.Name4, Is.EqualTo("0ddlyoko-2-3-4"));
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name"], model.Id), Is.False);
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name2"], model.Id), Is.False);
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name3"], model.Id), Is.False);
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name4"], model.Id), Is.False);
        
        model.Name2 = "1ddlyoko";
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name"], model.Id), Is.False);
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name2"], model.Id), Is.False);
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name3"], model.Id), Is.True);
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name4"], model.Id), Is.True);
        
        Assert.That(model.Name4, Is.EqualTo("1ddlyoko-3-4"));
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name"], model.Id), Is.False);
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name2"], model.Id), Is.False);
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name3"], model.Id), Is.False);
        Assert.That(_env.Cache.IsToRecompute(finalModel.Fields["Name4"], model.Id), Is.False);
        
        model.Name = "test";
        Assert.That(model.Name4, Is.EqualTo("test-2-3-4"));
    }

    [Test]
    public void TestX2XCompute()
    {
        // TODO FIXME
        TestMultipleRecompute model = _env.Create<TestMultipleRecompute>([[]]);
        TestMultipleRecompute model2 = _env.Create<TestMultipleRecompute>([[]]);
        TestMultipleRecompute model3 = _env.Create<TestMultipleRecompute>([[]]);
        
        Assert.That(model.Multi.Ids, Is.Empty);
        model.Single = model2;
        
        Assert.That(model.Single, Is.EqualTo(model2));
        Assert.That(model.Single2, Is.EqualTo(model2));
        Assert.That(model2.Multi, Is.EqualTo(model));

        model.Single = model3;
        // Assert.That(_env.Cache.IsToRecompute(_env.PluginManager.GetFinalModel(model3.ModelName).Fields["Single2"], model.Id), Is.False, "This field should have been computed");
        Assert.That(_env.Cache.IsToRecompute(_env.PluginManager.GetFinalModel(model3.ModelName).Fields["Single3"], model.Id), Is.True);
        Assert.That(_env.Cache.IsToRecompute(_env.PluginManager.GetFinalModel(model3.ModelName).Fields["Single2"], model2.Id), Is.False, "This field should have been computed");
        Assert.That(_env.Cache.IsToRecompute(_env.PluginManager.GetFinalModel(model3.ModelName).Fields["Single3"], model2.Id), Is.True);
        // TODO Next line is failing but should work
        // Assert.That(_env.Cache.IsToRecompute(_env.PluginManager.GetFinalModel(model3.ModelName).Fields["Multi"], model3.Id), Is.True);
        Assert.That(model2.Multi.Ids, Is.Empty);
        Assert.That(model3.Multi, Is.EqualTo(model));

        model.Single = model3;
        // Assert.That(model3.Multi.Ids, Is.Empty);
    }

    [Test]
    public void TestMultipleRecomputeShouldHaveCorrectAllOccurrences()
    {
        FinalModel finalModel = _pluginManager.GetFinalModel("test_multiple_recompute");
        FinalModel finalModel2 = _pluginManager.GetFinalModel("test_model_2");
        
        Assert.That(finalModel.Fields["Name"].TreeDependency.Items, Is.EquivalentTo(new Dictionary<string, TreeNode> {
            {"test_multiple_recompute.Name2", new TreeNode(finalModel.Fields["Name2"], true)},
            {"test_model_2.AnotherModel", new TreeNode(finalModel2.Fields["AnotherModel"], false)},
        }));
        Assert.That(finalModel.Fields["Name2"].TreeDependency.Items, Is.EquivalentTo(new Dictionary<string, TreeNode>
        {
            {"test_multiple_recompute.Name3", new TreeNode(finalModel.Fields["Name3"], true)},
        }));
        Assert.That(finalModel.Fields["Name3"].TreeDependency.Items, Is.EquivalentTo(new Dictionary<string, TreeNode>
        {
            {"test_multiple_recompute.Name4", new TreeNode(finalModel.Fields["Name4"], true)},
        }));
        Assert.That(finalModel.Fields["Name4"].TreeDependency.Items, Is.Empty);
        Assert.That(finalModel.Fields["Single"].TreeDependency.Items, Is.EquivalentTo(new Dictionary<string, TreeNode>
        {
            {"test_multiple_recompute.Single2", new TreeNode(finalModel.Fields["Single2"], true)},
        }));
        Assert.That(finalModel.Fields["Single2"].TreeDependency.Items, Is.EquivalentTo(new Dictionary<string, TreeNode>
        {
            {"test_multiple_recompute.Single3", new TreeNode(finalModel.Fields["Single3"], true)},
        }));
        Assert.That(finalModel.Fields["Multi"].TreeDependency.Items, Is.Empty);
        
        Assert.That(finalModel2.Fields["Name"].TreeDependency.Items, Is.Empty);
        Assert.That(finalModel2.Fields["IsPresent"].TreeDependency.Items, Is.Empty);
        Assert.That(finalModel2.Fields["AnotherModel"].TreeDependency.Items, Is.EquivalentTo(new Dictionary<string, TreeNode>
        {
            {"test_model_2.Name", new TreeNode(finalModel2.Fields["Name"], true)},
            {"test_model_2.IsPresent", new TreeNode(finalModel2.Fields["IsPresent"], true)},
        }));
    }

    [Test]
    public void TestChangeFieldInModelShouldRecomputeOnAnotherModel()
    {
        TestMultipleRecompute model = _env.Create<TestMultipleRecompute>([[]]);
        TestModel2 model2 = _env.Create<TestModel2>([[]]);
        FinalModel finalModel2 = _env.PluginManager.GetFinalModel(model2.ModelName);

        Assert.That(_env.Cache.IsToRecompute(finalModel2.Fields["AnotherModel"], model2.Id), Is.False);
        Assert.That(_env.Cache.IsToRecompute(finalModel2.Fields["Name"], model2.Id), Is.True);
        Assert.That(_env.Cache.IsToRecompute(finalModel2.Fields["IsPresent"], model2.Id), Is.True);

        Assert.That(model2.Name, Is.EqualTo("Unknown"));
        Assert.That(_env.Cache.IsToRecompute(finalModel2.Fields["Name"], model2.Id), Is.False);
        Assert.That(_env.Cache.IsToRecompute(finalModel2.Fields["IsPresent"], model2.Id), Is.True);
        Assert.That(model2.IsPresent, Is.False);
        Assert.That(_env.Cache.IsToRecompute(finalModel2.Fields["Name"], model2.Id), Is.False);
        Assert.That(_env.Cache.IsToRecompute(finalModel2.Fields["IsPresent"], model2.Id), Is.False);


        model2.AnotherModel = model;
        Assert.That(_env.Cache.IsToRecompute(finalModel2.Fields["Name"], model2.Id), Is.True);
        Assert.That(_env.Cache.IsToRecompute(finalModel2.Fields["IsPresent"], model2.Id), Is.True);

        Assert.That(model2.Name, Is.EqualTo("0ddlyoko"));
        Assert.That(_env.Cache.IsToRecompute(finalModel2.Fields["Name"], model2.Id), Is.False);
        Assert.That(_env.Cache.IsToRecompute(finalModel2.Fields["IsPresent"], model2.Id), Is.True);
        
        Assert.That(model2.IsPresent, Is.True);
        Assert.That(_env.Cache.IsToRecompute(finalModel2.Fields["Name"], model2.Id), Is.False);
        Assert.That(_env.Cache.IsToRecompute(finalModel2.Fields["IsPresent"], model2.Id), Is.False);
        
        // Change the name should not set ToRecompute=True on the IsPresent field
        model.Name = "1ddlyoko";
        Assert.That(_env.Cache.IsToRecompute(finalModel2.Fields["Name"], model2.Id), Is.True);
        Assert.That(_env.Cache.IsToRecompute(finalModel2.Fields["IsPresent"], model2.Id), Is.False);
    }

    [Test]
    public void TestDependencyTreeWithMultipleRecompute()
    {
        FinalModel finalModel1 = _pluginManager.GetFinalModel("test_multiple_recompute");
        FinalModel finalModel2 = _pluginManager.GetFinalModel("test_model_2");
        
        Assert.That(finalModel1.Fields["Name"].TreeDependency.IsLeaf, Is.False);
        Assert.That(finalModel1.Fields["Name"].TreeDependency.Root, Is.EqualTo(finalModel1.Fields["Name"]));
        Assert.That(finalModel1.Fields["Name"].TreeDependency.Items, Contains.Key("test_multiple_recompute.Name2"));
        Assert.That(finalModel1.Fields["Name"].TreeDependency.Items, Contains.Key("test_model_2.AnotherModel"));
        Assert.That(finalModel1.Fields["Name"].TreeDependency.Items, Has.Count.EqualTo(2));
        Assert.That(finalModel1.Fields["Name2"].TreeDependency.IsLeaf, Is.False);
        Assert.That(finalModel1.Fields["Name2"].TreeDependency.Root, Is.EqualTo(finalModel1.Fields["Name2"]));
        Assert.That(finalModel1.Fields["Name2"].TreeDependency.Items, Contains.Key("test_multiple_recompute.Name3"));
        Assert.That(finalModel1.Fields["Name2"].TreeDependency.Items, Has.Count.EqualTo(1));
        Assert.That(finalModel1.Fields["Name3"].TreeDependency.IsLeaf, Is.False);
        Assert.That(finalModel1.Fields["Name3"].TreeDependency.Root, Is.EqualTo(finalModel1.Fields["Name3"]));
        Assert.That(finalModel1.Fields["Name3"].TreeDependency.Items, Contains.Key("test_multiple_recompute.Name4"));
        Assert.That(finalModel1.Fields["Name3"].TreeDependency.Items, Has.Count.EqualTo(1));
        Assert.That(finalModel1.Fields["Name4"].TreeDependency.IsLeaf, Is.True);
        Assert.That(finalModel1.Fields["Name4"].TreeDependency.Root, Is.EqualTo(finalModel1.Fields["Name4"]));

        Assert.That(finalModel2.Fields["Name"].TreeDependency.IsLeaf, Is.True);
        Assert.That(finalModel2.Fields["Name"].TreeDependency.Root, Is.EqualTo(finalModel2.Fields["Name"]));
        Assert.That(finalModel2.Fields["IsPresent"].TreeDependency.IsLeaf, Is.True);
        Assert.That(finalModel2.Fields["IsPresent"].TreeDependency.Root, Is.EqualTo(finalModel2.Fields["IsPresent"]));
        Assert.That(finalModel2.Fields["AnotherModel"].TreeDependency.IsLeaf, Is.False);
        Assert.That(finalModel2.Fields["AnotherModel"].TreeDependency.Root, Is.EqualTo(finalModel2.Fields["AnotherModel"]));
        Assert.That(finalModel2.Fields["AnotherModel"].TreeDependency.Items, Contains.Key("test_model_2.Name"));
        Assert.That(finalModel2.Fields["AnotherModel"].TreeDependency.Items, Contains.Key("test_model_2.IsPresent"));
        Assert.That(finalModel2.Fields["AnotherModel"].TreeDependency.Items, Has.Count.EqualTo(2));
    }

    [Test]
    public void TestSelection()
    {
        FinalModel finalModel = _pluginManager.GetFinalModel("test_limited_partner");
        Assert.That(finalModel.Fields["State"].FieldType, Is.EqualTo(FieldType.Selection));
        Assert.That(finalModel.Fields["State"].Selection, Is.Not.Null);
        Assert.That(finalModel.Fields["State"].Selection.Selections, Is.EquivalentTo(new Dictionary<string, string>()
        {
            { "blocked", "Blocked" },
            { "managed", "Managed" },
            { "free", "Free" },
        }));

        TestLimitedPartner p = _env.Create<TestLimitedPartner>([[]]);
        Assert.That(p.State, Is.EqualTo("free"));
        Assert.That(p.IsLimited, Is.False);
        
        p.CurrentMoney = 1_000_000;
        Assert.That(p.IsLimited, Is.False);

        p.Limit = 1_000;
        Assert.That(p.IsLimited, Is.False);

        p.State = "managed";
        Assert.That(p.IsLimited, Is.True);

        p.Limit = 0;
        Assert.That(p.IsLimited, Is.False);

        p.State = "blocked";
        Assert.That(p.IsLimited, Is.True);
    }
}
