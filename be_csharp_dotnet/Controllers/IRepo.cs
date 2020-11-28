using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TodoList.Models;

namespace TodoList.Controllers
{
    public interface IRepo
    {
        Task Add( TodoTask task );

        Task Delete();
        Task<Dictionary<String, Object>> Get( Int32? page );

        Task Update( TodoTask task );
    }
}