using TransactionStore.Core.DTOs;
using TransactionStore.Core.Models.Responses;

namespace TransactionStore.DataLayer.Repositories;

public interface ITransactionsRepository
{
    Task<Guid> AddDepositWithdrawTransactionAsync(TransactionDto transaction);
    Task<TransferGuidsResponse> AddTransferTransactionAsync(TransactionDto transferWithdraw, TransactionDto transferDeposit);
    Task<List<TransactionDto>> GetTransactionByIdAsync(Guid id);
    Task<List<TransactionDto>> GetTransactionsByAccountIdAsync(Guid id);
    void SetNewRates(List<CurrencyRateDto> rates);
    Task<List<CurrencyRateDto>> GetRatesAsync();
}