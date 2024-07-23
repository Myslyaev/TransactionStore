using Microsoft.AspNetCore.Mvc;
using Serilog;
using TransactionStore.Business.Services;
using TransactionStore.Core.Enums;
using TransactionStore.Core.Models.Requests;
using TransactionStore.Core.Models.Responses;

namespace TransactionStore.API.Controllers;

[ApiController]
[Route("/api/transactions")]
public class TransactionsController(ITransactionsService transactionsService) : Controller
{
    private readonly Serilog.ILogger _logger = Log.ForContext<TransactionsController>();

    [HttpPost("deposit")]
    public async Task<ActionResult<Guid>> AddDepositTransaction([FromBody] DepositWithdrawRequest request)
    {
        _logger.Information($"A deposit transaction has been added for the account with Id {request.AccountId}.");
        var transactionId = await transactionsService.AddDepositWithdrawTransactionAsync(TransactionType.Deposit, request);
        return Created($"/api/transactions/{transactionId}", transactionId);
    }

    [HttpPost("withdraw")]
    public async Task<ActionResult<Guid>> AddWithdrawTransaction([FromBody] DepositWithdrawRequest request)
    {
        _logger.Information($"A withdraw transaction has been added for the account with Id {request.AccountId}.");
        var transactionId = await transactionsService.AddDepositWithdrawTransactionAsync(TransactionType.Withdraw, request);
        return Created($"/api/transactions/{transactionId}", transactionId);
    }

    [HttpPost("transfer")]
    public async Task<ActionResult<TransferGuidsResponse>> AddTransferTransaction([FromBody] TransferRequest request)
    {
        _logger.Information($"A transfer transaction from an account with Id {request.AccountFromId} to an account with Id {request.AccountToId} has been added into the database.");
        var response = await transactionsService.AddTransferTransactionAsync(request);
        return Created($"/api/transactions/{response}", response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FullTransactionResponse>> GetTransactionById(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.Warning($"Transaction ID is empty.");
            return NotFound("Transaction ID is empty.");
        }

        _logger.Information($"Getting transaction by Id {id}.");
        var transaction = await transactionsService.GetTransactionByIdAsync(id);

        if (transaction == null)
        {
            _logger.Warning($"Transaction with Id {id} not found.");
            return NotFound($"Transaction with Id {id} not found.");
        }

        return Ok(transaction);
    }
}