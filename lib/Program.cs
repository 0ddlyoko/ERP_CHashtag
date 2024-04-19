﻿using CommandLine;
using lib.model;
using lib.plugin;


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
            Console.WriteLine($"{pluginManager.AvailablePluginsSize} plugins registered!");
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
            pluginManager.InstallNeededPlugins();
            Console.WriteLine($"We have {pluginManager.PluginsSize} installed plugins");
            Console.WriteLine($"We have {pluginManager.CommandsSize} installed commands");
            Console.WriteLine($"We have {pluginManager.ModelsSize} installed models");
            
            Console.WriteLine($"Updating {string.Join(", ", parser.Value.Update)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
