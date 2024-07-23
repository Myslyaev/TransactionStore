using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionStore.Core.DTOs;
using TransactionStore.Core.Exceptions;
using TransactionStore.DataLayer.Repositories;
using Xunit;

namespace TransactionStore.DataLayer.Tests
{
    public class TransactionsRepositoryTests
    {
        private static DbContextOptions<TransactionStoreContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<TransactionStoreContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private static async Task<(TransactionStoreContext context, TransactionsRepository repository)> InitializeContextAndRepositoryAsync()
        {
            var options = CreateNewContextOptions();
            var context = new TransactionStoreContext(options);
            var repository = new TransactionsRepository(context, options); // Pass both context and options to the repository
            await context.Database.EnsureCreatedAsync(); // Ensure database is created

            return (context, repository);
        }

        [Fact]
        public async Task AddDepositWithdrawTransaction_ValidTransaction_ReturnsId()
        {
            // Arrange
            var (_, repository) = await InitializeContextAndRepositoryAsync();
            var transaction = new TransactionDto { Id = Guid.NewGuid() };

            // Act
            var result = await repository.AddDepositWithdrawTransactionAsync(transaction);

            // Assert
            result.Should().Be(transaction.Id);
        }

        [Fact]
        public async Task AddDepositWithdrawTransaction_NullTransaction_ThrowsException()
        {
            // Arrange
            var (_, repository) = await InitializeContextAndRepositoryAsync();

            // Act & Assert
            Func<Task> act = async () => await repository.AddDepositWithdrawTransactionAsync(null);
            await act.Should().ThrowAsync<Core.Exceptions.ArgumentNullException>()
                .WithMessage("Transaction cannot be null.");
        }

        [Fact]
        public async Task AddTransferTransaction_ValidTransactions_NoExceptionsThrown()
        {
            // Arrange
            var (context, repository) = await InitializeContextAndRepositoryAsync();
            var transferWithdraw = new TransactionDto { Id = Guid.NewGuid() };
            var transferDeposit = new TransactionDto { Id = Guid.NewGuid() };

            // Act
            await repository.AddTransferTransactionAsync(transferWithdraw, transferDeposit);

            // Assert
            var result = await context.Transactions.ToListAsync();
            result.Count.Should().Be(2);
        }

        [Fact]
        public async Task AddTransferTransaction_NullTransaction_ThrowsException()
        {
            // Arrange
            var (_, repository) = await InitializeContextAndRepositoryAsync();

            // Act & Assert
            Func<Task> actWithdraw = async () => await repository.AddTransferTransactionAsync(null, new TransactionDto { Id = Guid.NewGuid() });
            await actWithdraw.Should().ThrowAsync<Core.Exceptions.ArgumentNullException>()
                .WithMessage("Transfer-withdraw transaction cannot be null.");

            Func<Task> actDeposit = async () => await repository.AddTransferTransactionAsync(new TransactionDto { Id = Guid.NewGuid() }, null);
            await actDeposit.Should().ThrowAsync<Core.Exceptions.ArgumentNullException>()
                .WithMessage("Transfer-deposit transaction cannot be null.");
        }

        [Fact]
        public async Task GetTransactionByIdAsync_ExistingTransaction_ReturnsListWithTransactionDto()
        {
            // Arrange
            var leadId = new Guid("550df504-032e-4ef7-aee2-53cf66e4d0c8");
            var (context, repository) = await InitializeContextAndRepositoryAsync();

            var transaction = new TransactionDto { Id = leadId, AccountId = Guid.NewGuid(), Amount = 100 };
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();

            var expected = new List<TransactionDto> { transaction };

            // Act
            var result = await repository.GetTransactionByIdAsync(leadId);

            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetTransactionByIdAsync_NonExistingTransaction_ThrowsNotFoundException()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            var (_, repository) = await InitializeContextAndRepositoryAsync();

            // Act & Assert
            Func<Task> act = async () => await repository.GetTransactionByIdAsync(nonExistingId);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Transaction with Id {nonExistingId} not found.");
        }

        [Fact]
        public async Task GetTransactionsByAccountId_ReturnsCorrectListOfTransactions()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var (context, repository) = await InitializeContextAndRepositoryAsync();

            var transaction1 = new TransactionDto { AccountId = accountId, Amount = 100 };
            var transaction2 = new TransactionDto { AccountId = accountId, Amount = 200 };
            context.Transactions.AddRange(transaction1, transaction2);
            await context.SaveChangesAsync();

            var expected = new List<TransactionDto> { transaction1, transaction2 };

            // Act
            var result = await repository.GetTransactionsByAccountIdAsync(accountId);

            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetTransactionsByAccountId_NoTransactions_ThrowsNotFoundException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var (context, repository) = await InitializeContextAndRepositoryAsync();

            // Clear existing transactions for the account to simulate no transactions found
            context.Transactions.RemoveRange(await context.Transactions.Where(t => t.AccountId == accountId).ToListAsync());
            await context.SaveChangesAsync();

            // Act & Assert
            Func<Task> act = async () => await repository.GetTransactionsByAccountIdAsync(accountId);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"No transactions found for account with Id {accountId}.");
        }
    }
}