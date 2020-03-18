namespace Todolist.Models

// Naming TodoTask to avoid name conflict with threading Task
type TodoTask () =
    member val Id : int = 0 with get, set
    member val Text : string = "" with get, set
    member val Priority : int = 0 with get, set
    