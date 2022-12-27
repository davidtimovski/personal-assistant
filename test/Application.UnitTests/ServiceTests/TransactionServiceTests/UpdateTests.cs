using Accountant.Application.Contracts.Accounts;
using Accountant.Application.Contracts.Transactions;
using Accountant.Application.Contracts.Transactions.Models;
using Accountant.Application.Mappings;
using Accountant.Application.Services;
using Application.UnitTests.Builders;
using Application.Domain.Accountant;
using FluentValidation;
using Moq;
using Xunit;

namespace Application.UnitTests.ServiceTests.TransactionServiceTests;

public class UpdateTests
{
    private readonly Mock<IValidator<UpdateTransaction>> _successfulValidatorMock;
    private readonly Mock<ITransactionsRepository> _transactionsRepositoryMock = new();
    private readonly Mock<IAccountsRepository> _accountsRepositoryMock = new();
    private readonly ITransactionService _sut;

    public UpdateTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateTransaction>();

        _sut = new TransactionService(
            _transactionsRepositoryMock.Object,
            _accountsRepositoryMock.Object,
            MapperMocker.GetMapper<AccountantProfile>(),
            null);
    }

    [Fact]
    public async Task Throws_IfAccountMissing()
    {
        var model = new UpdateTransaction();
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAsync(model));
    }

    [Fact]
    public async Task Throws_IfFromAndToAccountsEqual()
    {
        var model = new TransactionBuilder().WithAccounts(2, 2).BuildUpdateModel();
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAsync(model));
    }

    [Fact]
    public async Task Throws_IfFromAccountDoesNotBelongToUser()
    {
        var model = new UpdateTransaction { UserId = 1, FromAccountId = 2 };

        _accountsRepositoryMock.Setup(x => x.Exists(model.FromAccountId.Value, model.UserId))
            .Returns(false);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAsync(model));
    }

    [Fact]
    public async Task Throws_IfToAccountDoesNotBelongToUser()
    {
        UpdateTransaction model = new UpdateTransaction { UserId = 1, ToAccountId = 2 };

        _accountsRepositoryMock.Setup(x => x.Exists(model.ToAccountId.Value, model.UserId))
            .Returns(false);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAsync(model));
    }

    [Fact]
    public async Task TrimsDescription_IfPresent()
    {
        _accountsRepositoryMock.Setup(x => x.Exists(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(true);

        string actualDescription = null;
        _transactionsRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Transaction>()))
            .Callback<Transaction>(t => actualDescription = t.Description);

        var model = new TransactionBuilder().WithAccounts(1, 2).WithDescription(" Description ").BuildUpdateModel();

        await _sut.UpdateAsync(model);
        const string expected = "Description";

        Assert.Equal(expected, actualDescription);
    }
}
