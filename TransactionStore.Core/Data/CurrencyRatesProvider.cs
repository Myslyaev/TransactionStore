using Messaging.Shared;
using Serilog;

namespace TransactionStore.Core.Data;

public class CurrencyRatesProvider : ICurrencyRatesProvider
{
    private static readonly object _lockObject = new();
    private static Dictionary<string, decimal> _rates = [];
    private readonly ILogger _logger = Log.ForContext<CurrencyRatesProvider>();

    private static string ConvertCurrencyEnumToString(Enum currencyNumber)
    {
        return currencyNumber.ToString().ToUpper();
    }

    public decimal ConvertFirstCurrencyToUsd(Enum currencyNumber)
    {
        var currency = ConvertCurrencyEnumToString(currencyNumber);
        lock (_lockObject)
        {
            if (_rates.TryGetValue(currency, out var rateToUsd))
            {
                _logger.Information($"���������� ���� {currency} � USD - {rateToUsd}.");
                return rateToUsd;
            }
        }

        _logger.Error($"���� ��� {currency} � USD �� ������.");
        throw new ArgumentException($"���� ��� {currency} � USD �� ������.");
    }

    public decimal ConvertUsdToSecondCurrency(Enum currencyNumber)
    {
        var currency = ConvertCurrencyEnumToString(currencyNumber);
        lock (_lockObject)
        {
            if (_rates.TryGetValue(currency, out var rateToUsd))
            {
                _logger.Information($"���������� ���� USD � {currency} - 1/{rateToUsd}.");
                return 1 / rateToUsd;
            }
        }

        _logger.Error($"���� ��� USD � {currency} �� ������.");
        throw new ArgumentException($"���� ��� USD � {currency} �� ������.");
    }

    public void SetRates(RatesInfo rates)
    {
        lock (_lockObject)
        {
            _logger.Information($"���������� ������ ����� �� {DateTime.Now}.");
            _rates = rates.Rates;
        }
    }
}