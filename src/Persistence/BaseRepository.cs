using System.Data.Common;
using Npgsql;
using Persistence;

namespace PersonalAssistant.Persistence
{
    public abstract class BaseRepository
    {
        private readonly string _connectionString;

        public BaseRepository(string connectionString, PersonalAssistantContext efContext)
        {
            _connectionString = connectionString;
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
