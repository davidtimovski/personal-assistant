using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Persistence;

namespace PersonalAssistant.Persistence
{
    public abstract class BaseRepository
    {
        private readonly string _connectionString;

        public BaseRepository(PersonalAssistantContext efContext)
        {
            _connectionString = efContext.Database.GetConnectionString();
            EFContext = efContext;
        }

        protected DbConnection Connection
        {
            get
            {
                return new NpgsqlConnection(_connectionString);
            }
        }

        protected readonly PersonalAssistantContext EFContext;
    }
}
