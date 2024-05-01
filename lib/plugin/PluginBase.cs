namespace lib.plugin;

// TODO Use Attribute instead of interface
public interface IPlugin
{
    string Id { get; }
    
    string Name { get; }
    
    string Version { get; }
    
    string[] Dependencies => [];

    List<Type> GetModels();

    void OnStart(Environment env) {}
    void OnStop(Environment env) {}
}
