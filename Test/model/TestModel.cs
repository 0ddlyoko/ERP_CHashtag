using lib;
using lib.cache;
using lib.field;
using lib.model;
using lib.test;
using Test.data.models;

namespace Test.model;

public class TestModel: ErpTest, IAsyncLifetime
{
    public new async Task InitializeAsync()
    {
        await base.InitializeAsync();
    }
    
    [Fact]
    public void TestCreateAndGetModel()
    {
        Assert.Throws<InvalidOperationException>(() => Env.Create<TestPartner>([]));
        
        TestPartner partner = Env.Create<TestPartner>([[]]);
        Assert.NotNull(partner);
        Assert.Equivalent(1, partner.Id);
        Assert.NotNull(partner.Env);
        
        TestPartner partner2 = Env.Create<TestPartner>([[]]);
        Assert.NotNull(partner2);
        Assert.Equal(2, partner2.Id);
        Assert.NotNull(partner2.Env);
        
        Assert.Equal(1, Env.Get<TestPartner>(1).Id);
        TestPartner partner3 = Env.Get<TestPartner>(3);
        Assert.NotNull(partner3);
    }

    [Fact]
    public void TestUpdate()
    {
        TestPartner partner = Env.Create<TestPartner>([[]]);
        TestPartner2 partner2 = partner.Transform<TestPartner2>();
        
        partner.Update(new Dictionary<string, object?>
        {
            {"Name", "0ddlyoko"},
            {"Age", 54},
        });
        
        Assert.Equal("0ddlyoko", partner.Name);
        Assert.Equal(54, partner.Age);
        Assert.Equal("0ddlyoko", partner2.Name);

        partner.Name = "1ddlyoko";
        partner.Update(new Dictionary<string, object?>
        {
            {"Age", 60},
        });
        Assert.Equal("1ddlyoko", partner.Name);
        Assert.Equal(60, partner.Age);
        Assert.Equal("1ddlyoko", partner2.Name);
    }

    [Fact]
    public void TestDefaultValue()
    {
        TestPartner partner = Env.Create<TestPartner>([[]]);
        TestPartner2 partner2 = partner.Transform<TestPartner2>();
        TestPartner3 partner3 = partner.Transform<TestPartner3>();
        
        Assert.Equal("LoL", partner.Name);
        Assert.Equal("LoL", partner2.Name);
        
        Assert.Equal(70, partner.Age);
        Assert.Equal(70, partner3.Age);
        
        Assert.Equal(30, partner2.Test);
        Assert.Equal(30, partner3.Test);

        TestPartner newPartner = Env.Create<TestPartner>([new Dictionary<string, object?>
        {
            {"Age", 100},
        }]);
    
        Assert.Equal(100, newPartner.Age);
    }

    [Fact]
    public void TestCompute()
    {
        TestPartner partner = Env.Create<TestPartner>([[]]);
        FinalModel finalModel = Env.PluginManager.GetFinalModel(partner.ModelName);
        CachedModel cachedModel = Env.Cache.CachedModels[partner.ModelName][partner.Id];
        TestPartner2 partner2 = partner.Transform<TestPartner2>();
        TestPartner3 partner3 = partner.Transform<TestPartner3>();
        Assert.Multiple(() =>
        {
            // DisplayName shouldn't be already computed
            Assert.Null(cachedModel.Fields["DisplayName"].Value);
            Assert.True(Env.Cache.IsToRecompute(finalModel.Fields["DisplayName"], partner.Id));
            // Assert.True(cachedModel.Fields["DisplayName"].Dirty);
            // Assert.True(cachedModel.Dirty);
        });

        // Now that we access to DisplayName, it should be computed
        Assert.Equal("Name: LoL, Age: 70", partner.DisplayName);
        Assert.Multiple(() =>
        {
            Assert.Equal("Name: LoL, Age: 70", cachedModel.Fields["DisplayName"].Value);
            Assert.False(Env.Cache.IsToRecompute(finalModel.Fields["DisplayName"], partner.Id));
            // Assert.True(cachedModel.Fields["DisplayName"].Dirty);
            // Assert.True(cachedModel.Dirty);
        });
        
        partner.Name = "0ddlyoko";
        partner.Age = 42;
        Assert.Multiple(() =>
        {
            // Modifying fields should not trigger the compute, but should set the flag to true
            Assert.Equal("Name: LoL, Age: 70", cachedModel.Fields["DisplayName"].Value);
            Assert.True(Env.Cache.IsToRecompute(finalModel.Fields["DisplayName"], partner.Id));
            // Assert.True(cachedModel.Fields["DisplayName"].Dirty);
            // Assert.True(cachedModel.Dirty);
        });
        Assert.Multiple(() =>
        {
            // Accessing again to DisplayName should compute it as ToRecompute = true
            Assert.Equal("Name: 0ddlyoko, Age: 42", partner.DisplayName);
            Assert.Equal("Name: 0ddlyoko, Age: 42", cachedModel.Fields["DisplayName"].Value);
            Assert.False(Env.Cache.IsToRecompute(finalModel.Fields["DisplayName"], partner.Id));
            // Assert.True(cachedModel.Fields["DisplayName"].Dirty);
            // Assert.True(cachedModel.Dirty);
        });
        partner2.Name = "1ddlyoko";
        Assert.Equal("Name: 1ddlyoko, Age: 42", partner.DisplayName);
        
        partner3.Update(new Dictionary<string, object?>
        {
            {"Age", 54},
        });
        Assert.Equal("Name: 1ddlyoko, Age: 54", partner.DisplayName);
    }

