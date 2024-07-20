using CommandLine;
using lib.model;
using lib.plugin;


namespace lib;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            var parser = Parser.Default.ParseArguments<Config>(args);
            if (parser.Tag == ParserResultType.NotParsed)
                return;
            
            var pluginManager = new PluginManager(parser.Value);
            Console.WriteLine("Registering plugins ...");
            pluginManager.RegisterPlugins();
            Console.WriteLine($"{pluginManager.AvailablePluginsSize} plugins registered!");
            
            Console.WriteLine("Loading \"main\", please wait");
            await pluginManager.LoadMain();
            Console.WriteLine($"{pluginManager.PluginsSize} plugins installed!");
            
            Console.WriteLine($"Installing {string.Join(", ", parser.Value.Install)}");
            foreach (var pluginToInstall in parser.Value.Install)
            {
                APlugin? plugin = pluginManager.GetPlugin(pluginToInstall);
                if (plugin == null)
                {
                    Console.Error.WriteLine($"Plugin {pluginToInstall} not found, skipping it");
                    continue;
                }
            
                pluginManager.SetPluginToInstall(plugin);
            }
            await pluginManager.InstallNeededPlugins();
            Console.WriteLine($"We have {pluginManager.PluginsSize} installed plugins");
            Console.WriteLine($"We have {pluginManager.CommandsSize} installed commands");
            Console.WriteLine($"We have {pluginManager.ModelsSize} installed models, with a total of {pluginManager.TotalModelsSize} override");
            
            Console.WriteLine($"Updating {string.Join(", ", parser.Value.Update)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
