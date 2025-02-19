using Calendar.API.Data;
using Calendar.API.Models.Entities;
using Calendar.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Calendar.API.Repositories
{
    public class TodoRepository : ITodoRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Todo> _todos;

        public TodoRepository(ApplicationDbContext context)
        {
            _context = context;
            _todos = context.Todos;
        }

        // 獲取所有RootTodo，包括其Subtask
        public async Task<IEnumerable<Todo>> GetAllAsync()
        {
            try
            {
                return await _todos
                    .AsNoTracking()  // 提高性能，因為我們只是讀取數據
                    .Include(t => t.SubTasks)  // 預加載Subtask
                    .Where(t => t.ParentId == null)  // 只獲取RootTodo
                    .OrderByDescending(t => t.CreatedAt)  // 按建立時間排序
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error retrieving todos", ex);
            }
        }

        // 獲取特定RootTodo的所有Subtask
        public async Task<IEnumerable<Todo>> GetSubTasksAsync(int parentId)
        {
            try
            {
                return await _todos
                    .AsNoTracking()
                    .Where(t => t.ParentId == parentId)
                    .OrderBy(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Error retrieving subtasks for parent ID {parentId}", ex);
            }
        }

        // 獲取單個Todo及其Subtasks
        public async Task<Todo> GetByIdAsync(int id)
        {
            try
            {
                var todo = await _todos
                    .AsNoTracking()
                    .Include(t => t.SubTasks)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (todo == null)
                {
                    throw new KeyNotFoundException($"Todo with ID {id} not found");
                }

                return todo;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                throw new RepositoryException($"Error retrieving todo with ID {id}", ex);
            }
        }

        // 獲取用於更新的Todo        
        public async Task<Todo> GetByIdForUpdateAsync(int id)
        {
            try
            {
                var todo = await _todos
                    .Include(t => t.SubTasks)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (todo == null)
                {
                    throw new KeyNotFoundException($"Todo with ID {id} not found");
                }

                return todo;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                throw new RepositoryException($"Error retrieving todo for update with ID {id}", ex);
            }
        }

        // 建立新的Todo
        public async Task<Todo> AddAsync(Todo todo)
        {
            if (todo == null)
            {
                throw new ArgumentNullException(nameof(todo));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 設置建立和更新時間
                var now = DateTime.UtcNow;
                todo.CreatedAt = now;
                todo.UpdatedAt = now;
                
                await _todos.AddAsync(todo);
                await SaveChangesAsync();
                await transaction.CommitAsync();
                
                return todo;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new RepositoryException("Error creating todo", ex);
            }
        }
        public async Task UpdateAsync(Todo todo)
        {
            if (todo == null)
            {
                throw new ArgumentNullException(nameof(todo));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 檢查實體是否存在
                if (!await ExistsAsync(todo.Id))
                {
                    throw new KeyNotFoundException($"Todo with ID {todo.Id} not found");
                }

                todo.UpdatedAt = DateTime.UtcNow;
                _todos.Update(todo);
                
                await SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                await transaction.RollbackAsync();
                throw new RepositoryException($"Error updating todo with ID {todo.Id}", ex);
            }
        }

        // 遞迴刪除Todo及其Subtasks
        public async Task DeleteAsync(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var todo = await GetByIdForUpdateAsync(id);
                await DeleteRecursivelyAsync(todo);
                await SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new RepositoryException($"Error deleting todo with ID {id}", ex);
            }
        }

        // 遞迴刪除Todo及其Subtask
        private async Task DeleteRecursivelyAsync(Todo todo)
        {
            // 首先刪除所有Subtask
            var subTasks = await GetSubTasksAsync(todo.Id);
            foreach (var subTask in subTasks)
            {
                await DeleteRecursivelyAsync(subTask);
            }

            // 然後刪除當前任務
            _todos.Remove(todo);
        }

        // 檢查Todo是否存在
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _todos
                    .AsNoTracking()
                    .AnyAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Error checking existence of todo with ID {id}", ex);
            }
        }

        private async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // 處理並發衝突
                throw new RepositoryException("A concurrency conflict occurred while saving changes", ex);
            }
            catch (DbUpdateException ex)
            {
                // 處理數據庫更新錯誤
                throw new RepositoryException("An error occurred while saving changes to the database", ex);
            }
        }
    }

}