    [Fact]
    public void TestRecomputeNotDoneIfFieldIsSet()
    {
        TestPartner partner = Env.Create<TestPartner>([[]]);
        FinalModel finalModel = Env.PluginManager.GetFinalModel(partner.ModelName);

        Assert.True(Env.Cache.IsToRecompute(finalModel.Fields["DisplayName"], partner.Id));
        partner.Name = "Test";
        Assert.True(Env.Cache.IsToRecompute(finalModel.Fields["DisplayName"], partner.Id));
        partner.DisplayName = "My Own display name";
        Assert.False(Env.Cache.IsToRecompute(finalModel.Fields["DisplayName"], partner.Id));
    }

    [Fact]
    public void TestRecomputeNotDoneIfUpdateIsCalledWithComputedField()
    {
        TestPartner partner = Env.Create<TestPartner>([[]]);
        FinalModel finalModel = Env.PluginManager.GetFinalModel(partner.ModelName);

        Assert.True(Env.Cache.IsToRecompute(finalModel.Fields["DisplayName"], partner.Id));
        partner.Update(new Dictionary<string, object?>
        {
            {"Name", "0ddlyoko"},
            {"DisplayName", "My Own Display Name"},
            {"Age", 54},
        });
        Assert.False(Env.Cache.IsToRecompute(finalModel.Fields["DisplayName"], partner.Id));
    }

    [Fact]
    public void TestUpdateDate()
    {
        DateTime fakeTime = new DateTime(1998, 7, 21);
        TestPartner partner;
        using (new DateTimeProvider.DateTimeProviderContext(fakeTime))
        {
            partner = Env.Create<TestPartner>([[]]);
            Assert.Equal(fakeTime, partner.CreationDate);
            Assert.Equal(fakeTime, partner.UpdateDate);
        }

        TestPartner2 partner2 = partner.Transform<TestPartner2>();
        DateTime fakeTime2 = new DateTime(1998, 7, 22);
        using (new DateTimeProvider.DateTimeProviderContext(fakeTime2))
        {
            partner.Name = "0ddlyoko";
            Assert.Equal(fakeTime, partner.CreationDate);
            Assert.Equal(fakeTime2, partner.UpdateDate);
            Assert.Equal(fakeTime, partner2.CreationDate);
            Assert.Equal(fakeTime2, partner2.UpdateDate);
        }
    }

