using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TodoList.Models
{
    // Naming TodoTask to avoid name conflict with threading Task
    [Table("tasks")]
    public class TodoTask
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("text")]
        public string Text { get; set; }
        [Column("priority")]
        public int Priority { get; set; }
    }

    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options)
            : base(options)
        {
        }

        public DbSet<TodoTask> TodoTasks { get; set; }
    }
}