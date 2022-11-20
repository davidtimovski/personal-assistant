using Accountant.Application.Contracts.Transactions.Models;

namespace Application.UnitTests.Builders;

public class TransactionBuilder
{
    private int? fromAccountId;
    private int? toAccountId;
    private string description;

    public TransactionBuilder WithAccounts(int newFromAccountId, int newToAccountId)
    {
        fromAccountId = newFromAccountId;
        toAccountId = newToAccountId;
        return this;
    }

    public TransactionBuilder WithDescription(string newDescription)
    {
        description = newDescription;
        return this;
    }

    public CreateTransaction BuildCreateModel()
    {
        return new CreateTransaction
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Description = description
        };
    }

    public UpdateTransaction BuildUpdateModel()
    {
        return new UpdateTransaction
        {
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Description = description
        };
    }
}