    [Fact]
    public void TestDate()
    {
        DateTime fakeTime = new DateTime(1998, 7, 21, 12, 0, 0);
        DateTime fakeTime2 = new DateTime(1998, 7, 21, 13, 0, 0);
        DateTime fakeDate = fakeTime.Date;
        using (new DateTimeProvider.DateTimeProviderContext(fakeTime))
        {
            TestPartner partner = Env.Create<TestPartner>([[]]);
            Assert.Equal(fakeDate, partner.MyDate);

            partner.Update(new Dictionary<string, object?>
            {
                {"MyDate", fakeTime},
                {"MyDateTime", fakeTime},
            });
            
            Assert.Equal(fakeDate, partner.MyDate);
            Assert.Equal(fakeTime, partner.MyDateTime);

            partner.Update(new Dictionary<string, object?>
            {
                {"MyDate", fakeTime2},
                {"MyDateTime", fakeTime2},
            });
            
            Assert.Equal(fakeDate, partner.MyDate);
            Assert.Equal(fakeTime2, partner.MyDateTime);
        }
    }

    [Fact]
    public void TestMultiplePartner()
    {
        TestPartner partner = Env.Create<TestPartner>([[], []]);
        Assert.Equal(2, partner.Ids.Count);
        Assert.Throws<InvalidOperationException>(() => { _ = partner.Name; });
        
        TestPartner2 partner2 = partner.Transform<TestPartner2>();
        Assert.Equal(2,  partner2.Ids.Count);

        TestPartner partnerWithOnlyFirstId = Env.Get<TestPartner>(partner.Ids[0]);
        partner.Name = "New Name";
        Assert.Equal("New Name", partnerWithOnlyFirstId.Name);
    }

    [Fact]
    public void TestPartnerToCategory()
    {
        TestPartner partner = Env.Create<TestPartner>([[]]);
        TestCategory category = Env.Create<TestCategory>([[]]);
        CachedModel cachedPartnerModel = Env.Cache.CachedModels[partner.ModelName][partner.Id];
        CachedModel cachedCategoryModel = Env.Cache.CachedModels[category.ModelName][category.Id];

        Assert.Contains("Category", cachedPartnerModel.Fields.Keys);
        Assert.Null(cachedPartnerModel.Fields["Category"].Value);
        
        Assert.Contains("Partners", cachedCategoryModel.Fields.Keys);
        Assert.NotNull(cachedCategoryModel.Fields["Partners"].Value);
        Assert.IsType<List<int>>(cachedCategoryModel.Fields["Partners"].Value);
        
        partner.Category = category;
        
        Assert.NotNull(cachedPartnerModel.Fields["Category"].Value);
        Assert.Equal(category.Id, cachedPartnerModel.Fields["Category"].Value);
        Assert.IsType<List<int>>(cachedCategoryModel.Fields["Partners"].Value);
        Assert.Equal(new List<int> { partner.Id }, cachedCategoryModel.Fields["Partners"].Value);

        Assert.NotNull(category.Partners);
        Assert.Single(category.Partners.Ids);
        Assert.Equal(partner.Id, category.Partners.Ids[0]);
        
        // Remove the category
        partner.Category = null;
        
        Assert.Null(cachedPartnerModel.Fields["Category"].Value);
        Assert.IsType<List<int>>(cachedCategoryModel.Fields["Partners"].Value);
        Assert.Null(cachedCategoryModel.Fields["Partners"].Value);
        
        // Now, check if we can modify it from the category
        category.Partners = partner;
        
        Assert.NotNull(cachedPartnerModel.Fields["Category"].Value);
        Assert.Equal(category.Id, cachedPartnerModel.Fields["Category"].Value);
        Assert.IsType<List<int>>(cachedCategoryModel.Fields["Partners"].Value);
        Assert.Equal(new[] { partner.Id }, cachedCategoryModel.Fields["Partners"].Value);

        Assert.NotNull(category.Partners);
        Assert.Single(category.Partners.Ids);
        Assert.Equal(partner.Id, category.Partners.Ids[0]);
    }

