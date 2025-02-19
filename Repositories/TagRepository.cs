using Calendar.API.Data;
using Calendar.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calendar.API.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Tag> _tags;

        public TagRepository(ApplicationDbContext context)
        {
            _context = context;
            _tags = context.Tags;
        }

        // 獲取所有Tag
        public async Task<IEnumerable<Tag>> GetAllAsync()
        {
            return await _tags
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        // 獲取單個Tag
        public async Task<Tag> GetByIdAsync(int id)
        {
            return await _tags
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // 根據名稱查找Tag
        public async Task<Tag> GetByNameAsync(string name)
        {
            return await _tags
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
        }

        // 建立新Tag
        public async Task<Tag> AddAsync(Tag tag)
        {        
            await _tags.AddAsync(tag);
            return tag;
        }

        // 更新Tag
        public void Update(Tag tag)
        {            
            _tags.Update(tag);
        }

        // 刪除Tag
        public async Task DeleteAsync(int id)
        {
            var tag = await _tags.FindAsync(id);
            if (tag != null)
            {
                _tags.Remove(tag);
            }
        }

    }
}