using TransactionStore.Core.Enums;

namespace TransactionStore.Core.Models.Requests;

public class DepositWithdrawRequest
{
    public Guid AccountId { get; set; }
    public Currency Currency { get; set; }
    public decimal Amount { get; set; }
}