using Calendar.API.Data;
using Calendar.API.Models.Entities;
using Calendar.API.Exceptions;
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

        public async Task<IEnumerable<Tag>> GetAllAsync()
        {
            try 
            {
                return await _tags
                    .AsNoTracking()
                    .OrderBy(t => t.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error retrieving tags", ex);
            }
        }

        public async Task<Tag> GetByIdAsync(int id)
        {
            try
            {
                var tag = await _tags
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (tag == null)
                {
                    throw new KeyNotFoundException($"Tag with ID {id} not found");
                }

                return tag;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                throw new RepositoryException($"Error retrieving tag with ID {id}", ex);
            }
        }

        public async Task<Tag> GetByNameAsync(string name)
        {
            try
            {
                var tag = await _tags
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());

                if (tag == null)
                {
                    throw new KeyNotFoundException($"Tag with name '{name}' not found");
                }

                return tag;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                throw new RepositoryException($"Error retrieving tag with name '{name}'", ex);
            }
        }

        public async Task<Tag> AddAsync(Tag tag)
        {
            if (tag == null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                tag.Id = 0;
                var entry = await _tags.AddAsync(tag);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return entry.Entity;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new RepositoryException("Error creating tag", ex);
            }
        }

        public async Task UpdateAsync(Tag tag)
        {
            if (tag == null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!await _tags.AnyAsync(t => t.Id == tag.Id))
                {
                    throw new KeyNotFoundException($"Tag with ID {tag.Id} not found");
                }

                _tags.Update(tag);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                await transaction.RollbackAsync();
                throw new RepositoryException($"Error updating tag with ID {tag.Id}", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var tag = await _tags.FindAsync(id);
                if (tag == null)
                {
                    throw new KeyNotFoundException($"Tag with ID {id} not found");
                }

                _tags.Remove(tag);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                await transaction.RollbackAsync();
                throw new RepositoryException($"Error deleting tag with ID {id}", ex);
            }
        }
    }

}