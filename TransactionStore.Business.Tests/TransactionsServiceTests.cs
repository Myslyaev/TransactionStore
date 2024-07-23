using AutoMapper;
using Backend.Core.Validators;
using FluentAssertions;
using FluentValidation;
using Moq;
using TransactionStore.Business.Services;
using TransactionStore.Core.Data;
using TransactionStore.Core.DTOs;
using TransactionStore.Core.Enums;
using TransactionStore.Core.Exceptions;
using TransactionStore.Core.Models;
using TransactionStore.Core.Models.Requests;
using TransactionStore.Core.Models.Responses;
using TransactionStore.DataLayer.Repositories;

namespace TransactionStore.Business.Tests;

public class TransactionsServiceTests
{
    private readonly Mock<ITransactionsRepository> _repositoryMock = new();
    private readonly Mock<ICurrencyRatesProvider> _currencyRatesProviderMock = new();
    private readonly Mock<ICommissionsProvider> _commissionsProviderMock = new();
    private readonly Mock<IMessagesService> _messagesServiceMock = new();
    private readonly IValidator<DepositWithdrawRequest> _depositWithdrawValidator;
    private readonly IValidator<TransferRequest> _transferValidator;
    private readonly IMapper _mapper;
    private readonly TransactionsService _service;

    public TransactionsServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<TransactionsMappingProfile>());
        _mapper = config.CreateMapper();

        _depositWithdrawValidator = new AddDepositWithdrawValidator();
        _transferValidator = new AddTransferValidator();

        _service = new TransactionsService(
            _repositoryMock.Object,
            _currencyRatesProviderMock.Object,
            _commissionsProviderMock.Object,
            _mapper,
            _messagesServiceMock.Object,
            _depositWithdrawValidator,
            _transferValidator
        );
    }

    private static Guid GenerateUniqueId() => Guid.NewGuid();

    [Fact]
    public async Task AddDepositTransaction_ValidRequest_ReturnsCorrectTransactionId()
    {
        // Arrange
        var accountId = GenerateUniqueId();
        var depositRequest = TransactionsServiceTestData.GetValidDepositWithdrawRequest(accountId);
        var depositId = GenerateUniqueId();

        var commissionPercent = 5m;
        _commissionsProviderMock.Setup(cp => cp.GetPercentForTransaction(TransactionType.Deposit)).Returns(commissionPercent);

        _repositoryMock.Setup(r => r.AddDepositWithdrawTransactionAsync(It.IsAny<TransactionDto>())).ReturnsAsync(depositId);

        // Act
        var depositResult = await _service.AddDepositWithdrawTransactionAsync(TransactionType.Deposit, depositRequest);

        // Assert
        depositResult.Should().Be(depositId);

        var depositCommission = depositRequest.Amount * commissionPercent / 100;
        var expectedDepositAmount = depositRequest.Amount - depositCommission;

        _repositoryMock.Verify(r => r.AddDepositWithdrawTransactionAsync(It.Is<TransactionDto>(t =>
            t.TransactionType == TransactionType.Deposit && t.Amount == expectedDepositAmount)), Times.Once);
    }

    [Fact]
    public async Task AddWithdrawTransaction_ValidRequest_ReturnsCorrectTransactionId()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var withdrawRequest = TransactionsServiceTestData.GetValidDepositWithdrawRequest(accountId);
        var withdrawId = Guid.NewGuid();

        var commissionPercent = 10m;
        _commissionsProviderMock.Setup(cp => cp.GetPercentForTransaction(TransactionType.Withdraw)).Returns(commissionPercent);

        _repositoryMock.Setup(r => r.AddDepositWithdrawTransactionAsync(It.IsAny<TransactionDto>())).ReturnsAsync(withdrawId);

        // Act
        var withdrawResult = await _service.AddDepositWithdrawTransactionAsync(TransactionType.Withdraw, withdrawRequest);

        // Assert
        withdrawResult.Should().Be(withdrawId);

        var withdrawCommission = withdrawRequest.Amount * commissionPercent / 100;
        var expectedWithdrawAmount = -(withdrawRequest.Amount - withdrawCommission);

        _repositoryMock.Verify(r => r.AddDepositWithdrawTransactionAsync(It.Is<TransactionDto>(t =>
            t.TransactionType == TransactionType.Withdraw && t.Amount == expectedWithdrawAmount)), Times.Once);
    }

    [Fact]
    public async Task AddDepositTransaction_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var invalidRequest = TransactionsServiceTestData.GetInvalidDepositWithdrawRequest();

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            _service.AddDepositWithdrawTransactionAsync(TransactionType.Deposit, invalidRequest));
    }

    [Fact]
    public async Task AddWithdrawTransaction_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var invalidRequest = TransactionsServiceTestData.GetInvalidDepositWithdrawRequest();

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            _service.AddDepositWithdrawTransactionAsync(TransactionType.Withdraw, invalidRequest));
    }

    [Fact]
    public async Task AddTransferTransaction_ValidRequest_ReturnsResponse()
    {
        // Arrange
        var transferRequest = TransactionsServiceTestData.GetValidTransferRequest();
        var commissionPercent = 1.5m;
        var commissionAmount = transferRequest.Amount * commissionPercent / 100;

        _repositoryMock.Setup(r => r.AddTransferTransactionAsync(It.IsAny<TransactionDto>(), It.IsAny<TransactionDto>()))
                       .ReturnsAsync(new TransferGuidsResponse());

        _repositoryMock.Setup(r => r.GetRatesAsync())
                       .ReturnsAsync(new List<CurrencyRateDto> { new() { Currency = Currency.USD, Rate = 1.0m } });

        _commissionsProviderMock.Setup(cp => cp.GetPercentForTransaction(TransactionType.Transfer))
                                .Returns(commissionPercent);

        _currencyRatesProviderMock.Setup(crp => crp.ConvertFirstCurrencyToUsd(It.IsAny<Enum>()))
                                  .Returns(1.0m);

        _currencyRatesProviderMock.Setup(crp => crp.ConvertUsdToSecondCurrency(It.IsAny<Currency>()))
                                  .Returns(75.0m); // Assuming conversion rate to RUB

        // Act
        var response = await _service.AddTransferTransactionAsync(transferRequest);

        // Assert
        response.Should().NotBeNull();
        _repositoryMock.Verify(r => r.AddTransferTransactionAsync(It.IsAny<TransactionDto>(), It.IsAny<TransactionDto>()), Times.Once);
    }

    //[Fact]
    //public async Task AddTransferTransaction_InvalidRequest_ThrowsValidationException()
    //{
    //    // Arrange
    //    var invalidRequest = TransactionsServiceTestData.GetInvalidTransferRequest();

    //    var validationFailures = new List<ValidationFailure>
    //    {
    //        new ValidationFailure("PropertyName", "Validation error message")
    //    };

    //    var validationResult = new ValidationResult(validationFailures);
    //    _transferValidator.Setup(v => v.ValidateAsync(invalidRequest, It.IsAny<CancellationToken>()))
    //                             .ReturnsAsync(validationResult);

    //    // Act & Assert
    //    await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
    //        _service.AddTransferTransactionAsync(invalidRequest));
    //}

    [Fact]
    public void CreateWithdrawTransaction_ValidRequest_TransactionCreated()
    {
        // Arrange
        var transferRequest = TransactionsServiceTestData.GetValidTransferRequest();
        var commissionPercent = 1.5m;
        var commissionAmount = transferRequest.Amount * commissionPercent / 100;

        _commissionsProviderMock.Setup(x => x.GetPercentForTransaction(TransactionType.Transfer)).Returns(commissionPercent);

        var expectedWithdrawTransaction = TransactionsServiceTestData.CreateExpectedWithdrawTransaction(transferRequest, commissionAmount);

        // Act
        var withdrawTransaction = _service.CreateWithdrawTransaction(transferRequest, commissionAmount);

        // Assert
        withdrawTransaction.Item1.Should().BeEquivalentTo(expectedWithdrawTransaction);
    }

    [Fact]
    public void CreateDepositTransaction_ValidRequest_TransactionCreated()
    {
        // Arrange
        var transferRequest = TransactionsServiceTestData.GetValidTransferRequest();
        var commissionPercent = 1.5m;
        var commissionAmount = transferRequest.Amount * commissionPercent / 100;
        var withdrawAmount = transferRequest.Amount - commissionAmount;
        var (rateToUSD, rateFromUsd) = TransactionsServiceTestData.GetCurrencyRates();

        _currencyRatesProviderMock.Setup(x => x.ConvertFirstCurrencyToUsd(transferRequest.CurrencyFrom)).Returns(rateToUSD);
        _currencyRatesProviderMock.Setup(x => x.ConvertUsdToSecondCurrency(transferRequest.CurrencyTo)).Returns(rateFromUsd);

        var expectedDepositTransaction = TransactionsServiceTestData.CreateExpectedDepositTransaction(transferRequest, withdrawAmount, rateToUSD, rateFromUsd);

        // Act
        var depositTransaction = _service.CreateDepositTransaction(transferRequest, withdrawAmount);

        // Assert
        depositTransaction.Should().BeEquivalentTo(expectedDepositTransaction);
    }

    [Fact]
    public async Task GetTransactionByIdAsync_ValidId_ReturnsTransactions()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var transactions = TransactionsServiceTestData.GetTransactions(transactionId);

        _repositoryMock.Setup(r => r.GetTransactionByIdAsync(transactionId)).ReturnsAsync(transactions);

        // Act
        var result = await _service.GetTransactionByIdAsync(transactionId);

        // Assert
        result.Should().BeOfType<FullTransactionResponse>();
        _repositoryMock.Verify(r => r.GetTransactionByIdAsync(transactionId), Times.Once);
    }

    [Fact]
    public async Task GetTransactionByIdAsync_InvalidId_ReturnsEmptyResponse()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var transactions = TransactionsServiceTestData.GetEmptyTransactions();

        _repositoryMock.Setup(r => r.GetTransactionByIdAsync(transactionId)).ReturnsAsync(transactions);

        // Act
        var result = await _service.GetTransactionByIdAsync(transactionId);

        // Assert
        result.Should().BeNull();
        _repositoryMock.Verify(r => r.GetTransactionByIdAsync(transactionId), Times.Once);
    }

    [Fact]
    public async Task GetTransactionByIdAsync_RepositoryThrowsServiceUnavailableException_ThrowsServiceUnavailableException()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetTransactionByIdAsync(transactionId)).ThrowsAsync(new ServiceUnavailableException("There is no connection to the database."));

        // Act
        Func<Task> act = async () => await _service.GetTransactionByIdAsync(transactionId);

        // Assert
        await act.Should().ThrowAsync<ServiceUnavailableException>();
        _repositoryMock.Verify(r => r.GetTransactionByIdAsync(transactionId), Times.Once);
    }

    [Fact]
    public async Task GetTransactionsByAccountIdAsync_ValidId_ReturnsTransactions()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transactions = TransactionsServiceTestData.GetTransactionsByAccountId(accountId);

        _repositoryMock.Setup(r => r.GetTransactionsByAccountIdAsync(accountId)).ReturnsAsync(transactions);

        // Act
        var result = await _service.GetTransactionsByAccountIdAsync(accountId);

        // Assert
        result.Should().BeEquivalentTo(_mapper.Map<List<TransactionResponse>>(transactions));
        _repositoryMock.Verify(r => r.GetTransactionsByAccountIdAsync(accountId), Times.Once);
    }

    [Fact]
    public async Task GetTransactionsByAccountIdAsync_InvalidId_ReturnsEmptyList()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transactions = TransactionsServiceTestData.GetEmptyTransactions();

        _repositoryMock.Setup(r => r.GetTransactionsByAccountIdAsync(accountId)).ReturnsAsync(transactions);

        // Act
        var result = await _service.GetTransactionsByAccountIdAsync(accountId);

        // Assert
        result.Should().BeEmpty();
        _repositoryMock.Verify(r => r.GetTransactionsByAccountIdAsync(accountId), Times.Once);
    }

    [Fact]
    public async Task GetTransactionsByAccountIdAsync_RepositoryThrowsServiceUnavailableException_ThrowsServiceUnavailableException()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetTransactionsByAccountIdAsync(accountId)).ThrowsAsync(new ServiceUnavailableException("There is no connection to the database."));

        // Act
        Func<Task> act = async () => await _service.GetTransactionsByAccountIdAsync(accountId);

        // Assert
        await act.Should().ThrowAsync<ServiceUnavailableException>();
        _repositoryMock.Verify(r => r.GetTransactionsByAccountIdAsync(accountId), Times.Once);
    }

    [Fact]
    public async Task GetBalanceByAccountIdAsync_ValidId_ReturnsBalanceResponse()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var transactions = TransactionsServiceTestData.GetTransactionsByAccountId(accountId);
        var expectedBalance = transactions.Sum(t => t.Amount);

        _repositoryMock.Setup(r => r.GetTransactionsByAccountIdAsync(accountId)).ReturnsAsync(transactions);

        // Act
        var result = await _service.GetBalanceByAccountIdAsync(accountId);

        // Assert
        result.Should().BeOfType<AccountBalanceResponse>();
        result.AccountId.Should().Be(accountId);
        result.Balance.Should().Be(expectedBalance);
    }

    [Fact]
    public async Task GetBalanceByAccountIdAsync_RepositoryThrowsException_ThrowsServiceUnavailableException()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetTransactionsByAccountIdAsync(accountId)).ThrowsAsync(new ServiceUnavailableException("There is no connection to the database."));

        // Act
        Func<Task> act = async () => await _service.GetBalanceByAccountIdAsync(accountId);

        // Assert
        await act.Should().ThrowAsync<ServiceUnavailableException>();
    }
}