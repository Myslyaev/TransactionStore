using MassTransit;
using TransactionStore.API.Consumers;
using TransactionStore.API.Filters;
using TransactionStore.Core.Data;
using TransactionStore.Core.Models;

namespace TransactionStore.API.Configuration;

public static class ConfigureServices
{
    public static void ConfigureApiServices(this IServiceCollection services, ConfigurationManager configurationManager)
    {
        services.AddControllers(config =>
        {
            config.Filters.Add(new GlobalFilter(configurationManager));
        });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.ConfigureDataBase(configurationManager);
        services.AddAutoMapper(typeof(TransactionsMappingProfile));
        services.AddMassTransit(x =>
        {
            x.AddConsumer<RatesInfoConsumer>();
            x.AddConsumer<SettingsConsumer>();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.ReceiveEndpoint("settings-rates", e =>
                {
                    e.ConfigureConsumer<RatesInfoConsumer>(context);
                });
                cfg.ReceiveEndpoint("settings-t-store", e =>
                {
                    e.Bind("configurations-exchange", x =>
                    {
                        x.ExchangeType = "fanout";
                    });
                    e.ConfigureConsumer<SettingsConsumer>(context);
                });
            });
        });
        services.AddSingleton<ICurrencyRatesProvider, CurrencyRatesProvider>();
        services.AddScoped<ICommissionsProvider, CommissionsProvider>();
        services.AddConfigurationServicesFromJson(configurationManager);
    }
}