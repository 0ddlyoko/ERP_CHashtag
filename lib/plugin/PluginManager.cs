using System.Reflection;

namespace lib.plugin;

public class PluginManager(string pluginPath)
{
    private readonly Dictionary<string, APlugin> _plugins = new();
    private readonly Dictionary<string, ICommand> _commands = new();

    public int PluginSize => _plugins.Count;
    public IEnumerable<APlugin> Plugins => _plugins.Values;
    public IEnumerable<APlugin> InstalledPlugins => _plugins.Values.Where(p => p.IsInstalled);
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
