using Calendar.API.DTOs.TodoDtos;

public interface ITodoService
{
    Task<IEnumerable<TodoResponseDto>> GetAllAsync();
    Task<TodoResponseDto> GetByIdAsync(int id);
    Task<TodoResponseDto> CreateAsync(TodoCreateDto dto);
    Task<TodoResponseDto> CreateSubTaskAsync(int parentId, TodoCreateDto dto);
    Task<TodoResponseDto> UpdateAsync(int id, TodoUpdateDto dto);
    Task DeleteAsync(int id);
}