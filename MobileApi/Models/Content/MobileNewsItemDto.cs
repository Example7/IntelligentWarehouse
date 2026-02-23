namespace MobileApi.Models.Content;

public sealed class MobileNewsItemDto
{
    public int Id { get; set; }
    public string LinkTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Position { get; set; }
}
