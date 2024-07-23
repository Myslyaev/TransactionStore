using TransactionStore.API.Configuration.Constants;
using TransactionStore.Core.Settings;

namespace TransactionStore.API.Configuration;

public static class ConfigureServicesFromJson
{
    public static void AddConfigurationServicesFromJson(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped(sp => configuration.GetSection(ConfigurationSettings.ComissionSettings)
            .Get<ComissionSettings>(options => options.BindNonPublicProperties = true));
    }
}