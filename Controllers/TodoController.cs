using Calendar.API.Data;
using Calendar.API.DTOs.TodoDtos;
using Calendar.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;


namespace Calendar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TodoController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Todo>> GetTodo(int id)
        {
            var todo = await _context.Todos.FindAsync(id);

            if (todo == null)
            {
                return NotFound();
            }

            return todo;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TodoCreateDto todoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var todo = new Todo
            {
                Title = todoDto.Title,
                Description = todoDto.Description,
                DueDate = todoDto.DueDate,
                IsCompleted = false
            };

            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todo);
        }
    }
}