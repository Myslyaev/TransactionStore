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
                _logger.Information($"Returning rate {currency} to USD - {rateToUsd}.");
                return rateToUsd;
            }
        }

        _logger.Error($"Throwing an error if rate for {currency} to USD not found.");
        throw new ArgumentException($"Rate for {currency} to USD not found.");
    }

    public decimal ConvertUsdToSecondCurrency(Enum currencyNumber)
    {
        var currency = ConvertCurrencyEnumToString(currencyNumber);
        lock (_lockObject)
        {
            if (_rates.TryGetValue(currency, out var rateToUsd))
            {
                _logger.Information($"Returning rate USD to {currency} - 1/{rateToUsd}.");
                return 1 / rateToUsd;
            }
        }

        _logger.Error($"Throwing an error if rate for USD to {currency} not found.");
        throw new ArgumentException($"Rate for USD to {currency} not found.");
    }

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task SetRates(RatesInfo rates)
    {
        await _semaphore.WaitAsync();
        try
        {
            // Логгирование обновления курсов
            _logger.Information($"Currency rates updated at {DateTime.Now}.");
        
            // Обновление _rates
            _rates = rates.Rates;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}