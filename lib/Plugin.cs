using System.Reflection;

namespace lib;

public class Plugin
{
    public string Name { get; }
    public List<ICommand> Commands { get; }

    public Plugin(Assembly assembly)
    {
        Name = assembly.GetName().Name ?? "???";
        Commands = GetOfType<ICommand>(assembly).ToList();
    }

    IEnumerable<TType> GetOfType<TType>(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (typeof(TType).IsAssignableFrom(type))
            {
                var command = (TType?) Activator.CreateInstance(type);
                if (command == null)
                    continue;
                yield return command;
            }
        }
    }
};
