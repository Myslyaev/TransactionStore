using TransactionStore.Core.Enums;

namespace Messaging.Shared;

public class TransactionCreated
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public Currency Currency { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal AmountInRUB { get; set; }
}