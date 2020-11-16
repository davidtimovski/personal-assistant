using System;

namespace PersonalAssistant.Application.Contracts.Accountant.Common.Models
{
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
}
