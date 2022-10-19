using System;
using System.Threading.Tasks;
using Application.Contracts.Accountant.Accounts;
using Application.Contracts.Accountant.Transactions;
using Application.Contracts.Accountant.Transactions.Models;
using Application.Mappings;
using Application.Services.Accountant;
using Application.UnitTests.Builders;
using Domain.Entities.Accountant;
using FluentValidation;
using Moq;
using Xunit;

namespace Application.UnitTests.ServiceTests.TransactionServiceTests;

public class CreateTests
{
    private readonly Mock<IValidator<CreateTransaction>> _successfulValidatorMock;
    private readonly Mock<ITransactionsRepository> _transactionsRepositoryMock = new();
    private readonly Mock<IAccountsRepository> _accountsRepositoryMock = new();
    private readonly ITransactionService _sut;

    public CreateTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<CreateTransaction>();

        _sut = new TransactionService(
            _transactionsRepositoryMock.Object,
            _accountsRepositoryMock.Object,
            MapperMocker.GetMapper<AccountantProfile>(),
            null);
    }

    [Fact]
    public async Task Throws_IfAccountMissing()
    {
        var model = new CreateTransaction();
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(model));
    }

    [Fact]
    public async Task Throws_IfFromAndToAccountsEqual()
    {
        var model = new TransactionBuilder().WithAccounts(2, 2).BuildCreateModel();
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(model));
    }

    [Fact]
    public async Task Throws_IfFromAccountDoesNotBelongToUser()
    {
        var model = new CreateTransaction { UserId = 1, FromAccountId = 2 };

        _accountsRepositoryMock.Setup(x => x.Exists(model.FromAccountId.Value, model.UserId))
            .Returns(false);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(model));
    }

    [Fact]
    public async Task Throws_IfToAccountDoesNotBelongToUser()
    {
        CreateTransaction model = new CreateTransaction { UserId = 1, ToAccountId = 2 };

        _accountsRepositoryMock.Setup(x => x.Exists(model.ToAccountId.Value, model.UserId))
            .Returns(false);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(model));
    }

    [Fact]
    public async Task TrimsDescription_IfPresent()
    {
        _accountsRepositoryMock.Setup(x => x.Exists(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(true);

        string actualDescription = null;
        _transactionsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Transaction>()))
            .Callback<Transaction>(t => actualDescription = t.Description);

        var model = new TransactionBuilder().WithAccounts(1, 2).WithDescription(" Description ").BuildCreateModel();

        await _sut.CreateAsync(model);
        const string expected = "Description";

        Assert.Equal(expected, actualDescription);
    }
}
