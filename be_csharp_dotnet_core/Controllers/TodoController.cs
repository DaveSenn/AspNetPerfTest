using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using TodoList.Models;

namespace TodoList.Controllers
{
    [Route("/tasks")]
    [ApiController]
    public class TodoController : ControllerBase
    {

        private readonly string _connectionString;
        public TodoController(IConfiguration configuration) => _connectionString = String.Format(
            configuration.GetConnectionString("DefaultConnection"),
            Environment.GetEnvironmentVariable("DEV_PG_HOST"),
            Environment.GetEnvironmentVariable("DEV_PG_PORT"),
            Environment.GetEnvironmentVariable("DEV_PG_USER"),
            Environment.GetEnvironmentVariable("DEV_PG_PASSWORD")
        );

        // GET tasks
        [HttpGet]
        public async Task<ActionResult<Dictionary<string, dynamic>>> Get(int? page)
        {
            Dictionary<string, dynamic> tasks = new Dictionary<string, dynamic>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var limit = 10;
                if (page == null) { page = 1; }
                var offset = ((page - 1) * limit);
                await connection.OpenAsync();
                var sql = "Select * FROM tasks ORDER BY priority asc OFFSET " + offset.ToString() + " LIMIT " + limit.ToString();
                IEnumerable<TodoTask> results = await connection.QueryAsync<TodoTask>(sql);
                tasks.Add("tasks", results);
                tasks.Add("position", offset);
                tasks.Add("page", page);
            }
            return tasks;
        }

        // POST tasks
        [HttpPost]
        public async Task<ActionResult<Dictionary<string, TodoTask>>> Post(TodoTask task)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = @"INSERT into tasks (text, priority) VALUES (@Text, @Priority)";
                await connection.ExecuteAsync(sql, task);
            }
            return new Dictionary<string, TodoTask>() {
                { "task", task }
            };
        }

        // PUT tasks
        [HttpPut()]
        public async Task<ActionResult<Dictionary<string, TodoTask>>> Put(TodoTask task)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = @"UPDATE tasks SET text = '@Text', priority = @Priority WHERE id = @Id";
                await connection.ExecuteAsync(sql, task);
            }
            return new Dictionary<string, TodoTask>() {
                { "task", task }
            };
        }

        // DELETE tasks
        [HttpDelete()]
        public void Delete()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Execute("DELETE FROM tasks;");
            }
        }
    }

    [Route("/status")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        public StatusController() {}

        // GET tasks
        [HttpGet]
        public string Get()
        {
            return "ok";
        }
    }
}
