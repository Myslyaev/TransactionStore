using Messaging.Shared;
using TransactionStore.Core.Enums;
using TransactionStore.Core.Models.Requests;
using TransactionStore.Core.Models.Responses;

namespace TransactionStore.Business.Services;

public interface ITransactionsService
{
    Task<Guid> AddDepositWithdrawTransactionAsync(TransactionType transactionType, DepositWithdrawRequest request);
    Task<TransferGuidsResponse> AddTransferTransactionAsync(TransferRequest request);
    Task<FullTransactionResponse> GetTransactionByIdAsync(Guid id);
    Task<List<TransactionResponse>> GetTransactionsByAccountIdAsync(Guid id);
    Task<AccountBalanceResponse> GetBalanceByAccountIdAsync(Guid id);
    Task SetRates(RatesInfo rates);
}