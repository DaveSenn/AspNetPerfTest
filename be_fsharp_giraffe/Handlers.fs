module TodoList.Handlers

open Microsoft.AspNetCore.Http
open System
open System.Text.Json
open System.Collections.Generic
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Dapper
open Npgsql
open TodoList.Models

let jsonOptions = JsonSerializerOptions()
jsonOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase

let connectionString = String.Format(
                        "Host={0};Port={1};Username={2};Password={3};Database=todolist;",
                        Environment.GetEnvironmentVariable "DEV_PG_HOST",
                        Environment.GetEnvironmentVariable "DEV_PG_PORT",
                        Environment.GetEnvironmentVariable "DEV_PG_USER",
                        Environment.GetEnvironmentVariable "DEV_PG_PASSWORD")

let taskDeleteHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        use connection =  new NpgsqlConnection(connectionString)
        connection.Execute("DELETE FROM tasks;") |> ignore
        text "ok" next ctx

let taskGetHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            use connection =  new NpgsqlConnection(connectionString)
            do! Async.AwaitTask (connection.OpenAsync())
            let! results = Async.AwaitTask (connection.QueryAsync<TodoTask>("SELECT * FROM tasks ORDER BY priority asc"))
            let taskList = List.ofSeq results
            let tasks = {
                Tasks=taskList;
                Position=0;
                Length=taskList.Length
            }
            do! Async.AwaitTask (connection.CloseAsync())
            return! json tasks next ctx
        }

let taskPostHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            use connection =  new NpgsqlConnection(connectionString)
            do! Async.AwaitTask (connection.OpenAsync())
            let sql = @"INSERT INTO tasks (text, priority) VALUES (@Text, @Priority)"
            // let todotask = ctx.BindModelAsync<TodoTask>().Result
            // ctx.BindModelAsync will fail due to a bug in Giraffe, so just do it manually instead:
            let body = ctx.ReadBodyFromRequestAsync().Result
            let todotask = JsonSerializer.Deserialize<TodoTask>(body, jsonOptions)
            connection.ExecuteAsync(sql, todotask).Result |> ignore
            let taskHolder = Dictionary<string, obj>()
            taskHolder.Add("task", todotask)
            do! Async.AwaitTask (connection.CloseAsync())
            return! json taskHolder next ctx
        }

let taskPutHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            use connection =  new NpgsqlConnection(connectionString)
            do! Async.AwaitTask (connection.OpenAsync())
            let sql = @"UPDATE tasks SET text = '@Text', priority = @Priority WHERE id = @Id"
            // let todotask = ctx.BindModelAsync<TodoTask>().Result
            // ctx.BindModelAsync will fail due to a bug in Giraffe, so just do it manually instead:
            let body = ctx.ReadBodyFromRequestAsync().Result
            let todotask = JsonSerializer.Deserialize<TodoTask>(body, jsonOptions)
            connection.ExecuteAsync(sql, todotask).Result |> ignore
            let taskHolder = Dictionary<string, obj>()
            taskHolder.Add("task", todotask)
            do! Async.AwaitTask (connection.CloseAsync())
            return! json taskHolder next ctx
        }


let statusHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            return! text "ok" next ctx
        }