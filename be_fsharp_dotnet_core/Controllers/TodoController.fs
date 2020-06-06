namespace TodoList.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Dapper
open Npgsql
open Todolist.Models

[<ApiController>]
[<Route("/tasks")>]
type TodoController (configuration:IConfiguration) =
    inherit ControllerBase()
    let _connectionString = configuration.GetConnectionString "DefaultConnection"

    [<HttpGet>]
    member __.Get() : Task<TaskList> =
        async {            
            let connection =  new NpgsqlConnection(_connectionString)
            do! Async.AwaitTask (connection.OpenAsync())
            let! results = Async.AwaitTask (connection.QueryAsync<TodoTask>("Select * FROM tasks ORDER BY priority asc"))
            let tasks = {
                Tasks=List.ofSeq results;
                Position=0;
                Length=results.Count()
            }
            do! Async.AwaitTask (connection.CloseAsync())
            return tasks
        } |> Async.StartAsTask

    [<HttpPost>]
    member __.Post(task: TodoTask) : Task<Dictionary<string,obj>> =
        async {
            let connection =  new NpgsqlConnection(_connectionString)
            do! Async.AwaitTask (connection.OpenAsync())
            let sql = @"INSERT into tasks (text, priority) VALUES (@Text, @Priority)";
            Async.AwaitTask (connection.ExecuteAsync(sql, task)) |> Async.RunSynchronously |> ignore
            let task = Dictionary<string, obj>()
            task.Add("task", task)
            do! Async.AwaitTask (connection.CloseAsync())
            return task
        } |> Async.StartAsTask


    [<HttpPut>]
    member __.Put(task: TodoTask) : Task<Dictionary<string,obj>> =
        async {
            let connection =  new NpgsqlConnection(_connectionString)
            do! Async.AwaitTask (connection.OpenAsync())
            let sql = @"UPDATE tasks SET (text = @Text, priority = @Priority) WHERE id = @Id";
            Async.AwaitTask (connection.ExecuteAsync(sql, task)) |> Async.RunSynchronously |> ignore
            let task = Dictionary<string, obj>()
            task.Add("task", task)
            do! Async.AwaitTask (connection.CloseAsync())
            return task
        } |> Async.StartAsTask

    [<HttpDelete>]
    member __.Delete() =
        let connection =  new NpgsqlConnection(_connectionString)
        connection.Execute("DELETE FROM tasks;")

[<ApiController>]
[<Route("/status")>]
type StatusController () =
    inherit ControllerBase()

    [<HttpGet>]
    member __.Get() : Task<string> =
        async {
            return "ok"
        } |> Async.StartAsTask
