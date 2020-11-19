module TodoList.Handlers

open Microsoft.AspNetCore.Http
open System
open System.Threading.Tasks
open System.Text.Json
open System.Collections.Generic
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Dapper
open Npgsql
open SnazzGenerator
open TodoList.Models

let jsonOptions = JsonSerializerOptions()
jsonOptions.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase

let insertSql = SnazzGen<TodoTask>("Id", "tasks").BuildInsert()
let updateSql = SnazzGen<TodoTask>("Id", "tasks").BuildUpdate()

let connectionString = String.Format(
                        "Host={0};Port={1};Username={2};Password={3};Database=todolist;",
                        Environment.GetEnvironmentVariable "DEV_PG_HOST",
                        Environment.GetEnvironmentVariable "DEV_PG_PORT",
                        Environment.GetEnvironmentVariable "DEV_PG_USER",
                        Environment.GetEnvironmentVariable "DEV_PG_PASSWORD")

let getPageNumber(ctx: HttpContext) =
    let page = ctx.GetQueryStringValue "page"
    match page with
    | Ok p -> 
        p |> int
    | Error _ -> 1

let taskDeleteHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        use connection =  new NpgsqlConnection(connectionString)
        connection.Execute("DELETE FROM tasks;") |> ignore
        text "ok" next ctx

let taskGetHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            use connection =  new NpgsqlConnection(connectionString)
            let limit = 10
            let page = getPageNumber(ctx)
            let offset = ((page - 1) * limit)
            let sql = "SELECT * FROM tasks ORDER BY priority asc OFFSET " + offset.ToString() + " LIMIT " + limit.ToString()
            do! connection.OpenAsync()
            let! results = Async.AwaitTask (connection.QueryAsync<TodoTask>(sql))
            let taskList = List.ofSeq results
            let tasks = {
                Tasks=taskList;
                Position=offset;
                Page=page;
            }
            do! connection.CloseAsync()
            return! json tasks next ctx
        }

let taskPostHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            use connection =  new NpgsqlConnection(connectionString)
            do! connection.OpenAsync()
            // let todotask = ctx.BindModelAsync<TodoTask>().Result
            // ctx.BindModelAsync will fail due to a bug in Giraffe, so just do it manually instead:
            let body = ctx.ReadBodyFromRequestAsync().Result
            let todotask = JsonSerializer.Deserialize<TodoTask>(body, jsonOptions)
            connection.ExecuteAsync(insertSql, todotask).Result |> ignore
            let taskHolder = Dictionary<string, obj>()
            taskHolder.Add("task", todotask)
            do! connection.CloseAsync()
            return! json taskHolder next ctx
        }

let taskPutHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            use connection =  new NpgsqlConnection(connectionString)
            do! connection.OpenAsync()
            // let todotask = ctx.BindModelAsync<TodoTask>().Result
            // ctx.BindModelAsync will fail due to a bug in Giraffe, so just do it manually instead:
            let body = ctx.ReadBodyFromRequestAsync().Result
            let todotask = JsonSerializer.Deserialize<TodoTask>(body, jsonOptions)
            connection.ExecuteAsync(updateSql, todotask).Result |> ignore
            let taskHolder = Dictionary<string, obj>()
            taskHolder.Add("task", todotask)
            do! connection.CloseAsync()
            return! json taskHolder next ctx
        }


let statusHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            return! text "ok" next ctx
        }