    [Fact]
    public void TestMultipleRecompute()
    {
        TestMultipleRecompute model = Env.Create<TestMultipleRecompute>([[]]);
        FinalModel finalModel = Env.PluginManager.GetFinalModel(model.ModelName);
        
        Assert.False(Env.Cache.IsToRecompute(finalModel.Fields["Name"], model.Id));
        Assert.True(Env.Cache.IsToRecompute(finalModel.Fields["Name2"], model.Id));
        Assert.True(Env.Cache.IsToRecompute(finalModel.Fields["Name3"], model.Id));
        Assert.True(Env.Cache.IsToRecompute(finalModel.Fields["Name4"], model.Id));
        
        Assert.Equal("0ddlyoko", model.Name);
        Assert.False(Env.Cache.IsToRecompute(finalModel.Fields["Name"], model.Id));
        Assert.True(Env.Cache.IsToRecompute(finalModel.Fields["Name2"], model.Id));
        Assert.True(Env.Cache.IsToRecompute(finalModel.Fields["Name3"], model.Id));
        Assert.True(Env.Cache.IsToRecompute(finalModel.Fields["Name4"], model.Id));
        
        Assert.Equal("0ddlyoko-2-3-4", model.Name4);
        Assert.False(Env.Cache.IsToRecompute(finalModel.Fields["Name"], model.Id));
        Assert.False(Env.Cache.IsToRecompute(finalModel.Fields["Name2"], model.Id));
        Assert.False(Env.Cache.IsToRecompute(finalModel.Fields["Name3"], model.Id));
        Assert.False(Env.Cache.IsToRecompute(finalModel.Fields["Name4"], model.Id));
        
        model.Name2 = "1ddlyoko";
        Assert.False(Env.Cache.IsToRecompute(finalModel.Fields["Name"], model.Id));
        Assert.False(Env.Cache.IsToRecompute(finalModel.Fields["Name2"], model.Id));
        Assert.True(Env.Cache.IsToRecompute(finalModel.Fields["Name3"], model.Id));
        Assert.True(Env.Cache.IsToRecompute(finalModel.Fields["Name4"], model.Id));
        
        Assert.Equal("1ddlyoko-3-4", model.Name4);
        Assert.False(Env.Cache.IsToRecompute(finalModel.Fields["Name"], model.Id));
        Assert.False(Env.Cache.IsToRecompute(finalModel.Fields["Name2"], model.Id));
        Assert.False(Env.Cache.IsToRecompute(finalModel.Fields["Name3"], model.Id));
        Assert.False(Env.Cache.IsToRecompute(finalModel.Fields["Name4"], model.Id));
        
        model.Name = "test";
        Assert.Equal("test-2-3-4", model.Name4);
    }

    [Fact]
    public void TestX2XCompute()
    {
        // TODO FIXME
        TestMultipleRecompute model = Env.Create<TestMultipleRecompute>([[]]);
        TestMultipleRecompute model2 = Env.Create<TestMultipleRecompute>([[]]);
        TestMultipleRecompute model3 = Env.Create<TestMultipleRecompute>([[]]);
        
        Assert.Empty(model.Multi.Ids);
        model.Single = model2;
        
        Assert.Equal(model2, model.Single);
        Assert.Equal(model2, model.Single2);
        Assert.Equal(model, model2.Multi);

        model.Single = model3;
        // Assert.False(Env.Cache.IsToRecompute(Env.PluginManager.GetFinalModel(model3.ModelName).Fields["Single2"], model.Id), "This field should have been computed");
        Assert.True(Env.Cache.IsToRecompute(Env.PluginManager.GetFinalModel(model3.ModelName).Fields["Single3"], model.Id));
        Assert.False(Env.Cache.IsToRecompute(Env.PluginManager.GetFinalModel(model3.ModelName).Fields["Single2"], model2.Id), "This field should have been computed");
        Assert.True(Env.Cache.IsToRecompute(Env.PluginManager.GetFinalModel(model3.ModelName).Fields["Single3"], model2.Id));
        // TODO Next line is failing but should work
        // Assert.True(Env.Cache.IsToRecompute(Env.PluginManager.GetFinalModel(model3.ModelName).Fields["Multi"], model3.Id));
        Assert.Empty(model2.Multi.Ids);
        Assert.Equal(model, model3.Multi);

        model.Single = model3;
        // Assert.Empty(model3.Multi.Ids);
    }

