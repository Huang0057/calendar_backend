using Calendar.API.Models.Entities;

public interface ITodoRepository
{
    Task<IEnumerable<Todo>> GetAllAsync();
    Task<Todo> GetByIdAsync(int id);
    Task<Todo> GetByIdForUpdateAsync(int id);
    Task<Todo> AddAsync(Todo todo);
    void Update(Todo todo);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}