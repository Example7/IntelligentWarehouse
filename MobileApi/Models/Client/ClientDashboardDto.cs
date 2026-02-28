namespace MobileApi.Models.Client;

public sealed class ClientDashboardDto
{
    public int ActiveOrdersCount { get; set; }
    public int PostedOrdersCount { get; set; }
    public int ReservationsCount { get; set; }
    public List<ClientOrderListItemDto> RecentOrders { get; set; } = new();
    public List<ClientReservationListItemDto> RecentReservations { get; set; } = new();
}
