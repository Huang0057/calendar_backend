using Calendar.API.Data;
using Calendar.API.Models.Entities;
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

        // 獲取所有根層級的Todo
        public async Task<IEnumerable<Todo>> GetAllAsync()
        {
            return await _todos
                .AsNoTracking()
                .Include(t => t.SubTasks)  // 預加載Subtasks
                .Where(t => t.ParentId == null)  // 只獲取根層級任務
                .OrderByDescending(t => t.CreatedAt)  // 按建立時間排序
                .ToListAsync();
        }

        // 獲取單個Todo及其Subtasks
        public async Task<Todo> GetByIdAsync(int id)
        {
            return await _todos
                .AsNoTracking()
                .Include(t => t.SubTasks)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // 獲取用於更新的Todo
        public async Task<Todo> GetByIdForUpdateAsync(int id)
        {
            return await _todos
                .Include(t => t.SubTasks)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // 建立新的Todo
        public async Task<Todo> AddAsync(Todo todo)
        {
            // 設置建立和更新時間
            todo.CreatedAt = DateTime.UtcNow;
            todo.UpdatedAt = DateTime.UtcNow;
            
            await _todos.AddAsync(todo);
            return todo;
        }

        // 更新Todo
        public void Update(Todo todo)
        {
            todo.UpdatedAt = DateTime.UtcNow;
            _todos.Update(todo);
        }

        // 遞迴刪除Todo及其Subtasks
        public async Task DeleteAsync(int id)
        {
            var todo = await GetByIdForUpdateAsync(id);
            if (todo == null) return;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await DeleteRecursivelyAsync(todo);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // 遞迴刪除輔助方法
        private async Task DeleteRecursivelyAsync(Todo todo)
        {
            var subTasks = await _todos
                .Where(t => t.ParentId == todo.Id)
                .ToListAsync();

            foreach (var subTask in subTasks)
            {
                await DeleteRecursivelyAsync(subTask);
            }

            _todos.Remove(todo);
        }

        // 檢查Todo是否存在
        public async Task<bool> ExistsAsync(int id)
        {
            return await _todos
                .AsNoTracking()
                .AnyAsync(t => t.Id == id);
        }
    }
}


