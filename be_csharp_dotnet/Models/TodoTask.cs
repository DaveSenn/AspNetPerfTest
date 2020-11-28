using System;

namespace TodoList.Models
{
    public class TodoTask
    {
        public Int32 Id { get; set; }
        public String Text { get; set; }
        public Int32 Priority { get; set; }
    }
}