namespace TodoList.Models
{
    // Naming TodoTask to avoid name conflict with threading Task
    public class TodoTask
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Priority { get; set; }
    }
}