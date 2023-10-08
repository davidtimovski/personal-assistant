using System.Data;
using Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Weatherman.Persistence;

public abstract class BaseRepository
{
    private readonly string _connectionString;

    protected BaseRepository(WeathermanContext efContext)
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

    protected readonly WeathermanContext EFContext;
}
