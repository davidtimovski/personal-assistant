using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Persistence;

public abstract class BaseRepository
{
    private readonly string _connectionString;

    protected BaseRepository(PersonalAssistantContext efContext)
    {
        _connectionString = efContext.Database.GetConnectionString();
        EFContext = efContext;
    }

    protected IDbConnection OpenConnection()
    {
        var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        return conn;
    }

    protected readonly PersonalAssistantContext EFContext;
}
