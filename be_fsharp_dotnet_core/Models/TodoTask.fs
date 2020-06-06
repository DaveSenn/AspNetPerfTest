namespace TodoList.Models

// Naming TodoTask to avoid name conflict with threading Task
type TodoTask (id, text, priority) =
    member val Id : int = id with get, set
    member val Text : string = text with get, set
    member val Priority : int = priority with get, set

type TaskList = {
    Tasks : TodoTask list
    Position: int
    Length: int
}
