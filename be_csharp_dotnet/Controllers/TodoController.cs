using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TodoList.Models;

namespace TodoList.Controllers
{
    [ApiController]
    [Route( "[controller]" )]
    public class TodoController : ControllerBase
    {
        private readonly IRepo _repo;

        public TodoController( IRepo repo ) =>
            _repo = repo;

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            await _repo.Delete();
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> Get( Int32? page )
        {
            var tasks = await _repo.Get( page );
            return Ok( tasks );
        }

        [HttpPost]
        public async Task<IActionResult> Post( TodoTask task )
        {
            await _repo.Add( task );

            return Ok( new Dictionary<String, TodoTask>
            {
                { "task", task }
            } );
        }

        [HttpPut]
        public async Task<IActionResult> Put( TodoTask task )
        {
            await _repo.Update( task );

            return Ok( new Dictionary<String, TodoTask>
            {
                { "task", task }
            } );
        }
    }
}