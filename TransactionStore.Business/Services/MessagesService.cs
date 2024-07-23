using MassTransit;
using Messaging.Shared;
using Serilog;
using TransactionStore.Core.DTOs;
using TransactionStore.Core.Enums;

namespace TransactionStore.Business.Services;
public class MessagesService(IPublishEndpoint publishEndpoint) : IMessagesService
{
    private readonly ILogger _logger = Log.ForContext<MessagesService>();
    private Task _publish;

    public async Task PublishTransactionAsync(List<TransactionDto> transactions, Currency currency, decimal comissionAmount, decimal amountInRUB)
    {
        foreach (var t in transactions)
        {
            _publish = publishEndpoint.Publish<TransactionCreated>(new
            {
                t.Id,
                t.AccountId,
                t.TransactionType,
                t.Amount,
                t.Date,
                Currency = currency,
                CommissionAmount = comissionAmount,
                AmountInRUB = amountInRUB
            });

        }
        _logger.Information("Sending transaction info to RabbitMQ.");
        await Task.WhenAll(_publish);
    }
}