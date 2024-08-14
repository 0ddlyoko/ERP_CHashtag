﻿using System.Reflection;
using lib.command;
using lib.model;

namespace lib.plugin;

public class APlugin
{
    public readonly IPlugin Plugin;
    public string Id => Plugin.Id.ToLower();
    public string Name => Plugin.Name;
    public string Version => Plugin.Version;
    public string[] Dependencies => Plugin.Dependencies;
    public readonly List<ICommand> Commands;
    public readonly Dictionary<string, List<PluginModel>> Models;
    public bool IsInstalled => State == PluginState.Installed;
    public PluginState State { get; internal set; } = PluginState.NotInstalled;

    public APlugin(Assembly assembly)
    {
        var pluginType = GetOfType<IPlugin>(assembly).First();
        var plugin = (IPlugin?) Activator.CreateInstance(pluginType);
        Plugin = plugin ?? throw new InvalidOperationException($"Cannot create an instance of {pluginType}");

        // Load commands
        Commands = [];
        // TODO Get list of commands from a method like GetModels()
        foreach (var commandType in GetOfType<ICommand>(assembly))
        {
            var command = (ICommand?)Activator.CreateInstance(commandType);
            if (command == null)
                throw new InvalidOperationException($"Cannot create an instance of command {commandType}");
            Commands.Add(command);
        }

        // Load models
        Models = new Dictionary<string, List<PluginModel>>();
        List<Type> models = Plugin.GetModels();
        foreach (var modelType in models)
        {
            if (!typeof(Model).IsAssignableFrom(modelType))
                throw new InvalidOperationException($"Given class {modelType.Name} is not a Model!");
            var modelDefinition = modelType.GetCustomAttribute<ModelDefinitionAttribute>();
            if (modelDefinition == null)
                throw new InvalidOperationException($"Model class {modelType} does not have attribute ModelDefinitionAttribute");
            if (!Models.ContainsKey(modelDefinition.Name))
                Models[modelDefinition.Name] = [];
            var pluginModel = new PluginModel(this, modelDefinition, modelType);
            Models[modelDefinition.Name].Add(pluginModel);
        }
    }

    private static IEnumerable<Type> GetOfType<T>(Assembly assembly)
    {
        return assembly.GetTypes().Where(type => typeof(T).IsAssignableFrom(type) && type != typeof(T));
    }

    public enum PluginState
    {
        NotInstalled,
        ToInstall,
        Installed,
        ToUninstall,
    }
}
