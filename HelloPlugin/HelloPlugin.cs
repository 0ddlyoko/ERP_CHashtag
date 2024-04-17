using lib;

namespace HelloPlugin;

public class HelloPlugin: ICommand
{
    public string Name { get => "hello"; }
    public string Description { get => "Hello Command"; }
    public int Execute()
    {
        Console.WriteLine("Hello world!!");
        return 0;
    }
}
