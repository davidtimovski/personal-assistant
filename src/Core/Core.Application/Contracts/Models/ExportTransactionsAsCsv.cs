namespace Core.Application.Contracts.Models;

public class ExportTransactionsAsCsv
{
    public ExportTransactionsAsCsv(int userId, string directory, Guid fileId, string uncategorized, string encryped)
    {
        UserId = userId;
        Directory = directory;
        FileId = fileId;
        Uncategorized = uncategorized;
        Encrypted = encryped;
    }

    public int UserId { get; set; }
    public string Directory { get; set; }
    public Guid FileId { get; set; }
    public string Uncategorized { get; set; }
    public string Encrypted { get; set; }
}
