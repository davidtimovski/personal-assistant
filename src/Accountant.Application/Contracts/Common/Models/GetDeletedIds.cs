namespace Accountant.Application.Contracts.Common.Models;

public class GetDeletedIds
{
    public GetDeletedIds(int userId, DateTime fromDate)
    {
        UserId = userId;
        FromDate = fromDate;
    }

    public int UserId { get; set; }
    public DateTime FromDate { get; set; }
}