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
        
        using (ITestRunner runner = engine.GetRunner(package))
        {
            // execute the tests
            XmlNode result = runner.Run(new DefaultTestEventListener(), TestFilter.Empty);
            Console.WriteLine($"{result.Name} - {result.InnerXml}");
        }
    }

    public class GenericTest
    {
        [SetUp]
        public void GenericSetUp()
        {
            Console.WriteLine($"WE ARE IN GenericSetUp");
        }
    }
    
    [TestFixture]
    public class MyTests: GenericTest
    {
        [SetUp]
        public void SetUp()
        {
            Console.WriteLine($"WE ARE IN MyTests");
        }
        
        [Test]
        public void TestA()
        {
            Assert.That("a", Is.EqualTo("a"));
        }
    }
}
