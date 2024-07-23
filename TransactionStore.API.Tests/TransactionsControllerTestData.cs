using TransactionStore.Core.Enums;
using TransactionStore.Core.Models.Requests;

namespace TransactionStore.API.Tests;

public static class TransactionsControllerTestData
{
    public static DepositWithdrawRequest GetDepositRequest() => new()
    {
        AccountId = Guid.NewGuid(),
        Currency = Currency.USD,
        Amount = 100
    };

    public static DepositWithdrawRequest GetWithdrawRequest() => new()
    {
        AccountId = Guid.NewGuid(),
        Currency = Currency.USD,
        Amount = 100
    };

    public static TransferRequest GetTransferRequest() => new()
    {
        AccountFromId = Guid.NewGuid(),
        AccountToId = Guid.NewGuid(),
        CurrencyFrom = Currency.RUB,
        CurrencyTo = Currency.EUR,
        Amount = 100
    };
}