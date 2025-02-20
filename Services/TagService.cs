using Calendar.API.Models.Entities;
using Calendar.API.Repositories;
using Calendar.API.Services.Interfaces;
using Calendar.API.Exceptions;

namespace Calendar.API.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        }

        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            try
            {
                return await _tagRepository.GetAllAsync();
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException("Failed to retrieve tags", ex);
            }
        }

        public async Task<Tag> GetTagByIdAsync(int id)
        {
            try
            {
                return await _tagRepository.GetByIdAsync(id);
            }
            catch (KeyNotFoundException ex)
            {
                throw new EntityNotFoundException($"Tag with ID {id} was not found", ex);
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to retrieve tag with ID {id}", ex);
            }
        }

        public async Task<Tag> GetTagByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Tag name cannot be empty or whitespace", nameof(name));
            }

            try
            {
                return await _tagRepository.GetByNameAsync(name);
            }
            catch (KeyNotFoundException ex)
            {
                throw new EntityNotFoundException($"Tag with name '{name}' was not found", ex);
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to retrieve tag with name '{name}'", ex);
            }
        }

        public async Task<Tag> CreateTagAsync(Tag tag)
        {
            if (tag == null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            if (string.IsNullOrWhiteSpace(tag.Name))
            {
                throw new ArgumentException("Tag name cannot be empty or whitespace");
            }

            try
            {
                // 檢查標籤名稱是否已存在
                if (await IsTagNameExistsAsync(tag.Name))
                {
                    throw new DuplicateEntityException($"Tag with name '{tag.Name}' already exists");
                }

                return await _tagRepository.AddAsync(tag);
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException("Failed to create tag", ex);
            }
        }

        public async Task UpdateTagAsync(Tag tag)
        {
            if (tag == null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            if (string.IsNullOrWhiteSpace(tag.Name))
            {
                throw new ArgumentException("Tag name cannot be empty or whitespace");
            }

            try
            {
                // 檢查標籤是否存在
                await GetTagByIdAsync(tag.Id);

                // 檢查更新後的名稱是否與其他標籤重複
                if (await IsTagNameExistsAsync(tag.Name, tag.Id))
                {
                    throw new DuplicateEntityException($"Tag with name '{tag.Name}' already exists");
                }

                await _tagRepository.UpdateAsync(tag);
            }
            catch (EntityNotFoundException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to update tag with ID {tag.Id}", ex);
            }
        }

        public async Task DeleteTagAsync(int id)
        {
            try
            {
                // 檢查標籤是否存在
                await GetTagByIdAsync(id);

                await _tagRepository.DeleteAsync(id);
            }
            catch (EntityNotFoundException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to delete tag with ID {id}", ex);
            }
        }

        public async Task<bool> IsTagNameExistsAsync(string name, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Tag name cannot be empty or whitespace", nameof(name));
            }

            try
            {
                var existingTag = await _tagRepository.GetByNameAsync(name);
                
                // 如果找不到標籤，表示名稱不存在
                if (existingTag == null)
                {
                    return false;
                }

                // 如果提供了排除ID，且找到的標籤ID與排除ID相同，表示是同一個標籤
                if (excludeId.HasValue && existingTag.Id == excludeId.Value)
                {
                    return false;
                }

                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
            catch (RepositoryException ex)
            {
                throw new ServiceException($"Failed to check tag name existence: {name}", ex);
            }
        }
    }
}