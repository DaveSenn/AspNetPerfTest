namespace TodoList.Models

// Naming TodoTask to avoid name conflict with threading Task
[<CLIMutable>]
type TodoTask = {
    Id : int
    Text : string
    Priority : int
}

type TaskList = {
    Tasks : TodoTask list
    Position: int
    Page: int
}
