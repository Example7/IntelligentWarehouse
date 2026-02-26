namespace Interfaces.Magazyn.Dtos
{
    public class RezerwacjaCreateClientCommandDto
    {
        public int UserId { get; set; }
        public int WarehouseId { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public string? Note { get; set; }
        public List<RezerwacjaCreateClientItemCommandDto> Items { get; set; } = new();
    }

    public class RezerwacjaCreateClientItemCommandDto
    {
        public int ProductId { get; set; }
        public int? LocationId { get; set; }
        public decimal Quantity { get; set; }
    }
}
