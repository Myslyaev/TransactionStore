using TransactionStore.Core.DTOs;
using TransactionStore.Core.Enums;
using TransactionStore.Core.Models.Requests;

namespace TransactionStore.Business.Tests;

public static class TransactionsServiceTestData
{
    public static DepositWithdrawRequest GetValidDepositWithdrawRequest(Guid accountId) => new()
    {
        AccountId = accountId,
        Currency = Currency.USD,
        Amount = 100
    };

    public static DepositWithdrawRequest GetInvalidDepositWithdrawRequest() => new()
    {
        AccountId = Guid.NewGuid(),
        Currency = Currency.USD,
        Amount = 0
    };

    public static TransferRequest GetValidTransferRequest() => new()
    {
        AccountFromId = Guid.NewGuid(),
        AccountToId = Guid.NewGuid(),
        CurrencyFrom = Currency.USD,
        CurrencyTo = Currency.EUR,
        Amount = 100
    };

    public static TransferRequest GetInvalidTransferRequest() => new()
    {
        AccountFromId = Guid.NewGuid(),
        AccountToId = Guid.NewGuid(),
        CurrencyFrom = Currency.RUB,
        CurrencyTo = Currency.EUR,
        Amount = 0
    };

    public static (decimal rateToUSD, decimal rateFromUsd) GetCurrencyRates() => (1.1m, 0.9m);

    public static TransactionDto CreateExpectedWithdrawTransaction(TransferRequest request, decimal commissionAmount)
    {
        var withdrawAmount = request.Amount - commissionAmount;
        return new TransactionDto
        {
            AccountId = request.AccountFromId,
            TransactionType = TransactionType.Transfer,
            Amount = -withdrawAmount
        };
    }

    public static TransactionDto CreateExpectedDepositTransaction(TransferRequest request, decimal withdrawAmount, decimal rateToUSD, decimal rateFromUsd)
    {
        var amountUsd = withdrawAmount * rateToUSD;
        return new TransactionDto
        {
            AccountId = request.AccountToId,
            TransactionType = TransactionType.Transfer,
            Amount = amountUsd * rateFromUsd
        };
    }

    public static List<TransactionDto> GetTransactions(Guid transactionId) =>
        [
            new TransactionDto { Id = transactionId, AccountId = Guid.NewGuid(), Amount = 100, TransactionType = TransactionType.Deposit },
            new TransactionDto { Id = transactionId, AccountId = Guid.NewGuid(), Amount = 50, TransactionType = TransactionType.Withdraw }
        ];

    public static List<TransactionDto> GetEmptyTransactions() => [];

    public static List<TransactionDto> GetTransactionsByAccountId(Guid accountId) =>
        [
            new TransactionDto { Id = Guid.NewGuid(), AccountId = accountId, Amount = 100, TransactionType = TransactionType.Deposit },
            new TransactionDto { Id = Guid.NewGuid(), AccountId = accountId, Amount = 50, TransactionType = TransactionType.Withdraw }
        ];
}