using lib.plugin;

namespace HelloPlugin.commands;

public class HelloPluginCommand: ICommand
{
    public string Name => "hello";
    public string Description => "Hello Command";

    public int Execute()
    {
        Console.WriteLine("Hello world!!");
        return 0;
    }
}
