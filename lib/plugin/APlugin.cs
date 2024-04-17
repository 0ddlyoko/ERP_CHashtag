using System.Reflection;

namespace lib.plugin;

public class APlugin(Assembly assembly)
{
    public readonly IPlugin Plugin = GetOfType<IPlugin>(assembly).First();
    public string Id => Plugin.Id;
    public string Name => Plugin.Name;
    public string Version => Plugin.Version;
    public string[] Dependencies => Plugin.Dependencies;
    public readonly IEnumerable<ICommand> Commands = GetOfType<ICommand>(assembly);
    public bool IsInstalled { get; protected set; } = false;

    private static IEnumerable<T> GetOfType<T>(Assembly assembly) {
        foreach (var type in assembly.GetTypes()) 
        {
            if (!typeof(T).IsAssignableFrom(type))
                continue;
            var command = (T?) Activator.CreateInstance(type);
            if (command == null)
                continue;
            yield return command;
        }
    }
}