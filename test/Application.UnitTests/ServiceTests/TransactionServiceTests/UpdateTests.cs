using Accountant.Application.Contracts.Transactions;
using Accountant.Application.Contracts.Transactions.Models;
using Accountant.Application.Mappings;
using Accountant.Application.Services;
using Application.Domain.Accountant;
using Application.UnitTests.Builders;
using FluentValidation;
using Moq;
using Xunit;

namespace Application.UnitTests.ServiceTests.TransactionServiceTests;

public class UpdateTests
{
    private readonly Mock<IValidator<UpdateTransaction>> _successfulValidatorMock;
    private readonly Mock<ITransactionsRepository> _transactionsRepositoryMock = new();
    private readonly ITransactionService _sut;

    public UpdateTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateTransaction>();

        _sut = new TransactionService(
            _transactionsRepositoryMock.Object,
            MapperMocker.GetMapper<AccountantProfile>(),
            null);
    }

    [Fact]
    public async Task TrimsDescription_IfPresent()
    {
        string actualDescription = null;
        _transactionsRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Transaction>()))
            .Callback<Transaction>(t => actualDescription = t.Description);

        var model = new TransactionBuilder().WithAccounts(1, 2).WithDescription(" Description ").BuildUpdateModel();

        await _sut.UpdateAsync(model);
        const string expected = "Description";

        Assert.Equal(expected, actualDescription);
    }
}
