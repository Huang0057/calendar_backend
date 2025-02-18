using Calendar.API.Data;
using Calendar.API.DTOs.TodoDtos;
using Calendar.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendar.API.Services
{
    public class TodoService : ITodoService
    {
        private readonly ApplicationDbContext _context;

        public TodoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TodoResponseDto>> GetAllAsync()
        {
            return await _context.Todos
                .AsNoTracking()
                .Include(t => t.SubTasks)
                .Where(t => t.ParentId == null)
                .Select(t => new TodoResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    CompletedAt = t.CompletedAt,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    ParentId = t.ParentId,
                    SubTasks = t.SubTasks.Select(s => new TodoResponseDto
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Description = s.Description,
                        IsCompleted = s.IsCompleted,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt,
                        CompletedAt = s.CompletedAt,
                        DueDate = s.DueDate,
                        Priority = s.Priority,
                        ParentId = s.ParentId
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<TodoResponseDto> GetByIdAsync(int id)
        {
            var todo = await _context.Todos
                .AsNoTracking()
                .Include(t => t.SubTasks)
                .Select(t => new TodoResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    IsCompleted = t.IsCompleted,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    CompletedAt = t.CompletedAt,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    ParentId = t.ParentId,
                    SubTasks = t.SubTasks.Select(s => new TodoResponseDto
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Description = s.Description,
                        IsCompleted = s.IsCompleted,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt,
                        CompletedAt = s.CompletedAt,
                        DueDate = s.DueDate,
                        Priority = s.Priority,
                        ParentId = s.ParentId
                    }).ToList()
                })
                .FirstOrDefaultAsync(t => t.Id == id);

            if (todo == null)
                throw new KeyNotFoundException($"Todo with ID {id} not found.");

            return todo;
        }

        public async Task<TodoResponseDto> CreateAsync(TodoCreateDto dto)
        {
            ValidateTodoCreate(dto);

            var todo = new Todo
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                Priority = dto.Priority,
                DueDate = dto.DueDate,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Todos.Add(todo);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(todo.Id);
        }

        public async Task<TodoResponseDto> CreateSubTaskAsync(int parentId, TodoCreateDto dto)
        {
            var parentExists = await _context.Todos.AnyAsync(t => t.Id == parentId);
            if (!parentExists)
                throw new KeyNotFoundException($"Parent todo with ID {parentId} not found.");

            ValidateTodoCreate(dto);

            var subTask = new Todo
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                Priority = dto.Priority,
                DueDate = dto.DueDate,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ParentId = parentId
            };

            _context.Todos.Add(subTask);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(subTask.Id);
        }

        public async Task<TodoResponseDto> UpdateAsync(int id, TodoUpdateDto dto)
        {
            var todo = await _context.Todos
                .FirstOrDefaultAsync(t => t.Id == id);

            if (todo == null)
                throw new KeyNotFoundException($"Todo with ID {id} not found.");

            UpdateTodoProperties(todo, dto);
                
            todo.UpdatedAt = DateTime.UtcNow;
            if (todo.IsCompleted && !todo.CompletedAt.HasValue)
            {
                todo.CompletedAt = DateTime.UtcNow;
            }
            else if (!todo.IsCompleted)
            {
                todo.CompletedAt = null;
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(todo.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var todo = await _context.Todos
                .Include(t => t.SubTasks)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (todo == null)
                throw new KeyNotFoundException($"Todo with ID {id} not found.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await DeleteTodoRecursively(todo);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task DeleteTodoRecursively(Todo todo)
        {
            if (todo.SubTasks?.Any() == true)
            {
                foreach (var subTask in todo.SubTasks.ToList())
                {
                    await DeleteTodoRecursively(subTask);
                }
            }

            _context.Todos.Remove(todo);
        }

        private void UpdateTodoProperties(Todo todo, TodoUpdateDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.Title))
            {
                todo.Title = dto.Title.Trim();
            }

            if (dto.Description != null)
            {
                todo.Description = dto.Description.Trim();
            }

            todo.Priority = dto.Priority;
            todo.DueDate = dto.DueDate;
            todo.IsCompleted = dto.IsCompleted;
        }

        private void ValidateTodoCreate(TodoCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title cannot be empty or whitespace.");
        }
    }
}