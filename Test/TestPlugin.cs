using lib.plugin;
using Test.data.models;
using Environment = lib.Environment;

namespace Test;

public class TestPlugin: IPlugin
{
    public int NumberOfOnStart = 0;
    public int NumberOfOnEnd = 0;
    
    public string Id => "test";
    public string Name => "Test";
    public string Version => "1.0.0";
    public List<Type> GetModels()
    {
        return
        [
            typeof(TestPartner),
            typeof(TestPartner2),
            typeof(TestPartner3),
            typeof(TestCategory),
            typeof(TestMultipleRecompute),
            typeof(TestModel2),
        ];
    }

    public void OnStart(Environment env)
    {
        NumberOfOnStart++;
    }

    public void OnStop(Environment env)
    {
        NumberOfOnEnd++;
    }
}