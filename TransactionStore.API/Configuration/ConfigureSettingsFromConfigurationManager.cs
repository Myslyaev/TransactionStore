using TransactionStore.API.Configuration.Constants;
using TransactionStore.API.Configuration.Exceptions;
using TransactionStore.Business.Services;

namespace TransactionStore.API.Configuration;

public static class ConfigureSettingsFromConfigurationManager
{
    private static async Task<Dictionary<string, string>> GetConfigurationSettings()
    {
        var httpClientService = new HttpClientService();
        var configurationSettings = await httpClientService.Get<Dictionary<string, string>>(ConfigurationSettings.ConfigurationServiceUrl, new CancellationToken());

        return configurationSettings;
    }

    public static async Task<Dictionary<string, string>> ReadSettingsFromConfigurationManager(this IConfiguration configuration)
    {
        var configurationSettings = await GetConfigurationSettings();
        SetValueFromConfigurationManager(configuration.GetSection(ConfigurationSettings.LogPath), configurationSettings);
        configuration.ReadSection(ConfigurationSettings.DatabaseSettings, configurationSettings);
        configuration.ReadSection(ConfigurationSettings.ComissionSettings, configurationSettings);
        configuration.ReadSection(ConfigurationSettings.ServicesUrlSettings, configurationSettings);

        return configurationSettings;
    }

    private static void ReadSection(this IConfiguration configuration, string keySection, Dictionary<string, string> configurationSettings)
    {
        var section = configuration.GetSection(keySection).GetChildren();
        foreach (var key in section)
        {
            SetValueFromConfigurationManager(key, configurationSettings);
        }
    }

    private static void SetValueFromConfigurationManager(IConfigurationSection key, Dictionary<string, string> configurationSettings)
    {
        var value = key.Value ?? throw new NoConfigurationException("Value of configuration key is null.");
        if (!configurationSettings.TryGetValue(value, out var configurationSetting))
        {
            throw new NoConfigurationException("Configuration manager variables are not specified.");
        }
        key.Value = configurationSetting;
    }

    public static void UpdateSettingsFromConfigurationManager(this IConfiguration configuration, Dictionary<string, string> settings)
    {
        var defaultSection = configuration.GetSection(ConfigurationSettings.DefaultConfigurationSection);
        UpdateValueFromConfigurationManager(defaultSection.GetSection(ConfigurationSettings.LogPath), configuration.GetSection(ConfigurationSettings.LogPath), settings);
        configuration.UpdateSection(ConfigurationSettings.DatabaseSettings, settings);
        configuration.UpdateSection(ConfigurationSettings.ComissionSettings, settings);
        configuration.UpdateSection(ConfigurationSettings.ServicesUrlSettings, settings);
    }

    private static void UpdateSection(this IConfiguration configuration, string keySection, Dictionary<string, string> configurationSettings)
    {
        var sourceSection = configuration.GetSection(ConfigurationSettings.DefaultConfigurationSection).GetSection(keySection).GetChildren(); ;
        var destinationSection = configuration.GetSection(keySection).GetChildren();

        var sourceKeys = sourceSection.Select(x => x.Key).ToList();
        var destinationKeys = destinationSection.Select(x => x.Key).ToList();

        for (var i = 0; i < sourceKeys.Count; i++)
        {
            var sourceKey = configuration.GetSection($"{ConfigurationSettings.DefaultConfigurationSection}:{keySection}:{sourceKeys[i]}");
            var destinationKey = configuration.GetSection($"{keySection}:{destinationKeys[i]}");
            UpdateValueFromConfigurationManager(sourceKey, destinationKey, configurationSettings);
        }
    }

    private static void UpdateValueFromConfigurationManager(IConfigurationSection sourceKey, IConfigurationSection destinationKey, Dictionary<string, string> configurationSettings)
    {
        var value = sourceKey.Value ?? throw new NoConfigurationException("Value of configuration key is null");
        if (!configurationSettings.TryGetValue(value, out var configurationSetting))
        {
            throw new NoConfigurationException("Configuration manager variables are not specified");
        }
        destinationKey.Value = configurationSetting;
    }
}