using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Models;

namespace TodoList.Controllers
{
    [Route("/tasks")]
    [ApiController]
    public class TodoController : ControllerBase
    {

        private readonly TodoContext _context;

        public TodoController(TodoContext context) => _context = context;

        // GET tasks
        [HttpGet]
        public async Task<ActionResult<Dictionary<string, dynamic>>> Get()
        {
            Dictionary<string, dynamic> tasks = new Dictionary<string, dynamic>();
            IEnumerable<TodoTask> results = await _context.TodoTasks.ToListAsync();
            tasks.Add("tasks", results);
            tasks.Add("position", 0);
            tasks.Add("length", results.Count());
            return tasks;
        }

        // POST tasks
        [HttpPost]
        public async Task<ActionResult<Dictionary<string, TodoTask>>> Post(TodoTask task)
        {
            _context.TodoTasks.Add(task);
            await _context.SaveChangesAsync();
            return new Dictionary<string, TodoTask>() {
                { "task", task }
            };
        }

        // PUT tasks
        [HttpPut()]
        public async Task<ActionResult<Dictionary<string, TodoTask>>> Put(TodoTask task)
        {
            _context.Entry(task).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return new Dictionary<string, TodoTask>() {
                { "task", task }
            };
        }

        // DELETE tasks
        [HttpDelete()]
        public void Delete()
        {
            _context.Database.ExecuteSqlCommand("delete from tasks");
        }
    }
}
