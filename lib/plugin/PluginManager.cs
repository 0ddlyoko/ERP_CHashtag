using System.Reflection;
using lib.command;
using lib.model;
using lib.util;

namespace lib.plugin;

public class PluginManager(string pluginPath)
{
    private readonly Dictionary<string, APlugin> _availablePlugins = new();
    private readonly Dictionary<string, APlugin> _plugins = new();
    private readonly Dictionary<string, ICommand> _commands = new();
    private readonly Dictionary<string, List<PluginModel>> _pluginModels = new();
    private readonly Dictionary<string, FinalModel> _models = new();

    public readonly List<APlugin> PluginsInDependencyOrder = [];
    public IEnumerable<APlugin> AvailablePlugins => _availablePlugins.Values;
    public int AvailablePluginsSize => _availablePlugins.Count;

    public IEnumerable<APlugin> Plugins => _plugins.Values;
    public int PluginsSize => _plugins.Count;

    public IEnumerable<ICommand> Commands => _commands.Values;
    public int CommandsSize => _commands.Count;

    public IEnumerable<FinalModel> Models => _models.Values;
    public int ModelsSize => _models.Count;
    public int TotalModelsSize => _pluginModels.Values.SelectMany(p => p).Count();

    public void RegisterPlugins()
    {
        if (_availablePlugins.Count != 0)
        {
            throw new InvalidOperationException("Cannot register plugins if plugins are already registered");
        }

        if (pluginPath == null)
            throw new InvalidOperationException("Invalid root directory for plugins!");
        if (!Directory.Exists(pluginPath))
            throw new InvalidOperationException($"Plugin directory not found! {pluginPath}");
        var files = Directory.GetFiles(pluginPath, "*.dll");
        // Import files
        foreach (var file in files)
        {
            try
            {
                RegisterPlugin(file);
            }
            catch (Exception)
            {
                Console.Error.WriteLine($"Cannot register plugin {file}: Probably Not a plugin");
                throw;
            }
        }

        LoadPlugins();
        foreach (var plugin in Plugins)
        {
            plugin.Plugin.OnStart();
        }
    }

    private void RegisterPlugin(string pluginLocation)
    {
        // Check if it's a symlink
        var fileInfo = new FileInfo(pluginLocation);
        if (fileInfo.LinkTarget != null)
            pluginLocation = fileInfo.LinkTarget;
        var loadContext = new PluginLoadContext(pluginLocation);
        Assembly assembly = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
        var plugin = new APlugin(assembly);
        _availablePlugins[plugin.Id] = plugin;
    }

    public IEnumerable<PluginModel> GetModelsInDependencyOrder(string modelName)
    {
        var models = _pluginModels[modelName].ToLookup(pluginModel => pluginModel.Plugin);
        if (models.Count == 0)
            return [];
        var pluginToFilterOn = models.Select(m => m.Key).ToHashSet();
        return PluginsInDependencyOrder.Where(plugin => pluginToFilterOn.Contains(plugin)).SelectMany(pl => models[pl]);
    }

    public APlugin? GetPlugin(string pluginName)
    {
        _availablePlugins.TryGetValue(pluginName.ToLower(), out var plugin);
        return plugin;
    }

    public APlugin? GetInstalledPlugin(string pluginName)
    {
        _plugins.TryGetValue(pluginName.ToLower(), out var plugin);
        return plugin;
    }

    public bool IsPluginInstalled(string pluginName) => _availablePlugins.ContainsKey(pluginName.ToLower());

    /**
     * Install given plugin and all his dependencies
     */
    public void InstallPlugin(APlugin plugin)
    {
        if (GetPlugin(plugin.Name) != plugin)
            throw new InvalidOperationException("This plugin is not registered, or is not the same as given one");
        if (plugin.State is APlugin.PluginState.ToUninstall)
            plugin.State = APlugin.PluginState.Installed;
        if (plugin.State == APlugin.PluginState.Installed)
            return;
        try
        {
            SetPluginToInstall(plugin);
        }
        catch (Exception)
        {
            // Exception while setting plugins and dependencies as installing.
            // This means a dependency was not found. Roll back all "To Install" plugins
            foreach (var pl in AvailablePlugins.Where(pl => pl.State == APlugin.PluginState.ToInstall))
            {
                pl.State = APlugin.PluginState.NotInstalled;
            }
            throw;
        }
        InstallNeededPlugins();
    }

    public void InstallNeededPlugins()
    {
        var pluginsToInstall = AvailablePlugins.Where(pl => pl.State == APlugin.PluginState.ToInstall).ToList();
        Console.WriteLine($"Installing {pluginsToInstall.Count} plugins");
        try
        {
            foreach (var plugin in pluginsToInstall)
            {
                _plugins[plugin.Id] = plugin;
                plugin.State = APlugin.PluginState.Installed;
                try
                {
                    plugin.Plugin.OnStart();
                }
                catch (Exception)
                {
                    _plugins.Remove(plugin.Id);
                    plugin.State = APlugin.PluginState.NotInstalled;
                    throw;
                }
            }
        }
        finally
        {
            LoadPlugins();
        }
    }

    /**
     * Mark given plugin to install, and all his dependencies
     */
    public void SetPluginToInstall(APlugin plugin)
    {
        plugin.State = APlugin.PluginState.ToInstall;
        foreach (string dependency in plugin.Dependencies)
        {
            APlugin? dependencyPlugin = GetPlugin(dependency);
            if (dependencyPlugin == null)
                throw new InvalidOperationException($"Plugin {dependencyPlugin} not found!");
            if (dependencyPlugin.State is APlugin.PluginState.Installed or APlugin.PluginState.ToInstall)
                continue;
            SetPluginToInstall(dependencyPlugin);
        }
    }

    public void UninstallPlugin(APlugin plugin)
    {
        if (GetPlugin(plugin.Name) != plugin)
            throw new InvalidOperationException("This plugin is not registered, or is not the same as given one!");
    }

    private void LoadPlugins()
    {
        LoadDependencies();
        LoadModels();
        LoadCommands();
    }

    private void LoadDependencies()
    {
        PluginsInDependencyOrder.Clear();
        var dependencies = new Dictionary<string, string[]>();
        foreach (var plugin in _plugins)
        {
            dependencies[plugin.Key] = plugin.Value.Dependencies;
        }

        List<string> pluginsOrdered = DependencyGraph.GetOrderedGraph(dependencies);
        foreach (var pluginOrdered in pluginsOrdered)
        {
            PluginsInDependencyOrder.Add(GetPlugin(pluginOrdered)!);
        }
    }

    private void LoadModels()
    {
        // Load models on installed plugins
        // TODO Load it in a specific order (based on depends)
        _pluginModels.Clear();
        _models.Clear();
        foreach (var plugin in PluginsInDependencyOrder)
        {
            foreach (var (id, models) in plugin.Models)
            {
                // Plugin models
                if (!_pluginModels.ContainsKey(id))
                    _pluginModels[id] = [];
                _pluginModels[id].AddRange(models);
                // Models
                foreach (var model in models)
                {
                    if (_models.TryGetValue(id, out var finalModel))
                        finalModel.MergeWith(model);
                    else
                        _models[id] = new FinalModel(model);
                }
            }
        }
    }

    private void LoadCommands()
    {
        // Load commands on installed plugins
        // TODO Load it in a specific order (based on depends)
        _commands.Clear();
        foreach (var plugin in Plugins)
        {
            foreach (var command in plugin.Commands)
            {
                _commands[command.Name] = command;
            }
        }
    }
}
