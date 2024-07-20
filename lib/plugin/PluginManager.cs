using System.Reflection;
using lib.command;
using lib.database;
using lib.field;
using lib.model;
using lib.util;

namespace lib.plugin;

public class PluginManager(Config config)
{
    public readonly Config Config = config;
    public readonly DatabaseConnectionConfig DatabaseConnectionConfig = new(config.DatabaseHostname, config.DatabasePort, config.DatabaseName, config.DatabaseUser, config.DatabasePassword);
    private readonly Dictionary<string, APlugin> _availablePlugins = new();
    private readonly Dictionary<string, APlugin> _plugins = new();
    private readonly Dictionary<string, ICommand> _commands = new();
    private readonly Dictionary<string, List<PluginModel>> _pluginModels = new();
    private readonly Dictionary<Type, PluginModel> _typeToPluginModel = new();
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

        if (Config.PluginsPath == null)
            throw new InvalidOperationException("Invalid root directory for plugins!");
        if (!Directory.Exists(Config.PluginsPath))
            throw new InvalidOperationException($"Plugin directory not found! {Config.PluginsPath}");
        var files = Directory.GetFiles(Config.PluginsPath, "*.dll");
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

        // Plugins are registered, now enable already installed plugins
        // TODO
        
        LoadPlugins();
        Environment env = new(this);
        foreach (var plugin in Plugins)
        {
            plugin.Plugin.OnStart(env);
        }
    }

    public void RegisterPlugin(string pluginLocation)
    {
        // Check if it's a symlink
        var fileInfo = new FileInfo(pluginLocation);
        if (fileInfo.LinkTarget != null)
            pluginLocation = fileInfo.LinkTarget;
        var loadContext = new PluginLoadContext(pluginLocation);
        Assembly assembly = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
        RegisterPlugin(assembly);
    }

    public void RegisterPlugin(Assembly assembly)
    {
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

    public bool IsPluginInstalled(string pluginName) => _plugins.ContainsKey(pluginName.ToLower());

    /**
     * Load the main plugin.
     * This should be called before loading all other plugins
     */
    public async Task LoadMain()
    {
        var plugin = GetPlugin("main");
        if (plugin == null)
            throw new InvalidOperationException("Plugin \"main\" not found. Please check your plugin path");
        await InstallPluginNow(plugin);
    }

    /**
     * Install given plugin and all his dependencies
     */
    public async void InstallPlugin(APlugin plugin)
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
        await InstallNeededPlugins();
    }

    public async Task InstallNeededPlugins()
    {
        var pluginsToInstall = AvailablePlugins.Where(pl => pl.State == APlugin.PluginState.ToInstall).ToList();
        Console.WriteLine($"Installing {pluginsToInstall.Count} plugins");
        try
        {
            foreach (var plugin in pluginsToInstall)
            {
                await InstallPluginNow(plugin);
            }
        }
        finally
        {
            // Call it here, as it's possible that the installation fails
            LoadPlugins();
        }
    }

    /**
     * Install given plugin now, without installing other plugins
     */
    public async Task InstallPluginNow(APlugin plugin)
    {
        if (GetPlugin(plugin.Name) != plugin)
            throw new InvalidOperationException("This plugin is not registered, or is not the same as given one");
        if (plugin.State is APlugin.PluginState.ToUninstall)
            plugin.State = APlugin.PluginState.Installed;
        if (plugin.State == APlugin.PluginState.Installed)
            return;
        
        // Check if all dependencies are installed
        foreach (var dependency in plugin.Dependencies)
        {
            var dependencyPlugin = GetPlugin(dependency);
            if (dependencyPlugin == null)
                throw new InvalidOperationException($"Plugin {plugin.Name} require dependency {dependency}, but this dependency is not available");
            await InstallPluginNow(dependencyPlugin);
        }
        
        Console.WriteLine($"Installing plugin {plugin.Name}");
        _plugins[plugin.Id] = plugin;
        plugin.State = APlugin.PluginState.Installed;
        try
        {
            // Yeah, we call this method in the for loop.
            // We need to do it to be able to install plugins one by one.
            // At least, if a plugin is failing to install, other plugins are installed
            LoadPlugins();
            Environment env = new(this);
            plugin.Plugin.OnInstalling(env);
            // Save models
            foreach (var (modelName, models) in plugin.Models)
            {
                var finalModel = GetFinalModel(modelName);
                await DatabaseHelper.CreateTable(env.Connection, modelName, finalModel.Description);
                foreach (var pluginModel in models)
                {
                    HashSet<FinalField> fields = [];
                    foreach (var (fieldName, field) in pluginModel.Fields)
                    {
                        if (field.FieldName != "Id" && field.FieldType is not FieldType.OneToMany and not FieldType.ManyToMany)
                            fields.Add(finalModel.Fields[fieldName]);
                    }
                    foreach (var finalField in fields)
                    {
                        await DatabaseHelper.CreateColumn(
                            connection: env.Connection,
                            tableName: modelName,
                            columnName: finalField.FieldName,
                            columnType: DatabaseHelper.FieldTypeToString(finalField.FieldType),
                            required: false,
                            targetTableName: finalField.TargetFinalModel?.Name,
                            targetColumnName: finalField.TargetFinalField?.FieldName
                        );
                    }
                }
            }
            Console.WriteLine($"Plugin installed using {env.Connection.NumberOfRequests} requests");
        }
        catch (Exception)
        {
            _plugins.Remove(plugin.Id);
            plugin.State = APlugin.PluginState.NotInstalled;
            throw;
        }
    }

    /**
     * Mark given plugin to install, and all his dependencies
     */
    public void SetPluginToInstall(APlugin plugin)
    {
        if (plugin.State != APlugin.PluginState.NotInstalled)
            return;
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

    public PluginModel GetPluginModelFromType(Type type) => _typeToPluginModel[type];

    public FinalModel GetFinalModel(string model) => _models[model];

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
        _typeToPluginModel.Clear();
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
                        _models[id] = new FinalModel(this, model);
                    _typeToPluginModel[model.Type] = model;
                }
            }
        }
        // Post loading
        foreach (var (_, model) in _models)
        {
            model.PostLoading(this);
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
