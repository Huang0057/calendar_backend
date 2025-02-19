using AutoMapper;
using Calendar.API.DTOs.TodoDtos;
using Calendar.API.Models.Entities;
using Calendar.API.Repositories;

namespace Calendar.API.Services
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _todoRepository;
        private readonly IMapper _mapper;

        public TodoService(ITodoRepository todoRepository, IMapper mapper)
        {
            _todoRepository = todoRepository;
            _mapper = mapper;
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
            
            
            // 驗證業務規則
            await ValidateTodoBusinessRules(dto);

            // 建立實體並設置初始狀態
            var todo = _mapper.Map<Todo>(dto);
            todo.IsCompleted = false;


            // 保存到數據庫
            var createdTodo = await _todoRepository.AddAsync(todo);
            return _mapper.Map<TodoResponseDto>(createdTodo);
        }

        public async Task<TodoResponseDto> CreateSubTaskAsync(int parentId, TodoCreateDto dto)
        {
            // 驗證業務規則，包括父任務相關的驗證
            await ValidateTodoBusinessRules(dto, parentId);

            // 建立子任務實體
            var subTask = _mapper.Map<Todo>(dto);
            subTask.IsCompleted = false;
            subTask.CreatedAt = DateTime.UtcNow;
            subTask.UpdatedAt = DateTime.UtcNow;
            subTask.ParentId = parentId;

            // 保存到數據庫
            var createdSubTask = await _todoRepository.AddAsync(subTask);
            return _mapper.Map<TodoResponseDto>(createdSubTask);
        }

        public async Task<TodoResponseDto> UpdateAsync(int id, TodoUpdateDto todoDto)
        {
            var todo = await _todoRepository.GetByIdForUpdateAsync(id);
            if (todo == null)
            {
                throw new KeyNotFoundException($"待辦事項 ID {id} 不存在");
            }

            // 更新實體
            _mapper.Map(todoDto, todo);
            todo.UpdatedAt = DateTime.UtcNow;

            // 保存更改
            await _todoRepository.UpdateAsync(todo);
            return _mapper.Map<TodoResponseDto>(todo);
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

    [Serializable]
    public class BusinessValidationException : Exception
    {
        public BusinessValidationException(string message) : base(message)
        {
        }
    }
}