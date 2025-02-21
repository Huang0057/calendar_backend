using AutoMapper;
using Calendar.API.DTOs.TodoDtos;
using Calendar.API.Models.Entities;
using Calendar.API.Repositories;
using Calendar.API.Exceptions;
using Microsoft.EntityFrameworkCore;
using Calendar.API.Data;
using Calendar.API.Common.Enums;

namespace Calendar.API.Services
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _todoRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public TodoService(ITodoRepository todoRepository, IMapper mapper, ApplicationDbContext context)
        {
            _todoRepository = todoRepository;
            _mapper = mapper;
            _context = context;
        }

        // 統一的業務規則驗證方法
        private async Task ValidateTodoBusinessRules(TodoCreateDto dto, int? parentId = null)
        {
            var validationErrors = new List<string>();
            
            // 基本驗證
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                throw new BusinessValidationException("標題不能為空");
            }

            if (dto.Title.Length > 100)
            {
                throw new BusinessValidationException("標題長度不能超過100個字符");
            }

            // 日期驗證
            if (dto.DueDate.HasValue && dto.DueDate.Value < DateTime.UtcNow)
            {
                throw new BusinessValidationException("截止日期不能早於當前時間");
            }

            // 優先級驗證
            if (dto.Priority is < 0 or > (Common.Enums.Priority)2)
            {
                throw new BusinessValidationException("優先級必須在0到2之間");
            }

            // 驗證標籤是否存在
            if (dto.TagIds.Any())
            {
                var existingTagCount = await _context.Tags
                    .Where(t => dto.TagIds.Contains(t.Id))
                    .CountAsync();

                if (existingTagCount != dto.TagIds.Count)
                {
                    throw new BusinessValidationException("部分標籤不存在");
                }
            }
            
            if (validationErrors.Any())
            {
                throw new BusinessValidationException(
                    string.Join(Environment.NewLine, validationErrors));
            }

            // 父任務驗證
            if (parentId.HasValue)
            {
                var parentTodo = await _todoRepository.GetByIdAsync(parentId.Value);
                if (parentTodo == null)
                {
                    throw new KeyNotFoundException($"父任務 ID {parentId} 不存在");
                }

                // 檢查父任務的截止日期
                if (parentTodo.DueDate.HasValue && dto.DueDate.HasValue 
                    && dto.DueDate.Value > parentTodo.DueDate.Value)
                {
                    throw new BusinessValidationException("子任務的截止日期不能晚於父任務");
                }
            }
        }

        public async Task<IEnumerable<TodoResponseDto>> GetAllAsync()
        {
            var todos = await _todoRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<TodoResponseDto>>(todos);
        }

        public async Task<TodoResponseDto> GetByIdAsync(int id)
        {
            var todo = await _todoRepository.GetByIdAsync(id);
            if (todo == null)
            {
                throw new KeyNotFoundException($"待辦事項 ID {id} 不存在");
            }

            return _mapper.Map<TodoResponseDto>(todo);
        }

        public async Task<TodoResponseDto> CreateAsync(TodoCreateDto dto)
        {
            await ValidateTodoBusinessRules(dto);
            
            var todo = _mapper.Map<Todo>(dto);
            todo.IsCompleted = false;
            
            // 處理 Tags
            if (dto.TagIds.Any())
            {
                var tags = await _context.Tags
                    .Where(t => dto.TagIds.Contains(t.Id))
                    .ToListAsync();

                todo.TodoTags = tags.Select(tag => new TodoTag
                {
                    Todo = todo,
                    Tag = tag
                }).ToList();
            }
            
            var createdTodo = await _todoRepository.AddAsync(todo);
            return _mapper.Map<TodoResponseDto>(createdTodo);
        }
        public async Task<TodoResponseDto> CreateSubTaskAsync(int parentId, TodoCreateDto dto)
        {
            await ValidateTodoBusinessRules(dto, parentId);

            var todo = _mapper.Map<Todo>(dto);
            todo.IsCompleted = false;
            todo.CreatedAt = DateTime.UtcNow;
            todo.UpdatedAt = DateTime.UtcNow;
            todo.ParentId = parentId;

            // 處理 Tags
            if (dto.TagIds.Any())
            {
                var tags = await _context.Tags
                    .Where(t => dto.TagIds.Contains(t.Id))
                    .ToListAsync();

                todo.TodoTags = tags.Select(tag => new TodoTag
                {
                    Todo = todo,
                    Tag = tag
                }).ToList();
            }

            var createdSubTask = await _todoRepository.AddAsync(todo);
            return _mapper.Map<TodoResponseDto>(createdSubTask);
        }

        public async Task<TodoResponseDto> UpdateAsync(int id, TodoUpdateDto todoDto)
        {
            // 1. 先取得包含所有關聯的現有 Todo
            var todo = await _todoRepository.GetByIdForUpdateAsync(id);
            if (todo == null)
            {
                throw new KeyNotFoundException($"待辦事項 ID {id} 不存在");
            }

            // 2. 更新基本屬性
            todo.Title = todoDto.Title?.Trim() ?? todo.Title;
            todo.Description = todoDto.Description?.Trim();
            todo.IsCompleted = todoDto.IsCompleted ?? todo.IsCompleted;
            todo.DueDate = todoDto.DueDate;    
            if (todoDto.Priority.HasValue)
            {
                if (Enum.IsDefined(typeof(Priority), todoDto.Priority.Value))
                {
                    todo.Priority = todoDto.Priority.Value;
                }
                else
                {
                    throw new BusinessValidationException($"無效的優先級值: {todoDto.Priority.Value}");
                }
            }
            todo.UpdatedAt = DateTime.UtcNow;

            if (todoDto.IsCompleted.HasValue && todoDto.IsCompleted.Value && !todo.IsCompleted)
            {
                todo.CompletedAt = DateTime.UtcNow;
            }

            // 3. 處理標籤更新
            // 先清除所有現有的關聯
            todo.TodoTags.Clear();
            
            // 如果有新的標籤，則建立新的關聯
            if (todoDto.TagIds?.Any() == true)
            {
                // 取得所有相關的 Tag 實體
                var tags = await _context.Tags
                    .Where(t => todoDto.TagIds.Contains(t.Id))
                    .ToListAsync();

                if (tags.Count != todoDto.TagIds.Count)
                {
                    throw new BusinessValidationException("部分標籤不存在");
                }

                // 為每個標籤建立新的 TodoTag 關聯
                foreach (var tag in tags)
                {
                    todo.TodoTags.Add(new TodoTag 
                    { 
                        TodoId = todo.Id,
                        TagId = tag.Id,
                        Todo = todo,
                        Tag = tag
                    });
                }
            }

            // 4. 保存更新
            await _todoRepository.UpdateAsync(todo);

            // 5. 重新查詢以確保返回最新資料
            var updatedTodo = await _todoRepository.GetByIdAsync(id);
            return _mapper.Map<TodoResponseDto>(updatedTodo);
        }
        public async Task DeleteAsync(int id)
        {
            var todo = await _todoRepository.GetByIdAsync(id);
            if (todo == null)
            {
                throw new KeyNotFoundException($"待辦事項 ID {id} 不存在");
            }

            await _todoRepository.DeleteAsync(id);
        }
    }
}