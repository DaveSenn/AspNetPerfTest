namespace TodoList

open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting

module Program =

    let CreateHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webBuilder ->
                webBuilder.UseStartup<Startup>().UseUrls("http://localhost:8000") |> ignore
            )

    [<EntryPoint>]
    let main args =
        printfn "Running on localhost port 8000"
        CreateHostBuilder(args).Build().Run()
        0
