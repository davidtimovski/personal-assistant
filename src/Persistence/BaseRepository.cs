﻿using System.Data;
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

        protected IDbConnection OpenConnection()
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            return conn;
        }

        protected readonly PersonalAssistantContext EFContext;
    }
}
