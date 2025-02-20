// UpdateTagDto.cs
namespace Calendar.API.DTOs.TagDtos
{
    public class UpdateTagDto
    {
        public required string Name { get; set; }
        public required string Color { get; set; }
    }
}