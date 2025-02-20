using Microsoft.AspNetCore.Mvc;
using Calendar.API.Models.Entities;
using Calendar.API.Services.Interfaces;
using Calendar.API.Exceptions;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>>> GetAllTags()
        {
            try
            {
                var tags = await _tagService.GetAllTagsAsync();
                return Ok(tags);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all tags");
                return StatusCode(500, "An error occurred while retrieving tags");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tag>> GetTagById(int id)
        {
            try
            {
                var tag = await _tagService.GetTagByIdAsync(id);
                return Ok(tag);
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
        public async Task<ActionResult<Tag>> GetTagByName(string name)
        {
            try
            {
                var tag = await _tagService.GetTagByNameAsync(name);
                return Ok(tag);
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
        public async Task<ActionResult<Tag>> CreateTag([FromBody] Tag tag)
        {
            try
            {
                var createdTag = await _tagService.CreateTagAsync(tag);
                return CreatedAtAction(nameof(GetTagById), new { id = createdTag.Id }, createdTag);
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
                _logger.LogWarning(ex, "Attempted to create duplicate tag: {Name}", tag.Name);
                return Conflict(ex.Message);
            }
            catch (ServiceException ex)
            {
                _logger.LogError(ex, "Error occurred while creating tag");
                return StatusCode(500, "An error occurred while creating the tag");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTag(int id, [FromBody] Tag tag)
        {
            if (id != tag.Id)
            {
                return BadRequest("Tag ID mismatch");
            }

            try
            {
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
                _logger.LogWarning(ex, "Attempted to update to duplicate tag name: {Name}", tag.Name);
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