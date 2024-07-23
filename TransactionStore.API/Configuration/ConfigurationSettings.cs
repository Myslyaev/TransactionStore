namespace TransactionStore.API.Configuration.Constants;

public static class ConfigurationSettings
{
    public const string LogPath = "Serilog:WriteTo:1:Args:path";
    public const string DatabaseSettings = "DatabaseSettings";
    public const string ServicesUrlSettings = "ServicesUrlSettings";
    public const string ComissionSettings = "ComissionSettings";
    public const string ConfigurationServiceUrl = "https://194.87.210.5:13000/api/configuration?service=2";
    public const string DefaultConfigurationSection = "DefaultSettings";
}