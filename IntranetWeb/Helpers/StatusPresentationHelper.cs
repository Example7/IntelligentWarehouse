namespace IntranetWeb.Helpers;

public static class StatusPresentationHelper
{
    public static string ReservationLabel(string? status)
    {
        return MapLabel(status, StatusKind.Reservation);
    }

    public static string WzLabel(string? status)
    {
        return MapLabel(status, StatusKind.Wz);
    }

    public static string BadgeClass(string? status)
    {
        var normalized = Normalize(status);

        if (normalized is "posted" or "closed" or "completed" or "convertedtowz" or "realized" or "issued" or "accepted" or "confirmed" or "active" or "released")
        {
            return "entity-badge--success";
        }

        if (normalized is "draft")
        {
            return "entity-badge--warning";
        }

        if (normalized is "cancelled" or "canceled" or "rejected" or "expired")
        {
            return "entity-badge--muted";
        }

        return "entity-badge--muted";
    }

    private static string MapLabel(string? status, StatusKind kind)
    {
        var normalized = Normalize(status);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return "-";
        }

        return kind switch
        {
            StatusKind.Reservation => normalized switch
            {
                "draft" => "Robocza",
                "accepted" or "confirmed" => "Zaakceptowana",
                "active" => "Aktywna",
                "convertedtowz" or "realized" or "completed" => "Zrealizowana",
                "rejected" => "Odrzucona",
                "cancelled" or "canceled" => "Anulowana",
                "expired" => "Przeterminowana",
                "released" => "Zrealizowana",
                _ => status ?? "-"
            },
            StatusKind.Wz => normalized switch
            {
                "draft" => "Robocze",
                "issued" => "Wydane",
                "posted" => "Zaksięgowane",
                "cancelled" or "canceled" => "Anulowane",
                _ => status ?? "-"
            },
            _ => status ?? "-"
        };
    }

    private static string Normalize(string? status)
    {
        return (status ?? string.Empty).Trim().ToLowerInvariant();
    }

    private enum StatusKind
    {
        Reservation,
        Wz
    }
}
