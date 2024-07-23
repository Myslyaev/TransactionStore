using FluentAssertions;
using TransactionStore.Core.Data;

namespace TransactionStore.Core.Tests;

public enum TestCurrency
{
    USD,
    RUB,
    EUR,
    JPY,
    CNY,
    RSD,
    BGN,
    ARS,
    INVALID // для проверки исключений
}

public class CurrencyRatesProviderTests
{
    private readonly CurrencyRatesProvider _currencyRatesProvider;

    public CurrencyRatesProviderTests()
    {
        _currencyRatesProvider = new CurrencyRatesProvider();
    }    

    [Fact]
    public void ConvertFirstCurrencyToUsd_InvalidCurrency_ThrowsArgumentException()
    {
        // Act
        Action act = () => _currencyRatesProvider.ConvertFirstCurrencyToUsd(TestCurrency.INVALID);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConvertUsdToSecondCurrency_InvalidCurrency_ThrowsArgumentException()
    {
        // Act
        Action act = () => _currencyRatesProvider.ConvertUsdToSecondCurrency(TestCurrency.INVALID);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}