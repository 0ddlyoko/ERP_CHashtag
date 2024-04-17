using System.Reflection;
using CommandLine;
using lib.plugin;
using TypeInfo = CommandLine.TypeInfo;


namespace lib;

internal static class Program
{
    private static void Main(string[] args)
    {
        try
        {
            var parser = Parser.Default.ParseArguments<Config>(args);

            if (parser.Tag == ParserResultType.NotParsed)
            {
                return;
            }
            
            var pluginManager = new PluginManager(parser.Value.PluginsPath);
            Console.WriteLine("Registering plugins ...");
            pluginManager.RegisterPlugins();
            Console.WriteLine($"{pluginManager.PluginSize} plugins registered!");

            Console.WriteLine("Plugins:");
            foreach (var plugin in pluginManager.Plugins)
            {
                Console.WriteLine($"- {plugin.Id}: {plugin.IsInstalled}");
            }

            Console.WriteLine("Commands:");
            foreach (var command in pluginManager.Commands)
            {
                Console.WriteLine($"- {command.Name}: {command.Description}");
            }
            
            Console.WriteLine($"Installing {string.Join(", ", parser.Value.Install)}");
            Console.WriteLine($"Updating {string.Join(", ", parser.Value.Update)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
