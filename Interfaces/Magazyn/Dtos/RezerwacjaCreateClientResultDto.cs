namespace Interfaces.Magazyn.Dtos
{
    public class RezerwacjaCreateClientResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? ReservationId { get; set; }
        public string? Number { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAtUtc { get; set; }

        public static RezerwacjaCreateClientResultDto Fail(string message)
            => new() { Success = false, Message = message };

        public static RezerwacjaCreateClientResultDto Ok(int reservationId, string number, string status, DateTime createdAtUtc)
            => new()
            {
                Success = true,
                ReservationId = reservationId,
                Number = number,
                Status = status,
                CreatedAtUtc = createdAtUtc
            };
    }
}
