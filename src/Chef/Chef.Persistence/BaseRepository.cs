using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Chef.Persistence;

public abstract class BaseRepository
{
    private readonly string _connectionString;

    protected BaseRepository(ChefContext efContext)
    {
        _connectionString = efContext.Database.GetConnectionString() ?? throw new ArgumentNullException("Connection string missing");
        EFContext = efContext;
    }

    protected IDbConnection OpenConnection()
    {
        var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        return conn;
    }

    protected readonly ChefContext EFContext;
}
