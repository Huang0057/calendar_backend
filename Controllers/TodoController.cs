using Calendar.API.DTOs.TodoDtos;
using Calendar.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Calendar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _todoService;
        private readonly ILogger<TodoController> _logger;

        public TodoController(ITodoService todoService, ILogger<TodoController> logger)
        {
            _todoService = todoService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoResponseDto>>> GetTodos()
        {
            try
            {
                var todos = await _todoService.GetAllAsync();
                return Ok(todos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all todos");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoResponseDto>> GetTodo(int id)
        {
            try
            {
                var todo = await _todoService.GetByIdAsync(id);
                return Ok(todo);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Todo with ID {TodoId} not found", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting todo {TodoId}", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TodoResponseDto>> CreateTodo([FromBody] TodoCreateDto todoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdTodo = await _todoService.CreateAsync(todoDto);
                return CreatedAtAction(nameof(GetTodo), new { id = createdTodo.Id }, createdTodo);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid todo creation attempt");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating todo");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost("{parentId}/subtasks")]
        public async Task<ActionResult<TodoResponseDto>> CreateSubTask(int parentId, [FromBody] TodoCreateDto todoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdSubTask = await _todoService.CreateSubTaskAsync(parentId, todoDto);
                return CreatedAtAction(nameof(GetTodo), new { id = createdSubTask.Id }, createdSubTask);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Parent todo with ID {ParentId} not found", parentId);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid subtask creation attempt");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating subtask");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TodoResponseDto>> UpdateTodo(int id, [FromBody] TodoUpdateDto todoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedTodo = await _todoService.UpdateAsync(id, todoDto);
                return Ok(updatedTodo);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Todo with ID {TodoId} not found for update", id);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid todo update attempt");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating todo {TodoId}", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            try
            {
                await _todoService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Todo with ID {TodoId} not found for deletion", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting todo {TodoId}", id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}