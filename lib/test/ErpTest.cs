using System.Reflection;
using lib.plugin;
using Xunit;

namespace lib.test;

/**
 * Class that implements basic testing util for this ERP.
 */
public class ErpTest: IAsyncLifetime
{
    // Static, so that it's only loaded one time
    private static Assembly _assembly = typeof(ErpTest).Assembly;
    // TODO Pass a real config here
    private static PluginManager _pluginManager = new(new Config())
    {
        Test = true,
    };

    private static bool _isLoaded = false;

    private static readonly string[] BlacklistKeys =
    [
        "System.",
        "xunit.",
        "JetBrains.",
        // Maybe remove later, if Microsoft wants to create their own modules /s
        "Microsoft.",
    ];

    private APlugin? _plugin;
    
    public ErpTest()
    {
        // _pluginManager.RegisterPlugins();

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        { 
            if (IsBlacklistAssembly(assembly))
                continue;

            var pluginType = assembly.GetTypes().Where(type => typeof(IPlugin).IsAssignableFrom(type));
            if (!pluginType.Any())
                continue;
            _pluginManager.RegisterPlugin(assembly);
        }
        
        // Install if needed the assembly that we are testing
        var callerAssembly = GetType().Assembly;
        _plugin = _pluginManager.GetPluginFromAssembly(callerAssembly);
    }

    private bool IsBlacklistAssembly(Assembly assembly)
    {
        foreach (var blacklistKey in BlacklistKeys)
        {
            if (assembly.GetName().Name?.Equals("lib") ?? false)
                return true;
            if (assembly.GetName().Name?.StartsWith(blacklistKey) ?? false)
                return true;
        }

        return false;
    }

    public Task InitializeAsync()
    {
        // TODO Fix why main is not installable (Assembly not loaded)
        return _pluginManager.LoadMain().ContinueWith(_ =>
        {
            if (_plugin != null)
            {
                return _pluginManager.InstallPluginNow(_plugin);
            }

            return null;
        });
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
