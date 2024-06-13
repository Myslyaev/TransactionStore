﻿using TransactionStore.Core.Enums;
using TransactionStore.Core.Models.Requests;

namespace TransactionStore.API.Tests;

public static class TransactionsControllerTestData
{
    public static DepositWithdrawRequest GetDepositRequest() => new()
    {
        AccountId = Guid.NewGuid(),
        CurrencyType = Currency.USD,
        Amount = 100
    };

    public static DepositWithdrawRequest GetWithdrawRequest() => new()
    {
        AccountId = Guid.NewGuid(),
        CurrencyType = Currency.USD,
        Amount = 100
    };

    public static TransferRequest GetTransferRequest() => new()
    {
        AccountFromId = Guid.NewGuid(),
        AccountToId = Guid.NewGuid(),
        CurrencyFromType = Currency.RUB,
        CurrencyToType = Currency.EUR,
        Amount = 100
    };
}