    [Fact]
    public void TestMultipleRecomputeShouldHaveCorrectAllOccurrences()
    {
        FinalModel finalModel = PluginManager.GetFinalModel("test_multiple_recompute");
        FinalModel finalModel2 = PluginManager.GetFinalModel("test_model_2");
        
        Assert.Equivalent(new Dictionary<string, TreeNode> {
            {"test_multiple_recompute.Name2", new TreeNode(finalModel.Fields["Name2"], true)},
            {"test_model_2.AnotherModel", new TreeNode(finalModel2.Fields["AnotherModel"], false)},
        }, finalModel.Fields["Name"].TreeDependency.Items);
        Assert.Equivalent(new Dictionary<string, TreeNode>
        {
            {"test_multiple_recompute.Name3", new TreeNode(finalModel.Fields["Name3"], true)},
        }, finalModel.Fields["Name2"].TreeDependency.Items);
        Assert.Equivalent(new Dictionary<string, TreeNode>
        {
            {"test_multiple_recompute.Name4", new TreeNode(finalModel.Fields["Name4"], true)},
        }, finalModel.Fields["Name3"].TreeDependency.Items);
        Assert.Empty(finalModel.Fields["Name4"].TreeDependency.Items);
        Assert.Equivalent(new Dictionary<string, TreeNode>
        {
            {"test_multiple_recompute.Single2", new TreeNode(finalModel.Fields["Single2"], true)},
        }, finalModel.Fields["Single"].TreeDependency.Items);
        Assert.Equivalent(new Dictionary<string, TreeNode>
        {
            {"test_multiple_recompute.Single3", new TreeNode(finalModel.Fields["Single3"], true)},
        }, finalModel.Fields["Single2"].TreeDependency.Items);
        Assert.Empty(finalModel.Fields["Multi"].TreeDependency.Items);
        
        Assert.Empty(finalModel2.Fields["Name"].TreeDependency.Items);
        Assert.Empty(finalModel2.Fields["IsPresent"].TreeDependency.Items);
        Assert.Equivalent(new Dictionary<string, TreeNode>
        {
            {"test_model_2.Name", new TreeNode(finalModel2.Fields["Name"], true)},
            {"test_model_2.IsPresent", new TreeNode(finalModel2.Fields["IsPresent"], true)},
        }, finalModel2.Fields["AnotherModel"].TreeDependency.Items);
    }

    [Fact]
    public void TestChangeFieldInModelShouldRecomputeOnAnotherModel()
    {
        TestMultipleRecompute model = Env.Create<TestMultipleRecompute>([[]]);
        TestModel2 model2 = Env.Create<TestModel2>([[]]);
        FinalModel finalModel2 = Env.PluginManager.GetFinalModel(model2.ModelName);

        Assert.False(Env.Cache.IsToRecompute(finalModel2.Fields["AnotherModel"], model2.Id));
        Assert.True(Env.Cache.IsToRecompute(finalModel2.Fields["Name"], model2.Id));
        Assert.True(Env.Cache.IsToRecompute(finalModel2.Fields["IsPresent"], model2.Id));

        Assert.Equal("Unknown", model2.Name);
        Assert.False(Env.Cache.IsToRecompute(finalModel2.Fields["Name"], model2.Id));
        Assert.True(Env.Cache.IsToRecompute(finalModel2.Fields["IsPresent"], model2.Id));
        Assert.False(model2.IsPresent);
        Assert.False(Env.Cache.IsToRecompute(finalModel2.Fields["Name"], model2.Id));
        Assert.False(Env.Cache.IsToRecompute(finalModel2.Fields["IsPresent"], model2.Id));


        model2.AnotherModel = model;
        Assert.True(Env.Cache.IsToRecompute(finalModel2.Fields["Name"], model2.Id));
        Assert.True(Env.Cache.IsToRecompute(finalModel2.Fields["IsPresent"], model2.Id));

        Assert.Equal("0ddlyoko", model2.Name);
        Assert.False(Env.Cache.IsToRecompute(finalModel2.Fields["Name"], model2.Id));
        Assert.True(Env.Cache.IsToRecompute(finalModel2.Fields["IsPresent"], model2.Id));
        
        Assert.True(model2.IsPresent);
        Assert.False(Env.Cache.IsToRecompute(finalModel2.Fields["Name"], model2.Id));
        Assert.False(Env.Cache.IsToRecompute(finalModel2.Fields["IsPresent"], model2.Id));
        
        // Change the name should not set ToRecompute=True on the IsPresent field
        model.Name = "1ddlyoko";
        Assert.True(Env.Cache.IsToRecompute(finalModel2.Fields["Name"], model2.Id));
        Assert.False(Env.Cache.IsToRecompute(finalModel2.Fields["IsPresent"], model2.Id));
    }

