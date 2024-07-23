using MassTransit;
using Messaging.Shared;
using Serilog;
using System.Text.Json;
using TransactionStore.API.Configuration;
using TransactionStore.Core.Enums;

namespace TransactionStore.API.Consumers;

public class SettingsConsumer(IConfiguration configuration) : IConsumer<ConfigurationMessage>
{
    private readonly Serilog.ILogger _logger = Log.ForContext<SettingsConsumer>();
    public Task Consume(ConsumeContext<ConfigurationMessage> context)
    {
        if (context.Message.ServiceType != ServiceType.transactionStore)
        {
            return Task.CompletedTask;
        }
        var jsonMessage = JsonSerializer.Serialize(context.Message.Configurations);
        _logger.Information($"Consuming message: {jsonMessage}.");
        configuration.UpdateSettingsFromConfigurationManager(context.Message.Configurations);

        return Task.CompletedTask;
    }
}