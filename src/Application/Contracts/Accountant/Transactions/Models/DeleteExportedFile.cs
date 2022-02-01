using System;

namespace Application.Contracts.Accountant.Transactions.Models
{
    public class DeleteExportedFile
    {
        public DeleteExportedFile(string directory, Guid fileId)
        {
            Directory = directory;
            FileId = fileId;
        }

        public string Directory { get; set; }
        public Guid FileId { get; set; }
    }
}