    [Fact]
    public void TestDependencyTreeWithMultipleRecompute()
    {
        FinalModel finalModel1 = PluginManager.GetFinalModel("test_multiple_recompute");
        FinalModel finalModel2 = PluginManager.GetFinalModel("test_model_2");
        
        Assert.False(finalModel1.Fields["Name"].TreeDependency.IsLeaf);
        Assert.Equal(finalModel1.Fields["Name"], finalModel1.Fields["Name"].TreeDependency.Root);
        Assert.Contains("test_multiple_recompute.Name2", finalModel1.Fields["Name"].TreeDependency.Items.Keys);
        Assert.Contains("test_model_2.AnotherModel", finalModel1.Fields["Name"].TreeDependency.Items.Keys);
        Assert.Equal(2, finalModel1.Fields["Name"].TreeDependency.Items.Count);
        Assert.False(finalModel1.Fields["Name2"].TreeDependency.IsLeaf);
        Assert.Equal(finalModel1.Fields["Name2"], finalModel1.Fields["Name2"].TreeDependency.Root);
        Assert.Contains("test_multiple_recompute.Name3", finalModel1.Fields["Name2"].TreeDependency.Items.Keys);
        Assert.Single(finalModel1.Fields["Name2"].TreeDependency.Items);
        Assert.False(finalModel1.Fields["Name3"].TreeDependency.IsLeaf);
        Assert.Equal(finalModel1.Fields["Name3"], finalModel1.Fields["Name3"].TreeDependency.Root);
        Assert.Contains("test_multiple_recompute.Name4", finalModel1.Fields["Name3"].TreeDependency.Items.Keys);
        Assert.Single(finalModel1.Fields["Name3"].TreeDependency.Items);
        Assert.True(finalModel1.Fields["Name4"].TreeDependency.IsLeaf);
        Assert.Equal(finalModel1.Fields["Name4"], finalModel1.Fields["Name4"].TreeDependency.Root);

        Assert.True(finalModel2.Fields["Name"].TreeDependency.IsLeaf);
        Assert.Equal(finalModel2.Fields["Name"], finalModel2.Fields["Name"].TreeDependency.Root);
        Assert.True(finalModel2.Fields["IsPresent"].TreeDependency.IsLeaf);
        Assert.Equal(finalModel2.Fields["IsPresent"].TreeDependency.Root, finalModel2.Fields["IsPresent"]);
        Assert.False(finalModel2.Fields["AnotherModel"].TreeDependency.IsLeaf);
        Assert.Equal(finalModel2.Fields["AnotherModel"].TreeDependency.Root, finalModel2.Fields["AnotherModel"]);
        Assert.Contains("test_model_2.Name", finalModel2.Fields["AnotherModel"].TreeDependency.Items.Keys);
        Assert.Contains("test_model_2.IsPresent", finalModel2.Fields["AnotherModel"].TreeDependency.Items.Keys);
        Assert.Single(finalModel2.Fields["AnotherModel"].TreeDependency.Items);
    }

    [Fact]
    public void TestSelection()
    {
        FinalModel finalModel = PluginManager.GetFinalModel("test_limited_partner");
        Assert.Equal(FieldType.Selection, finalModel.Fields["State"].FieldType);
        Assert.NotNull(finalModel.Fields["State"].Selection);
        Assert.Equivalent(new Dictionary<string, string>
        {
            { "blocked", "Blocked" },
            { "managed", "Managed" },
            { "free", "Free" },
        }, finalModel.Fields["State"].Selection.Selections);

        TestLimitedPartner p = Env.Create<TestLimitedPartner>([[]]);
        Assert.Equal("free", p.State);
        Assert.False(p.IsLimited);
        
        p.CurrentMoney = 1_000_000;
        Assert.False(p.IsLimited);

        p.Limit = 1_000;
        Assert.False(p.IsLimited);

        p.State = "managed";
        Assert.True(p.IsLimited);

        p.Limit = 0;
        Assert.False(p.IsLimited);

        p.State = "blocked";
        Assert.True(p.IsLimited);
    }
}
