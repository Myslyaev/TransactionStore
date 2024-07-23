using FluentAssertions;
using TransactionStore.Core.Data;
using TransactionStore.Core.Enums;
using TransactionStore.Core.Settings;

namespace TransactionStore.Core.Tests
{
    public class CommissionsProviderTests
    {
        private readonly ComissionSettings _comissionSettings = new("5,0", "10,0", "7,5");

        private readonly CommissionsProvider _commissionsProvider;

        public CommissionsProviderTests()
        {
            _commissionsProvider = new CommissionsProvider(_comissionSettings);
        }

        [Theory]
        [InlineData(TransactionType.Deposit, 5)]
        [InlineData(TransactionType.Withdraw, 10)]
        [InlineData(TransactionType.Transfer, 7.5)]
        public void GetPercentForTransaction_ExistingTransactionType_ReturnsPercent(TransactionType transactionType, decimal expectedPercent)
        {
            // Act
            var percent = _commissionsProvider.GetPercentForTransaction(transactionType);

            // Assert
            percent.Should().Be(expectedPercent);
        }

        [Fact]
        public void GetPercentForTransaction_NonExistingTransactionType_ThrowsArgumentException()
        {
            // Arrange
            Enum nonExistingType = TransactionType.Unknown; // Use Unknown from TransactionType enum

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _commissionsProvider.GetPercentForTransaction(nonExistingType));
            Assert.Contains("The percent of commission for UNKNOWN not found.", exception.Message, StringComparison.InvariantCultureIgnoreCase);
        }

    }
}
