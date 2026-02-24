namespace Interfaces.Magazyn.Dtos
{
    public class RezerwacjaStatusChangeResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public IList<(string Field, string Error)> Errors { get; set; } = new List<(string Field, string Error)>();

        public static RezerwacjaStatusChangeResultDto Ok(string message)
            => new() { Success = true, Message = message };

        public static RezerwacjaStatusChangeResultDto Fail(string message)
            => new() { Success = false, Message = message };
    }
}
