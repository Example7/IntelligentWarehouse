namespace Interfaces.Magazyn.Dtos
{
    public class LogAudytuChangeDto
    {
        public string FieldKey { get; set; } = string.Empty;
        public string FieldLabel { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? OldDisplayValue { get; set; }
        public string? NewDisplayValue { get; set; }
        public string ChangeType { get; set; } = string.Empty;
    }
}
