using System.Reflection;
using lib.database;
using lib.model;
using lib.plugin;
using Environment = lib.Environment;

namespace Test.database;

[TestFixture]
public class TestQuery
{
    private Assembly _assembly;
    private PluginManager _pluginManager;
    private APlugin _aPlugin;
    private TestPlugin _plugin;
    private Environment _env;
    private FinalModel _finalModel;

    [SetUp]
    public void Setup()
    {
        _assembly = typeof(TestQuery).Assembly;
        // _pluginManager = new("");
        _pluginManager.RegisterPlugin(_assembly);
        _aPlugin = _pluginManager.AvailablePlugins.First();
        _pluginManager.InstallPlugin(_aPlugin);
        _plugin = _aPlugin.Plugin as TestPlugin;
        _env = new Environment(_pluginManager);
        _finalModel = _pluginManager.GetFinalModel("test_partner");
    }

    [Test]
    public void TestBasicDomains()
    {
        Query.DomainQuery domainQuery = Query.DomainToQuery(_finalModel, [("Name", "=", "Test")]);
        
        Assert.That(domainQuery.Where, Is.EqualTo("((\"test_partner\".\"Name\" = $1))"));
        Assert.That(domainQuery.Arguments, Is.EquivalentTo(new[] { "Test" }));
        Assert.That(domainQuery.LeftJoins, Is.Empty);
        
        
        domainQuery = Query.DomainToQuery(_finalModel, [("Name", "=", "Test"), ("Age", ">=", 18)]);
        
        Assert.That(domainQuery.Where, Is.EqualTo("((\"test_partner\".\"Name\" = $1) AND (\"test_partner\".\"Age\" >= $2))"));
        Assert.That(domainQuery.Arguments, Is.EquivalentTo(new List<object> { "Test", 18 }));
        Assert.That(domainQuery.LeftJoins, Is.Empty);
        
        
        domainQuery = Query.DomainToQuery(_finalModel, ['&', ("Name", "=", "Test"), ("Age", ">=", 18)]);
        
        Assert.That(domainQuery.Where, Is.EqualTo("(((\"test_partner\".\"Name\" = $1) AND (\"test_partner\".\"Age\" >= $2)))"));
        Assert.That(domainQuery.Arguments, Is.EquivalentTo(new List<object> { "Test", 18 }));
        Assert.That(domainQuery.LeftJoins, Is.Empty);
        
        
        domainQuery = Query.DomainToQuery(_finalModel, ['|', ("Name", "=", "Test"), ("Age", ">=", 18)]);
        
        Assert.That(domainQuery.Where, Is.EqualTo("(((\"test_partner\".\"Name\" = $1) OR (\"test_partner\".\"Age\" >= $2)))"));
        Assert.That(domainQuery.Arguments, Is.EquivalentTo(new List<object> { "Test", 18 }));
        Assert.That(domainQuery.LeftJoins, Is.Empty);
        
        
        domainQuery = Query.DomainToQuery(_finalModel, ['|', ("Name", "=", "Test"), ("Name", "=", "Test2"), ("Age", ">=", 18)]);
        
        Assert.That(domainQuery.Where, Is.EqualTo("(((\"test_partner\".\"Name\" = $1) OR (\"test_partner\".\"Name\" = $2)) AND (\"test_partner\".\"Age\" >= $3))"));
        Assert.That(domainQuery.Arguments, Is.EquivalentTo(new List<object> { "Test", "Test2", 18 }));
        Assert.That(domainQuery.LeftJoins, Is.Empty);
        
        
        domainQuery = Query.DomainToQuery(_finalModel, ['&', '|', ("Name", "=", "Test"), ("Name", "=", "Test2"), ("Age", ">=", 18)]);
        
        Assert.That(domainQuery.Where, Is.EqualTo("((((\"test_partner\".\"Name\" = $1) OR (\"test_partner\".\"Name\" = $2)) AND (\"test_partner\".\"Age\" >= $3)))"));
        Assert.That(domainQuery.Arguments, Is.EquivalentTo(new List<object> { "Test", "Test2", 18 }));
        Assert.That(domainQuery.LeftJoins, Is.Empty);
    }

