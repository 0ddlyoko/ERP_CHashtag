namespace lib.plugin;

public interface ICommand
{
    string Name { get; }
    string Description { get; }

    int Execute();
}

public interface IPlugin
{
    string Id { get; }
    
    string Name { get; }
    
    string Version { get; }
    
    string[] Dependencies => [];
}
