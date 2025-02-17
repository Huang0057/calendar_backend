using Calendar.API.Data;
using Calendar.API.DTOs.TodoDtos;
using Calendar.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Todo>>> GetTodos()
        {        
            return await _context.Todos
                .Include(t => t.SubTasks)
                .Include(t => t.TodoTags)
                    .ThenInclude(tt => tt.Tag)
                .Where(t => t.ParentId == null)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoResponseDto>> GetTodo(int id)
        {
            var todo = await _context.Todos
                .Include(t => t.SubTasks)
                .Include(t => t.TodoTags)
                    .ThenInclude(tt => tt.Tag)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (todo == null)
            {
                return NotFound();
            }

            return MapToDto(todo);
        }

        private TodoResponseDto MapToDto(Todo todo)
        {
            return new TodoResponseDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted,
                CreatedAt = todo.CreatedAt,
                UpdatedAt = todo.UpdatedAt,
                CompletedAt = todo.CompletedAt,
                DueDate = todo.DueDate,
                Priority = todo.Priority,
                ParentId = todo.ParentId,
                SubTasks = todo.SubTasks.Select(MapToDto).ToList()
            };
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TodoCreateDto todoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(todoDto.Title))
            {
                ModelState.AddModelError("Title", "標題不能為空");
                return BadRequest(ModelState);
            }


            var todo = new Todo
            {
                Title = todoDto.Title.Trim(),
                Description = todoDto.Description?.Trim(),
                Priority = todoDto.Priority,
                DueDate = todoDto.DueDate,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();
           
            if (todoDto.TagIds?.Any() == true)
            {
                foreach (var tagId in todoDto.TagIds)
                {
                    var tag = await _context.Tags.FindAsync(tagId);
                    if (tag != null)
                    {
                        var todoTag = new TodoTag
                        {
                            TodoId = todo.Id,
                            TagId = tagId,
                            Todo = todo,
                            Tag = tag
                        };
                        _context.TodoTags.Add(todoTag);
                    }
                }
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todo);
        }
        [HttpPost("{parentId}/subtasks")]
        public async Task<IActionResult> CreateSubTask(int parentId, [FromBody] TodoCreateDto todoDto)
        {
            var parentTodo = await _context.Todos.FindAsync(parentId);
            if (parentTodo == null)
            {
                return NotFound("找不到指定的父層待辦事項");
            }

            var subTask = new Todo
            {
                Title = todoDto.Title.Trim(),
                Description = todoDto.Description?.Trim(),
                Priority = todoDto.Priority,
                DueDate = todoDto.DueDate,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                ParentId = parentId
            };

            _context.Todos.Add(subTask);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodo), new { id = subTask.Id }, subTask);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, [FromBody] TodoUpdateDto todoDto)
        {
            var todo = await _context.Todos
                .Include(t => t.TodoTags)
                .ThenInclude(tt => tt.Tag)
                .Include(t => t.SubTasks) 
                .FirstOrDefaultAsync(t => t.Id == id);

            if (todo == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(todoDto.Title))
            {
                todo.Title = todoDto.Title.Trim();
            }

            if (todoDto.Description != null)
            {
                todo.Description = todoDto.Description.Trim();
            }

            todo.Priority = todoDto.Priority;
            todo.DueDate = todoDto.DueDate;
            todo.IsCompleted = todoDto.IsCompleted;

            if (todoDto.TagIds != null)
            {
                _context.TodoTags.RemoveRange(todo.TodoTags);

                foreach (var tagId in todoDto.TagIds)
                {
                    var tag = await _context.Tags.FindAsync(tagId);
                    if (tag != null)
                    {
                        var todoTag = new TodoTag
                        {
                            TodoId = todo.Id,
                            TagId = tagId,
                            Todo = todo,
                            Tag = tag
                        };
                        _context.TodoTags.Add(todoTag);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok(MapToDto(todo));
        }    

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TodoExists(int id)
        {
            return _context.Todos.Any(e => e.Id == id);
        }
    }
}