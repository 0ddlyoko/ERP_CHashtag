using lib.plugin;
using main.models;
using Environment = lib.Environment;

namespace main;

public class MainPlugin: IPlugin
{
    public string Id => "main";
    public string Name => "Main";
    public string Version => "1.0.0";

    public void OnStart(Environment env)
    {
        Partner partner = env.Create<Partner>();
        partner.Name = "0ddlyoko";
        
        Console.WriteLine($"Partner created! Name = {partner.Name}");
        env.ResetModelToCacheState(partner);
        Console.WriteLine($"Partner created! Name = {partner.Name}");
    }
}
