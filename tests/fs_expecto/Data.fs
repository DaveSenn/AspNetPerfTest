module Data
open Dapper
open Npgsql
open TodoList.Models
// Data test fixtures & utils

let GetConnection = 
    new NpgsqlConnection("Host=localhost;Port=5432;Username=tdl_user;Password=secretz;Database=todolist;")

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