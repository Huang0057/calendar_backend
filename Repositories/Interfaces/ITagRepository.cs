using Calendar.API.Models.Entities;

public interface ITagRepository
{
    Task<IEnumerable<Tag>> GetAllAsync();
    Task<Tag> GetByIdAsync(int id);
    Task<Tag> GetByNameAsync(string name);
    Task<Tag> AddAsync(Tag tag);
    void Update(Tag tag);
    Task DeleteAsync(int id);
}