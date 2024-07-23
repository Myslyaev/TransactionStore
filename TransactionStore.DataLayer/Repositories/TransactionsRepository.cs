using Microsoft.EntityFrameworkCore;
using Serilog;
using TransactionStore.Core.DTOs;
using TransactionStore.Core.Exceptions;
using TransactionStore.Core.Models.Responses;

namespace TransactionStore.DataLayer.Repositories;

public class TransactionsRepository : BaseRepository, ITransactionsRepository
{
    private readonly ILogger _logger = Log.ForContext<TransactionsRepository>();
    private readonly DbContextOptions<TransactionStoreContext> _options;

    public TransactionsRepository(TransactionStoreContext context, DbContextOptions<TransactionStoreContext> options) : base(context)
    {
        _options = options;
        if (!_ctx.Database.CanConnect())
        {
            _logger.Error("Throwing an error if there is no connection to the database.");
            throw new ServiceUnavailableException("There is no connection to the database.");
        }
    }

    public async Task<Guid> AddDepositWithdrawTransactionAsync(TransactionDto transaction)
    {
        if (transaction == null)
        {
            _logger.Information("Throwing an error if the transaction is null.");
            throw new Core.Exceptions.ArgumentNullException("Transaction cannot be null.");
        }

        _logger.Information($"Saving the transaction in the database.");
        _ctx.Transactions.Add(transaction);
        await _ctx.SaveChangesAsync();

        _logger.Information($"Returning the Id {transaction.Id} of the added transaction.");
        return transaction.Id;
    }

    public async Task<TransferGuidsResponse> AddTransferTransactionAsync(TransactionDto transferWithdraw,
        TransactionDto transferDeposit)
    {
        if (transferWithdraw == null)
        {
            _logger.Information("Throwing an error if the transfer-withdraw transaction is null.");
            throw new Core.Exceptions.ArgumentNullException("Transfer-withdraw transaction cannot be null.");
        }

        if (transferDeposit == null)
        {
            _logger.Information("Throwing an error if the transfer-deposit is null.");
            throw new Core.Exceptions.ArgumentNullException("Transfer-deposit transaction cannot be null.");
        }

        _logger.Information($"Registering and saving the records of the transfer transaction in the database.");
        await _ctx.Transactions.AddAsync(transferWithdraw);
        await _ctx.Transactions.AddAsync(transferDeposit);
        await _ctx.SaveChangesAsync();

        _logger.Information($"Returning transfer-withdraw Id {transferWithdraw.Id} and transfer-deposit Id {transferDeposit.Id}.");
        return new TransferGuidsResponse
        {
            TransferWithdrawId = transferWithdraw.Id,
            TransferDepositId = transferDeposit.Id
        };
    }

    public async Task<List<TransactionDto>> GetTransactionByIdAsync(Guid id)
    {
        _logger.Information($"Looking for transaction by Id {id} in the database.");
        var transaction = await _ctx.Transactions.AsNoTracking().Where(t => t.Id == id).ToListAsync();

        if (transaction.Count == 0)
        {
            throw new NotFoundException($"Transaction with Id {id} not found.");
        }

        if (transaction[0].TransactionType == Core.Enums.TransactionType.Transfer)
        {
            var transactionDateTime = transaction[0].Date;

            return await _ctx.Transactions.AsNoTracking().Where(t => t.Date == transactionDateTime).ToListAsync();
        }
        else
        {
            return transaction;
        }
    }

    public async Task<List<TransactionDto>> GetTransactionsByAccountIdAsync(Guid id)
    {
        _logger.Information($"Returning information about transactions of the account with Id {id}.");
        var transactions = await _ctx.Transactions.AsNoTracking().Where(t => t.AccountId == id).ToListAsync();

        if (transactions == null || transactions.Count == 0)
        {
            _logger.Information($"Throwing an error if transactions for account with Id {id} not found.");
            throw new NotFoundException($"No transactions found for account with Id {id}.");
        }

        return transactions;
    }

    public async Task<List<CurrencyRateDto>> GetRatesAsync()
    {
        await using var context = new TransactionStoreContext(_options);
        _logger.Information("Getting currency rates.");
        var rates = await context.CurrencyRates.ToListAsync();
        return rates;
    }

    public void SetNewRates(List<CurrencyRateDto> rates)
    {
        _logger.Information("Loading all current currency rates into memory.");
        var oldRates = _ctx.CurrencyRates.ToList();

        _logger.Information("Deleting all current currency rates from the database.");
        _ctx.CurrencyRates.RemoveRange(oldRates);

        _logger.Information("Adding new currency rates to the database.");
        _ctx.CurrencyRates.AddRange(rates);

        _logger.Information("Saving changes to the database.");
        _ctx.SaveChanges();
    }
}