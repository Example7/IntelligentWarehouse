namespace MobileApi.Models.Client;

public sealed class ClientAttachmentDto
{
    public long AttachmentId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public int DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; }
}
