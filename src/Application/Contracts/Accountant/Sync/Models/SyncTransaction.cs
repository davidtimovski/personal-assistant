using System;

namespace Application.Contracts.Accountant.Sync.Models
{
    public class SyncTransaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? FromAccountId { get; set; }
        public int? ToAccountId { get; set; }
        public int? CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public bool IsEncrypted { get; set; }
        public byte[] EncryptedDescription { get; set; }
        public byte[] Salt { get; set; }
        public byte[] Nonce { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
