using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using TodoList.Models;

namespace TodoList.Controllers
{
    public class Repo : IRepo
    {
        private readonly String _connectionString;

        public Repo( IConfiguration configuration ) =>
            _connectionString = configuration.GetConnectionString( "DefaultConnection" );

        public async Task Add( TodoTask task )
        {
            await using var connection = new NpgsqlConnection( _connectionString );

            await connection.OpenAsync();
            const String sql = @"INSERT into tasks (text, priority) VALUES (@Text, @Priority)";
            await connection.ExecuteAsync( sql, task );
            await connection.CloseAsync();
        }

        public async Task Delete()
        {
            await using var connection = new NpgsqlConnection( _connectionString );
            await connection.ExecuteAsync( "DELETE FROM tasks;" );
        }

        public async Task<Dictionary<String, Object>> Get( Int32? page )
        {
            const Int32 limit = 10;
            page ??= 1;
            var offset = ( page - 1 ) * limit;

            var tasks = new Dictionary<String, Object>();
            await using var connection = new NpgsqlConnection( _connectionString );
            await connection.OpenAsync();

            var sql = "Select * FROM tasks ORDER BY priority asc OFFSET " + offset + " LIMIT " + limit;
            var results = await connection.QueryAsync<TodoTask>( sql );
            tasks.Add( "tasks", results );
            tasks.Add( "position", offset );
            tasks.Add( "page", page );

            return tasks;
        }

        public async Task Update( TodoTask task )
        {
            await using var connection = new NpgsqlConnection( _connectionString );

            await connection.OpenAsync();
            const String sql = @"UPDATE tasks SET text = '@Text', priority = @Priority WHERE id = @Id";
            await connection.ExecuteAsync( sql, task );
        }
    }
}