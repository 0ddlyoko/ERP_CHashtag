using System.Collections;
using System.Reflection;

namespace lib;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            if (args.Length == 1 && args[0] == "/d")
            {
                Console.WriteLine("Waiting for any key...");
                Console.ReadLine();
            }

            string[] pluginPaths = [
                "HelloPlugin.dll",
                // TODO
            ];
            List<Plugin> plugins = pluginPaths.Select(LoadPlugin).ToList();
            IEnumerable<ICommand> commands = plugins.SelectMany(plugin => plugin.Commands).ToList();
            
            // Load commands from plugins.
            if (args.Length == 0)
            {
                Console.WriteLine("Plugins: ");
                foreach (Plugin plugin in plugins)
                {
                    Console.WriteLine($"> {plugin.Name}");
                }
                
                Console.WriteLine("Commands: ");
                foreach (ICommand command in commands)
                {
                    Console.WriteLine($"> {command.Name}\t - {command.Description}");
                }
            }
            else
            {
                foreach (string commandName in args)
                {
                    Console.WriteLine($"-- {commandName} --");

                    // Execute the command with the name passed as an argument.

                    Console.WriteLine();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    static Plugin LoadPlugin(string pluginName)
    {
        // TODO Fix this
        string root = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(
                            Path.GetDirectoryName(typeof(Program).Assembly.Location)))))));
        root = Path.Combine(root, "lib/plugins");
        string pluginLocation = Path.GetFullPath(Path.Combine(root, pluginName.Replace('\\', Path.DirectorySeparatorChar)));
        
        PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
        Assembly assembly = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
        return new Plugin(assembly);
    }

    static IEnumerable<TType> CreateOfType<TType>(Assembly assembly)
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
}
