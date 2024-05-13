using System.Reflection;
using lib;
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
        _pluginManager = new("");
        _pluginManager.RegisterPlugin(_assembly);
        _aPlugin = _pluginManager.AvailablePlugins.First();
        _plugin = _aPlugin.Plugin as TestPlugin;
        _env = new Environment(_pluginManager);
    }

    [Test]
    public void TestCreateAndGetModel()
    {
        Assert.Throws<KeyNotFoundException>(() => _env.Create<TestPartner>(), "Plugin is not installed");
        _pluginManager.InstallPlugin(_aPlugin);
        
        TestPartner partner = _env.Create<TestPartner>();
        Assert.That(partner, Is.Not.Null);
        Assert.That(partner.Id, Is.EqualTo(1));
        Assert.That(partner.Env, Is.Not.Null);
        
        TestPartner partner2 = _env.Create<TestPartner>();
        Assert.That(partner2, Is.Not.Null);
        Assert.That(partner2.Id, Is.EqualTo(2));
        Assert.That(partner2.Env, Is.Not.Null);
        
        Assert.That(_env.Get<TestPartner>(1).Id, Is.EqualTo(1));
        Assert.Throws<KeyNotFoundException>(() => _env.Get<TestPartner>(3), "TestPartner with id 3 should not exist");
    }

    [Test]
    public void TestSave()
    {
        _pluginManager.InstallPlugin(_aPlugin);
        
        TestPartner partner = _env.Create<TestPartner>();
        TestPartner partner2 = _env.Get<TestPartner>(partner.Id);
        
        partner.Name = "test";
        
        Assert.That(partner.Name, Is.EqualTo("test"), "Name should be test");
        Assert.That(partner2.Name, Is.EqualTo("LoL"), "Name hasn't been saved, it should be the default value");
        partner.Save();
        Assert.That(partner.Name, Is.EqualTo("test"));
        Assert.That(partner2.Name, Is.EqualTo("test"));
        
        // Save should also work with models extending the basic one
        Assert.That(partner.Transform<TestPartner2>().Name, Is.EqualTo("test"));
    }

    [Test]
    public void TestUpdate()
    {
        _pluginManager.InstallPlugin(_aPlugin);
        
        TestPartner partner = _env.Create<TestPartner>();
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
        _pluginManager.InstallPlugin(_aPlugin);
        
        TestPartner partner = _env.Create<TestPartner>();
        TestPartner2 partner2 = partner.Transform<TestPartner2>();
        TestPartner3 partner3 = partner.Transform<TestPartner3>();
        
        Assert.That(partner.Name, Is.EqualTo("LoL"), "Name is overriden by TestPartner2");
        Assert.That(partner2.Name, Is.EqualTo("LoL"), "Name is overriden by TestPartner2");
        
        Assert.That(partner.Age, Is.EqualTo(70), "Age is overriden by TestPartner3");
        Assert.That(partner3.Age, Is.EqualTo(70), "Age is overriden by TestPartner3");
        
        Assert.That(partner2.Test, Is.EqualTo(30), "Test is overriden by TestPartner3");
        Assert.That(partner3.Test, Is.EqualTo(30), "Test is overriden by TestPartner3");

        TestPartner newPartner = _env.Create<TestPartner>(new Dictionary<string, object?>
        {
            {"Age", 100},
        });
    
        Assert.That(newPartner.Age, Is.EqualTo(100), "Default value should prioritize given values");}

    [Test]
    public void TestReset()
    {
        _pluginManager.InstallPlugin(_aPlugin);
        
        TestPartner partner = _env.Create<TestPartner>();

        partner.Name = "0ddlyoko";
        partner.Reset();
        Assert.That(partner.Name, Is.EqualTo("LoL"), "Calling Reset method should reset the model");
    }

    [Test]
    public void TestCompute()
    {
        _pluginManager.InstallPlugin(_aPlugin);
        
        TestPartner partner = _env.Create<TestPartner>();
        TestPartner2 partner2 = partner.Transform<TestPartner2>();
        TestPartner3 partner3 = partner.Transform<TestPartner3>();
        
        Assert.That(partner.DisplayName, Is.EqualTo("Name: LoL, Age: 70"));
        partner.Name = "0ddlyoko";
        partner.Age = 42;
        Assert.That(partner.DisplayName, Is.EqualTo("Name: LoL, Age: 70"), "Changing name without saving shouldn't recompute the method");
        partner.Save();
        Assert.That(partner.DisplayName, Is.EqualTo("Name: 0ddlyoko, Age: 42"), "Saving should recompute the method");

        partner2.Name = "1ddlyoko";
        partner2.Save();
        Assert.That(partner.DisplayName, Is.EqualTo("Name: 1ddlyoko, Age: 42"), "Modifying a field from a child model should recompute the method");
        
        partner3.Update(new Dictionary<string, object?>
        {
            {"Age", 54},
        });
        Assert.That(partner.DisplayName, Is.EqualTo("Name: 1ddlyoko, Age: 54"), "Modifying a field from a child model should recompute the method");
    }

    [Test]
    public void TestUpdateDate()
    {
        _pluginManager.InstallPlugin(_aPlugin);
        
        DateTime fakeTime = new DateTime(1998, 7, 21);
        TestPartner partner;
        using (new DateTimeProvider.DateTimeProviderContext(fakeTime))
        {
            partner = _env.Create<TestPartner>();
            Assert.That(partner.CreationDate, Is.EqualTo(fakeTime));
            Assert.That(partner.UpdateDate, Is.EqualTo(fakeTime));
        }

        TestPartner2 partner2 = partner.Transform<TestPartner2>();
        DateTime fakeTime2 = new DateTime(1998, 7, 22);
        using (new DateTimeProvider.DateTimeProviderContext(fakeTime2))
        {
            partner.Name = "0ddlyoko";
            Assert.That(partner.CreationDate, Is.EqualTo(fakeTime), "Creation date shouldn't change at all");
            Assert.That(partner.UpdateDate, Is.EqualTo(fakeTime), "Update date shouldn't change as the record isn't saved");
            Assert.That(partner2.CreationDate, Is.EqualTo(fakeTime), "Creation date shouldn't change at all");
            Assert.That(partner2.UpdateDate, Is.EqualTo(fakeTime), "Update date shouldn't change as the record isn't saved");
            
            partner.Save();
            Assert.That(partner.CreationDate, Is.EqualTo(fakeTime), "Creation date shouldn't change at all");
            Assert.That(partner.UpdateDate, Is.EqualTo(fakeTime2), "Update date should have changed");
            Assert.That(partner2.CreationDate, Is.EqualTo(fakeTime), "Creation date shouldn't change at all");
            Assert.That(partner2.UpdateDate, Is.EqualTo(fakeTime2), "Update date should have changed");
        }
    }

    [Test]
    public void TestDate()
    {
        _pluginManager.InstallPlugin(_aPlugin);
        DateTime fakeTime = new DateTime(1998, 7, 21, 12, 0, 0);
        DateTime fakeTime2 = new DateTime(1998, 7, 21, 13, 0, 0);
        DateTime fakeDate = fakeTime.Date;
        TestPartner partner;
        using (new DateTimeProvider.DateTimeProviderContext(fakeTime))
        {
            partner = _env.Create<TestPartner>();
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
}
