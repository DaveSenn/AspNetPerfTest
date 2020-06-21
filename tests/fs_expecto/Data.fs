module Data
open System
open Dapper
open Npgsql
open TodoList.Models
// Data test fixtures & utils

let GetConnection = 
    let _connectionString = String.Format(
                                "Host={0};Port=5432;Username={1};Password={2};Database=todolist;",
                                Environment.GetEnvironmentVariable "DEV_PG_HOST",
                                Environment.GetEnvironmentVariable "DEV_PG_USER",
                                Environment.GetEnvironmentVariable "DEV_PG_PASSWORD")
    new NpgsqlConnection(_connectionString)

let AddTask text priority =
    let connection = GetConnection
    try
        connection.Open()
        let sql = @"INSERT INTO tasks (text, priority) VALUES (@Text, @Priority) RETURNING id"
        let task = TodoList.Models.TodoTask(0, text, priority)
        let result = connection.ExecuteScalar<int>(sql, task)
        result
    finally
        connection.Close()

let RemoveTask id =
    let connection = GetConnection
    try
        connection.Open()
        let sql = @"DELETE FROM tasks WHERE id = @Id"
        let task = TodoList.Models.TodoTask(id, "", 0)
        let result = connection.Execute(sql, task)
        result
    finally
        connection.Close()