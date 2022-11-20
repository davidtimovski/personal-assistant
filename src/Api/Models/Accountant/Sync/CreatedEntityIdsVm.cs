namespace Api.Models.Accountant.Sync;

public class CreatedEntityIdsVm
{
    public List<CreatedEntityIdPair> AccountIdPairs { get; set; } = new List<CreatedEntityIdPair>();
    public List<CreatedEntityIdPair> CategoryIdPairs { get; set; } = new List<CreatedEntityIdPair>();
    public List<CreatedEntityIdPair> TransactionIdPairs { get; set; } = new List<CreatedEntityIdPair>();
    public List<CreatedEntityIdPair> UpcomingExpenseIdPairs { get; set; } = new List<CreatedEntityIdPair>();
    public List<CreatedEntityIdPair> DebtIdPairs { get; set; } = new List<CreatedEntityIdPair>();
    public List<CreatedEntityIdPair> AutomaticTransactionIdPairs { get; set; } = new List<CreatedEntityIdPair>();
}

public class CreatedEntityIdPair
{
    public CreatedEntityIdPair(int localId, int id)
    {
        LocalId = localId;
        Id = id;
    }

    public int LocalId { get; set; }
    public int Id { get; set; }
}
