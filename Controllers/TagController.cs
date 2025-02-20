using Microsoft.AspNetCore.Mvc;
using Calendar.API.Models.Entities;
using Calendar.API.Services.Interfaces;
using Calendar.API.Exceptions;
using Calendar.API.DTOs.TagDtos;

namespace Calendar.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;
        private readonly ILogger<TagController> _logger;

        public TagController(ITagService tagService, ILogger<TagController> logger)
        {
            _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private static TagDto ToDto(Tag tag)
        {
            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Color = tag.Color
            };
        }

        private static Tag ToEntity(CreateTagDto dto)
        {
            return new Tag
            {
                Name = dto.Name,
                Color = dto.Color
            };
        }

        private static Tag ToEntity(UpdateTagDto dto, int id)
        {
            return new Tag
            {
                Id = id,
                Name = dto.Name,
                Color = dto.Color
            };
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagDto>>> GetAllTags()
        {
            try
            {
                var tags = await _tagService.GetAllTagsAsync();
                return Ok(tags.Select(ToDto));
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all tags");
                return StatusCode(500, "An error occurred while retrieving tags");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TagDto>> GetTagById(int id)
        {
            try
            {
                var tag = await _tagService.GetTagByIdAsync(id);
                return Ok(ToDto(tag));
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Tag not found with ID: {Id}", id);
                return NotFound(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving tag with ID: {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the tag");
            }
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<TagDto>> GetTagByName(string name)
        {
            try
            {
                var tag = await _tagService.GetTagByNameAsync(name);
                return Ok(ToDto(tag));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid tag name provided: {Name}", name);
                return BadRequest(ex.Message);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Tag not found with name: {Name}", name);
                return NotFound(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving tag with name: {Name}", name);
                return StatusCode(500, "An error occurred while retrieving the tag");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TagDto>> CreateTag([FromBody] CreateTagDto createTagDto)
        {
            try
            {
                var tag = ToEntity(createTagDto);
                var createdTag = await _tagService.CreateTagAsync(tag);
                var createdTagDto = ToDto(createdTag);
                
                return CreatedAtAction(
                    nameof(GetTagById),
                    new { id = createdTagDto.Id },
                    createdTagDto
                );
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null tag provided");
                return BadRequest("Tag cannot be null");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid tag data provided");
                return BadRequest(ex.Message);
            }
            catch (DuplicateEntityException ex)
            {
                _logger.LogWarning(ex, "Attempted to create duplicate tag: {Name}", createTagDto.Name);
                return Conflict(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while creating tag");
                return StatusCode(500, "An error occurred while creating the tag");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTag(int id, [FromBody] UpdateTagDto updateTagDto)
        {
            try
            {
                var tag = ToEntity(updateTagDto, id);
                await _tagService.UpdateTagAsync(tag);
                return NoContent();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null tag provided for update");
                return BadRequest("Tag cannot be null");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid tag data provided for update");
                return BadRequest(ex.Message);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Tag not found for update with ID: {Id}", id);
                return NotFound(ex.Message);
            }
            catch (DuplicateEntityException ex)
            {
                _logger.LogWarning(ex, "Attempted to update to duplicate tag name: {Name}", updateTagDto.Name);
                return Conflict(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while updating tag with ID: {Id}", id);
                return StatusCode(500, "An error occurred while updating the tag");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            try
            {
                await _tagService.DeleteTagAsync(id);
                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Tag not found for deletion with ID: {Id}", id);
                return NotFound(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while deleting tag with ID: {Id}", id);
                return StatusCode(500, "An error occurred while deleting the tag");
            }
        }
    }
}