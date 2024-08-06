namespace Test;

internal static class Program
{
    // public static void Main(string[] args)
    // {
    //     // set up the options
    //     string path = Assembly.GetExecutingAssembly().Location;
    //     TestPackage package = new TestPackage(path);
    //     package.AddSetting("WorkDirectory", Environment.CurrentDirectory);
    //     
    //     // prepare the engine
    //     ITestEngine engine = TestEngineActivator.CreateInstance();
    //     
    //     using (ITestRunner runner = engine.GetRunner(package))
    //     {
    //         // execute the tests
    //         XmlNode result = runner.Run(new DefaultTestEventListener(), TestFilter.Empty);
    //         Console.WriteLine($"{result.Name} - {result.InnerXml}");
    //     }
    // }

    public class GenericTest
    {
        public GenericTest()
        {
            Console.WriteLine($"WE ARE IN GenericSetUp");
        }
    }
    
    public class MyTests: GenericTest
    {

        public MyTests()
        {
            Console.WriteLine($"WE ARE IN MyTests");
        }
        
        [Fact]
        public void TestA()
        {
            Assert.Equal("a", "a");
        }
    }
}
