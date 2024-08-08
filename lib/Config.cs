using CommandLine;

namespace lib;

public class Config
{
    // Database
    [Option("db_host", Required = false, HelpText = "Database hostname", Default = "127.0.0.1")]
    public string DatabaseHostname { get; set; } = "localhost";
    
    [Option("db_port", Required = false, HelpText = "Database port", Default = 5432)]
    public int DatabasePort { get; set; } = 5432;

    [Option("db_name", Required = false, HelpText = "Database name", Default = "erp")]
    public string DatabaseName { get; set; } = "erp";
    
    [Option("db_user", Required = false, HelpText = "Database user", Default = "postgres")]
    public string DatabaseUser { get; set; } = "postgres";
    
    [Option("db_password", Required = false, HelpText = "Database password", Default = "postgres")]
    public string DatabasePassword { get; set; } = "postgres";

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
