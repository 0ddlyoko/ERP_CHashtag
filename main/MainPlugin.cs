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
        Partner partner1 = env.Create<Partner>();
        Partner2 partner2 = partner1.Transform<Partner2>();
        
        Console.WriteLine($"Empty: Partner1 Name = {partner1.Name}");
        Console.WriteLine($"Empty: Partner2 Name = {partner2.Name}");
        
        partner1.Name = "0ddlyoko";
        Console.WriteLine($"Name: Partner1 Name = {partner1.Name}");
        Console.WriteLine($"Name: Partner2 Name = {partner2.Name}");
        
        partner1.Save();
        Console.WriteLine($"Saved: Partner1 Name = {partner1.Name}");
        Console.WriteLine($"Saved: Partner2 Name = {partner2.Name}");
        
        partner1.Update(new Dictionary<string, object?>
        {
            {"Name", "1ddlyoko"},
        });
        Console.WriteLine($"Saved: Partner1 Name = {partner1.Name}");
        Console.WriteLine($"Saved: Partner2 Name = {partner2.Name}");
    }
}
