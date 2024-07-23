using System.Reflection;
using System.Xml;
using NUnit.Engine;

namespace Test;

internal static class Program
{
    public static void Main(string[] args)
    {
        // set up the options
        string path = Assembly.GetExecutingAssembly().Location;
        TestPackage package = new TestPackage(path);
        package.AddSetting("WorkDirectory", Environment.CurrentDirectory);
        
        // prepare the engine
        ITestEngine engine = TestEngineActivator.CreateInstance();
        var _filterService = engine.Services.GetService<ITestFilterService>();
        ITestFilterBuilder builder = _filterService.GetTestFilterBuilder();
        TestFilter emptyFilter = builder.GetFilter();
        
        using (ITestRunner runner = engine.GetRunner(package))
        {
            // execute the tests
            XmlNode result = runner.Run(null, emptyFilter);
            Console.WriteLine(result);
        }
    }
    
    [TestFixture]
    public class MyTests
    {
        [Test]
        public void TestA()
        {
            Assert.That("a", Is.EqualTo("a"));
        }
    }
}
