using System.ComponentModel.DataAnnotations;

namespace MobileApi.Models.Client;

public sealed class ClientCreateReservationRequestDto
{
    [Required]
    public int WarehouseId { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    [MinLength(1)]
    public List<ClientCreateReservationItemRequestDto> Items { get; set; } = new();
}

public sealed class ClientCreateReservationItemRequestDto
{
    [Required]
    public int ProductId { get; set; }

    public int? LocationId { get; set; }

    [Range(typeof(decimal), "0.001", "999999999999", ErrorMessage = "Ilość musi być większa od zera.")]
    public decimal Quantity { get; set; }
}
