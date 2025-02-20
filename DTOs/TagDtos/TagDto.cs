namespace Calendar.API.DTOs.TagDtos
{
    public class TagDto
    {
        public int? Id { get; set; } 
        public required string Name { get; set; }
        public required string Color { get; set; }
    }
}