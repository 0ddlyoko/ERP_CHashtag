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
    protected static readonly PluginManager PluginManager = new(new Config())
    {
        Test = true,
    };
    protected readonly APlugin? Plugin;
    protected Environment Env;

    private static readonly string[] BlacklistKeys =
    [
        "System.",
        "xunit.",
        "JetBrains.",
        // Maybe remove later, if Microsoft wants to create their own modules /s
        "Microsoft.",
    ];
    
    protected ErpTest()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        { 
            if (IsBlacklistAssembly(assembly))
                continue;

            var pluginType = assembly.GetTypes().Where(type => typeof(IPlugin).IsAssignableFrom(type) && type != typeof(IPlugin));
            if (!pluginType.Any())
                continue;
            PluginManager.RegisterPlugin(assembly);
        }
        
        // Install if needed the assembly that we are testing
        var callerAssembly = GetType().Assembly;
        Plugin = PluginManager.GetPluginFromAssembly(callerAssembly);
    }

    private bool IsBlacklistAssembly(Assembly assembly)
    {
        foreach (var blacklistKey in BlacklistKeys)
        {
            if (assembly.GetName().Name?.StartsWith(blacklistKey) ?? false)
                return true;
        }

        return false;
    }

    public async Task InitializeAsync()
    {
        await PluginManager.LoadMain();
        if (Plugin != null)
            await PluginManager.InstallPluginNow(Plugin);
        Env = new Environment(PluginManager);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
