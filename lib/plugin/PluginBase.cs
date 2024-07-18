namespace lib.plugin;

// TODO Use Attribute instead of interface
public interface IPlugin
{
    string Id { get; }
    
    string Name { get; }
    
    string Version { get; }
    
    string[] Dependencies => [];

    List<Type> GetModels();
    
    /**
     * Called before installing this plugin
     */
    void OnInstalling(Environment env) {}

    /**
     * Called once this plugin is installed
     */
    void OnInstalled(Environment env) {}
    
    /**
     * Called when this plugin is starting
     */
    void OnStart(Environment env) {}
    
    /**
     * Called when this plugin is stopping
     */
    void OnStop(Environment env) {}
    
    /**
     * Called when uninstalling this plugin
     */
    void OnUninstall(Environment env) {}
}
