using TransactionStore.Core.Enums;

namespace TransactionStore.Core.Models.Responses;

public class FullTransactionResponse
{
    public Guid Id { get; set; }
    public Guid AccountFromId { get; set; }
    public Guid AccountToId { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}