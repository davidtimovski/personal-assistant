namespace Accountant.Application.Contracts.Common.Models;

public class GetAll
{
    public GetAll(int userId, DateTime fromModifiedDate)
    {
        UserId = userId;
        FromModifiedDate = fromModifiedDate;
    }

    public int UserId { get; set; }
    public DateTime FromModifiedDate { get; set; }
}
