using TransactionStore.Core.DTOs;
using TransactionStore.Core.Enums;

namespace TransactionStore.Business.Services;

public interface IMessagesService
{
    Task PublishTransactionAsync(List<TransactionDto> transactions, Currency currency, decimal comissionAmount, decimal amountInRUB);
}