﻿using lib.@base.models;
using lib.plugin;

namespace lib.@base;

public class MainPlugin: IPlugin
{
    public string Id => "main";
    public string Name => "Main";
    public string Version => "1.0.0";

    public List<Type> GetModels()
    {
        return [
            typeof(Module),
            typeof(Partner),
            typeof(Partner2),
            typeof(Partner3),
        ];
    }

    public void OnStart(Environment env)
    {
        Partner partner1 = env.Create<Partner>([[]]);
        
        Console.WriteLine($"Name: {partner1.Name}");
        Console.WriteLine($"DisplayName (Test): {partner1.DisplayName}");
        
        partner1.Name = "0ddlyoko";
        Console.WriteLine($"Name: {partner1.Name}");
        Console.WriteLine($"DisplayName (0ddlyoko): {partner1.DisplayName}");
        
        partner1.Update(new Dictionary<string, object?>
        {
            {"Name", "1ddlyoko"},
            {"Age", 54},
        });
        Console.WriteLine($"Name: {partner1.Name}");
        Console.WriteLine($"DisplayName (1ddlyoko): {partner1.DisplayName}");
        Console.WriteLine($"Date (1ddlyoko): {partner1.Date}");
    }
}
