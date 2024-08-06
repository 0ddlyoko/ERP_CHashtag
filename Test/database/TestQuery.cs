using System.Reflection;
using lib.database;
using lib.model;
using lib.plugin;
using Environment = lib.Environment;

namespace Test.database;

public class TestQuery
{
    private Assembly _assembly;
    private PluginManager _pluginManager;
    private APlugin _aPlugin;
    private TestPlugin _plugin;
    private Environment _env;
    private FinalModel _finalModel;

    public TestQuery()
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

    [Fact]
    public void TestBasicDomains()
    {
        Query.DomainQuery domainQuery = Query.DomainToQuery(_finalModel, [("Name", "=", "Test")]);
        
        Assert.Equal("((\"test_partner\".\"Name\" = $1))", domainQuery.Where);
        Assert.Equivalent(new[] { "Test" }, domainQuery.Arguments);
        Assert.Empty(domainQuery.LeftJoins);
        
        
        domainQuery = Query.DomainToQuery(_finalModel, [("Name", "=", "Test"), ("Age", ">=", 18)]);
        
        Assert.Equal("((\"test_partner\".\"Name\" = $1) AND (\"test_partner\".\"Age\" >= $2))", domainQuery.Where);
        Assert.Equivalent(new List<object> { "Test", 18 }, domainQuery.Arguments);
        Assert.Empty(domainQuery.LeftJoins);
        
        
        domainQuery = Query.DomainToQuery(_finalModel, ['&', ("Name", "=", "Test"), ("Age", ">=", 18)]);
        
        Assert.Equal("(((\"test_partner\".\"Name\" = $1) AND (\"test_partner\".\"Age\" >= $2)))", domainQuery.Where);
        Assert.Equivalent(new List<object> { "Test", 18 }, domainQuery.Arguments);
        Assert.Empty(domainQuery.LeftJoins);
        
        
        domainQuery = Query.DomainToQuery(_finalModel, ['|', ("Name", "=", "Test"), ("Age", ">=", 18)]);
        
        Assert.Equal("(((\"test_partner\".\"Name\" = $1) OR (\"test_partner\".\"Age\" >= $2)))", domainQuery.Where);
        Assert.Equivalent(new List<object> { "Test", 18 }, domainQuery.Arguments);
        Assert.Empty(domainQuery.LeftJoins);
        
        
        domainQuery = Query.DomainToQuery(_finalModel, ['|', ("Name", "=", "Test"), ("Name", "=", "Test2"), ("Age", ">=", 18)]);
        
        Assert.Equal("(((\"test_partner\".\"Name\" = $1) OR (\"test_partner\".\"Name\" = $2)) AND (\"test_partner\".\"Age\" >= $3))", domainQuery.Where);
        Assert.Equivalent(new List<object> { "Test", "Test2", 18 }, domainQuery.Arguments);
        Assert.Empty(domainQuery.LeftJoins);
        
        
        domainQuery = Query.DomainToQuery(_finalModel, ['&', '|', ("Name", "=", "Test"), ("Name", "=", "Test2"), ("Age", ">=", 18)]);
        
        Assert.Equal("((((\"test_partner\".\"Name\" = $1) OR (\"test_partner\".\"Name\" = $2)) AND (\"test_partner\".\"Age\" >= $3)))", domainQuery.Where);
        Assert.Equivalent(new List<object> { "Test", "Test2", 18 }, domainQuery.Arguments);
        Assert.Empty(domainQuery.LeftJoins);
    }

