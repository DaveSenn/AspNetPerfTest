module TodoList.App

open System
open System.Collections.Generic
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Dapper
open Npgsql
open TodoList.Models

// ---------------------------------
// Web app
// ---------------------------------


let connectionString = String.Format(
                        "Host={0};Port=5432;Username={1};Password={2};Database=todolist;",
                        Environment.GetEnvironmentVariable "DEV_PG_HOST",
                        Environment.GetEnvironmentVariable "DEV_PG_USER",
                        Environment.GetEnvironmentVariable "DEV_PG_PASSWORD")

let taskDeleteHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let connection =  new NpgsqlConnection(connectionString)
        connection.Execute("DELETE FROM tasks;") |> ignore
        text "ok" next ctx

let taskGetHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let connection =  new NpgsqlConnection(connectionString)
            do! Async.AwaitTask (connection.OpenAsync())
            let! results = Async.AwaitTask (connection.QueryAsync<TodoTask>("Select * FROM tasks ORDER BY priority asc"))
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
            let connection =  new NpgsqlConnection(connectionString)
            do! Async.AwaitTask (connection.OpenAsync())
            let sql = @"INSERT INTO tasks (text, priority) VALUES (@Text, @Priority)"
            let todotask = ctx.BindModelAsync<TodoTask>().Result
            Async.AwaitTask (connection.ExecuteAsync(sql, todotask)) |> Async.RunSynchronously |> ignore
            let taskHolder = Dictionary<string, obj>()
            taskHolder.Add("task", task)
            do! Async.AwaitTask (connection.CloseAsync())
            return! json taskHolder next ctx
        }

let taskPutHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let connection =  new NpgsqlConnection(connectionString)
            do! Async.AwaitTask (connection.OpenAsync())
            let sql = @"UPDATE tasks SET (text = @Text, priority = @Priority) WHERE id = @Id"
            let todotask = ctx.BindModelAsync<TodoTask>().Result
            Async.AwaitTask (connection.ExecuteAsync(sql, todotask)) |> Async.RunSynchronously |> ignore
            let taskHolder = Dictionary<string, obj>()
            taskHolder.Add("task", task)
            do! Async.AwaitTask (connection.CloseAsync())
            return! json taskHolder next ctx
        }


let statusHandler : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            return! text "ok" next ctx
        }

let webApp =
    choose [
        DELETE >=>
            choose [
                route "/tasks" >=> taskDeleteHandler
            ]
        GET >=>
            choose [
                route "/tasks" >=> taskGetHandler
                route "/status" >=> statusHandler
            ]
        POST >=>
            choose [
                route "/tasks" >=> taskPostHandler
            ]
        PUT >=>
            choose [
                route "/tasks" >=> taskPutHandler
            ]
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8000")
           .AllowAnyMethod()
           .AllowAnyHeader()
           |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    (match env.IsDevelopment() with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler)
        .UseHttpsRedirection()
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddFilter(fun l -> l.Equals LogLevel.Error)
           .AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    let contentRoot = Directory.GetCurrentDirectory()
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .UseUrls("http://localhost:8000")
        .Build()
        .Run()
    0