    [Test]
    public void TestLeftJoinDomains()
    {
        Query.DomainQuery domainQuery = Query.DomainToQuery(_finalModel, [("Category.Name", "=", "Test")]);

        Assert.That(domainQuery.Where, Is.EqualTo("((\"test_partner.Category\".\"Name\" = $1))"));
        Assert.That(domainQuery.Arguments, Is.EquivalentTo(new[] { "Test" }));
        Assert.That(domainQuery.LeftJoins,
            Is.EquivalentTo(new[]
            {
                "LEFT JOIN \"test_category\" AS \"test_partner.Category\" ON \"test_partner\".\"Category\" = \"test_partner.Category\".\"id\""
            }));


        domainQuery = Query.DomainToQuery(_finalModel,
            [("Category.Name", "=", "Test"), ("Category.Partners.Name", "like", "Hello")]);

        Assert.That(domainQuery.Where,
            Is.EqualTo(
                "((\"test_partner.Category\".\"Name\" = $1) AND (\"test_partner.Category.Partners\".\"Name\" LIKE $2))"));
        Assert.That(domainQuery.Arguments, Is.EquivalentTo(new[] { "Test", "Hello" }));
        Assert.That(domainQuery.LeftJoins, Is.EquivalentTo(new[]
        {
            "LEFT JOIN \"test_category\" AS \"test_partner.Category\" ON \"test_partner\".\"Category\" = \"test_partner.Category\".\"id\"",
            "LEFT JOIN \"test_partner\" AS \"test_partner.Category.Partners\" ON \"test_partner.Category\".\"id\" = \"test_partner.Category.Partners\".\"Category\"",
        }));


        domainQuery = Query.DomainToQuery(_finalModel,
            [("Category.Name", "=", "Test"), ("Category", "in", new List<int> { 1, 2, 3 })]);

        Assert.That(domainQuery.Where,
            Is.EqualTo("((\"test_partner.Category\".\"Name\" = $1) AND (\"test_partner\".\"Category\" IN $2))"));
        Assert.That(domainQuery.Arguments, Is.EquivalentTo(new List<object>() { "Test", new List<int> { 1, 2, 3 } }));
        Assert.That(domainQuery.LeftJoins, Is.EquivalentTo(new[]
        {
            "LEFT JOIN \"test_category\" AS \"test_partner.Category\" ON \"test_partner\".\"Category\" = \"test_partner.Category\".\"id\"",
        }));

    }

    [Test]
    public void TestLeftJoinWithId()
    {
        Query.DomainQuery domainQuery = Query.DomainToQuery(_finalModel, [("Category.Name", "=", "Test"), ("Category.Partners.Id", "in", new List<int> { 1, 2, 3 })]);
        
        Assert.That(domainQuery.Where, Is.EqualTo("((\"test_partner.Category\".\"Name\" = $1) AND (\"test_partner.Category.Partners\".\"Id\" IN $2))"));
        Assert.That(domainQuery.Arguments, Is.EquivalentTo(new List<object>() { "Test", new List<int> { 1, 2, 3 } }));
        Assert.That(domainQuery.LeftJoins, Is.EquivalentTo(new[]
        {
            "LEFT JOIN \"test_category\" AS \"test_partner.Category\" ON \"test_partner\".\"Category\" = \"test_partner.Category\".\"id\"",
            "LEFT JOIN \"test_partner\" AS \"test_partner.Category.Partners\" ON \"test_partner.Category\".\"id\" = \"test_partner.Category.Partners\".\"Category\"",
        }));
        
        // Same as above, but without ".Id" in domain
        domainQuery = Query.DomainToQuery(_finalModel, [("Category.Name", "=", "Test"), ("Category.Partners", "in", new List<int> { 1, 2, 3 })]);
        
        Assert.That(domainQuery.Where, Is.EqualTo("((\"test_partner.Category\".\"Name\" = $1) AND (\"test_partner.Category.Partners\".\"Id\" IN $2))"));
        Assert.That(domainQuery.Arguments, Is.EquivalentTo(new List<object>() { "Test", new List<int> { 1, 2, 3 } }));
        Assert.That(domainQuery.LeftJoins, Is.EquivalentTo(new[]
        {
            "LEFT JOIN \"test_category\" AS \"test_partner.Category\" ON \"test_partner\".\"Category\" = \"test_partner.Category\".\"id\"",
            "LEFT JOIN \"test_partner\" AS \"test_partner.Category.Partners\" ON \"test_partner.Category\".\"id\" = \"test_partner.Category.Partners\".\"Category\"",
        }));
    }
}
