using lib.plugin;

namespace HelloPlugin;

public class HelloPlugin: IPlugin
{
    public string Id => "hello";
    public string Name => "Hello";
    public string Version => "1.0.0";
    // public string[] Dependencies => ["test"];
}
