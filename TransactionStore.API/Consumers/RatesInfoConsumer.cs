using MassTransit;
using Messaging.Shared;
using Serilog;
using System.Text.Json;
using TransactionStore.Business.Services;

namespace TransactionStore.API.Consumers;

public class RatesInfoConsumer(ITransactionsService service) : IConsumer<RatesInfo>
{
    private readonly ITransactionsService _service = service;
    private readonly Serilog.ILogger _logger = Log.ForContext<RatesInfoConsumer>();

    public Task Consume(ConsumeContext<RatesInfo> context)
    {
        var messageJson = JsonSerializer.Serialize(context.Message, new JsonSerializerOptions { WriteIndented = true });
        _logger.Information($"Consuming message: {messageJson}.");

        _logger.Information("Setting currency rates.");
        _service.SetRates(context.Message);

        _logger.Information($"Getting currency rates: {JsonSerializer.Serialize(context.Message.Rates, new JsonSerializerOptions { WriteIndented = true })} from Rates Provider.");

        return Task.CompletedTask;
    }
}