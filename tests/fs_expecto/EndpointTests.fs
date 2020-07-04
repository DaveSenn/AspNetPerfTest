module Tests
open System.Text
open System.Net
open System.Net.Http
open Newtonsoft.Json
open Expecto
open Data
open TodoList.Models

type MutableTaskList = {
    Tasks : TodoTask array
    Position: int
    Length: int
}

[<Tests>]
let tests =
    testList "Endpoint Tests" [

        test "Test get tasks" {
            let taskText = "check pants pocket"
            let taskId = AddTask taskText 1
            let client = new HttpClient()
            let resp = Async.AwaitTask (client.GetStringAsync("http://localhost:8000/tasks")) |> Async.RunSynchronously
            let taskList = JsonConvert.DeserializeObject<MutableTaskList> resp
            let tasks = taskList.Tasks |> Array.toList
            let found = List.exists (fun (t:TodoTask) -> t.Text = taskText) tasks
            Expect.isTrue found "Added element not found"
        }

        test "Test post task" {
            let client = new HttpClient()
            let json = "{\"text\": \"Wowza\", \"priority\": 2}"
            let data = new StringContent(json, Encoding.UTF8, "application/json");
            let resp = Async.AwaitTask (client.PostAsync("http://localhost:8000/tasks", data)) |> Async.RunSynchronously
            Expect.equal resp.StatusCode HttpStatusCode.OK "Bad response"
        }

        test "Test status" {
            let client = new HttpClient()
            let resp = Async.AwaitTask (client.GetStringAsync("http://localhost:8000/status")) |> Async.RunSynchronously
            Expect.equal resp "ok" "Status fail"
        }

    ]