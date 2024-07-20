using CommandLine;

namespace lib;

public class Config
{
    // Database
    [Option("db_host", Required = true, HelpText = "Database hostname")]
    public string DatabaseHostname { get; set; } = "";
    
    [Option("db_port", Required = false, HelpText = "Database port", Default = 5432)]
    public int DatabasePort { get; set; } = 5433;

    [Option("db_name", Required = true, HelpText = "Database name")]
    public string DatabaseName { get; set; } = "";
    
    [Option("db_user", Required = true, HelpText = "Database user")]
    public string DatabaseUser { get; set; } = "";
    
    [Option("db_password", Required = true, HelpText = "Database password")]
    public string DatabasePassword { get; set; } = "";

    // Install / Update
    [Option('i', "install", Required = false, HelpText = "Install listed plugins")]
    public string InstallString { private get; set; } = "";
    public string[] Install => InstallString.Split(',');

    [Option('u', "update", Required = false, HelpText = "Update listed plugins")]
    public string UpdateString { private get; set; } = "";
    public string[] Update => UpdateString.Split(',');
    
    // Required
    [Option('p', "plugins", Required = true, HelpText = "Plugin path")]
    public string PluginsPath { get; set; } = "";
    
    // Non required
}
