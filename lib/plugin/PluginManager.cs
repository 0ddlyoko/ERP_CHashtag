using System.Reflection;

namespace lib.plugin;

public class PluginManager(string pluginPath)
{
    private readonly Dictionary<string, APlugin> _plugins = new();
    private readonly Dictionary<string, ICommand> _commands = new();

    public int PluginSize => _plugins.Count;
    public IEnumerable<APlugin> Plugins => _plugins.Values;
    public IEnumerable<APlugin> InstalledPlugins => _plugins.Values.Where(p => p.IsInstalled);
    public int InstalledPluginSize => InstalledPlugins.Count();
    
    public IEnumerable<ICommand> Commands => _commands.Values;

    public void RegisterPlugins()
    {
        if (_plugins.Count != 0)
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
            RegisterPlugin(file);
        }

        LoadCommands();
    }

    private void RegisterPlugin(string pluginLocation)
    {
        var loadContext = new PluginLoadContext(pluginLocation);
        Assembly assembly = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
        var plugin = new APlugin(assembly);
        _plugins[plugin.Id] = plugin;
    }

    public APlugin? GetPlugin(string pluginName)
    {
        _plugins.TryGetValue(pluginName, out var plugin);
        return plugin;
    }

    public APlugin? GetInstalledPlugin(string pluginName)
    {
        _plugins.TryGetValue(pluginName, out var plugin);
        if (plugin is not { IsInstalled: true })
            return null;
        return plugin;
    }

    public bool IsPluginInstalled(string pluginName) => GetPlugin(pluginName)?.IsInstalled ?? false;

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
            foreach (var pl in Plugins.Where(pl => pl.State == APlugin.PluginState.ToInstall))
            {
                pl.State = APlugin.PluginState.NotInstalled;
            }
            throw;
        }
        InstallNeededPlugins();
    }

    public void InstallNeededPlugins()
    {
        var pluginsToInstall = Plugins.Where(pl => pl.State == APlugin.PluginState.ToInstall).ToList();
        Console.WriteLine($"Installing {pluginsToInstall.Count} plugins");
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

    private void LoadCommands()
    {
        // Load commands on installed plugins
        // TODO Load it in a specific order (based on depends)
        foreach (var plugin in Plugins)
        {
            if (!plugin.IsInstalled)
                continue;
            foreach (var command in plugin.Commands)
            {
                _commands[command.Name] = command;
            }
        }
    }
}