    [Fact]
    public void TestLeftJoinDomains()
    {
        Query.DomainQuery domainQuery = Query.DomainToQuery(_finalModel, [("Category.Name", "=", "Test")]);

        Assert.Equal("((\"test_partner.Category\".\"Name\" = $1))", domainQuery.Where);
        Assert.Equivalent(new[] { "Test" }, domainQuery.Arguments);
        Assert.Equivalent(new[]
        {
            "LEFT JOIN \"test_category\" AS \"test_partner.Category\" ON \"test_partner\".\"Category\" = \"test_partner.Category\".\"id\"",
        }, domainQuery.LeftJoins);


        domainQuery = Query.DomainToQuery(_finalModel,
            [("Category.Name", "=", "Test"), ("Category.Partners.Name", "like", "Hello")]);

        Assert.Equal("((\"test_partner.Category\".\"Name\" = $1) AND (\"test_partner.Category.Partners\".\"Name\" LIKE $2))", domainQuery.Where);
        Assert.Equivalent(new[] { "Test", "Hello" }, domainQuery.Arguments);
        Assert.Equivalent(new[]
        {
            "LEFT JOIN \"test_category\" AS \"test_partner.Category\" ON \"test_partner\".\"Category\" = \"test_partner.Category\".\"id\"",
            "LEFT JOIN \"test_partner\" AS \"test_partner.Category.Partners\" ON \"test_partner.Category\".\"id\" = \"test_partner.Category.Partners\".\"Category\"",
        }, domainQuery.LeftJoins);


        domainQuery = Query.DomainToQuery(_finalModel,
            [("Category.Name", "=", "Test"), ("Category", "in", new List<int> { 1, 2, 3 })]);

        Assert.Equal("((\"test_partner.Category\".\"Name\" = $1) AND (\"test_partner\".\"Category\" IN $2))", domainQuery.Where);
        Assert.Equivalent(new List<object> { "Test", new List<int> { 1, 2, 3 } }, domainQuery.Arguments);
        Assert.Equivalent(new[]
        {
            "LEFT JOIN \"test_category\" AS \"test_partner.Category\" ON \"test_partner\".\"Category\" = \"test_partner.Category\".\"id\"",
        }, domainQuery.LeftJoins);

    }

    [Fact]
    public void TestLeftJoinWithId()
    {
        Query.DomainQuery domainQuery = Query.DomainToQuery(_finalModel, [("Category.Name", "=", "Test"), ("Category.Partners.Id", "in", new List<int> { 1, 2, 3 })]);
        
        Assert.Equal("((\"test_partner.Category\".\"Name\" = $1) AND (\"test_partner.Category.Partners\".\"Id\" IN $2))", domainQuery.Where);
        Assert.Equivalent(new List<object> { "Test", new List<int> { 1, 2, 3 } }, domainQuery.Arguments);
        Assert.Equivalent(new[]
        {
            "LEFT JOIN \"test_category\" AS \"test_partner.Category\" ON \"test_partner\".\"Category\" = \"test_partner.Category\".\"id\"",
            "LEFT JOIN \"test_partner\" AS \"test_partner.Category.Partners\" ON \"test_partner.Category\".\"id\" = \"test_partner.Category.Partners\".\"Category\"",
        }, domainQuery.LeftJoins);
        
        // Same as above, but without ".Id" in domain
        domainQuery = Query.DomainToQuery(_finalModel, [("Category.Name", "=", "Test"), ("Category.Partners", "in", new List<int> { 1, 2, 3 })]);
        
        Assert.Equal("((\"test_partner.Category\".\"Name\" = $1) AND (\"test_partner.Category.Partners\".\"Id\" IN $2))", domainQuery.Where);
        Assert.Equivalent(new List<object> { "Test", new List<int> { 1, 2, 3 } }, domainQuery.Arguments);
        Assert.Equivalent(new[]
        {
            "LEFT JOIN \"test_category\" AS \"test_partner.Category\" ON \"test_partner\".\"Category\" = \"test_partner.Category\".\"id\"",
            "LEFT JOIN \"test_partner\" AS \"test_partner.Category.Partners\" ON \"test_partner.Category\".\"id\" = \"test_partner.Category.Partners\".\"Category\"",
        }, domainQuery.LeftJoins);
    }
}
