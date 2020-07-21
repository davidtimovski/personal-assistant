using System.Data.Common;
using Npgsql;

namespace PersonalAssistant.Persistence
{
    public abstract class BaseRepository
    {
        private readonly string _connectionString;

        public BaseRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        internal DbConnection Connection
        {
            get
            {
                return new NpgsqlConnection(_connectionString);
            }
        }
    }
}
