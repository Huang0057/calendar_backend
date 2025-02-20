using Calendar.API.Models.Entities;

namespace Calendar.API.Services.Interfaces
{
    public interface ITagService
    {
        /// 獲取所有標籤        
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        
        /// 根據ID獲取標籤        
        Task<Tag> GetTagByIdAsync(int id);
        
        /// 根據名稱獲取標籤        
        Task<Tag> GetTagByNameAsync(string name);
        
        /// 創建新標籤        
        Task<Tag> CreateTagAsync(Tag tag);
        
        /// 更新標籤        
        Task UpdateTagAsync(Tag tag);
        
        /// 刪除標籤        
        Task DeleteTagAsync(int id);
        
        /// 檢查標籤名稱是否已存在        
        Task<bool> IsTagNameExistsAsync(string name, int? excludeId = null);
    }
}