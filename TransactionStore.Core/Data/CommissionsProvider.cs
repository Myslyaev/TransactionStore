using Serilog;
using System.Globalization;
using TransactionStore.Core.Enums;
using TransactionStore.Core.Settings;

namespace TransactionStore.Core.Data;

public class CommissionsProvider : ICommissionsProvider
{
    private readonly Dictionary<string, decimal> _percent;
    private readonly ILogger _logger = Log.ForContext<CommissionsProvider>();

    public CommissionsProvider(ComissionSettings comission)
    {
        _percent = new Dictionary<string, decimal>()
        {
            { TransactionType.Deposit.ToString().ToUpper(), decimal.Parse(comission.Deposit, new NumberFormatInfo() { NumberDecimalSeparator = "," }) },
            { TransactionType.Withdraw.ToString().ToUpper(), decimal.Parse(comission.Withdraw, new NumberFormatInfo() { NumberDecimalSeparator = "," }) },
            { TransactionType.Transfer.ToString().ToUpper(), decimal.Parse(comission.Transfer, new NumberFormatInfo() { NumberDecimalSeparator = "," }) }
        };
    }

    private static string ConvertTransactionEnumToString(Enum transactionType)
    {
        return transactionType.ToString().ToUpper();
    }

    public decimal GetPercentForTransaction(Enum transactionType)
    {
        var transaction = ConvertTransactionEnumToString(transactionType);
        if (_percent.TryGetValue(transaction, out var percent))
        {
            _logger.Information($"Returning percent of commission for {transactionType} - {percent}.");
            return percent;
        }
        _logger.Error($"Throwing an error if percent of commission for {transaction} not found.");
        throw new ArgumentException($"The percent of commission for {transaction} not found.");
    }
}