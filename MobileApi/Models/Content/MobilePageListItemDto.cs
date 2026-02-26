namespace MobileApi.Models.Content;

public sealed class MobilePageListItemDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Position { get; set; }
}
