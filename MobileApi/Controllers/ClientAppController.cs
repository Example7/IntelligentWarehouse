using System.Security.Claims;
using Data.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileApi.Models.Client;

namespace MobileApi.Controllers;

[ApiController]
[Route("api/client")]
[Authorize(Roles = "Client")]
public class ClientAppController : ControllerBase
{
    private readonly DataContext _context;

    public ClientAppController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("home")]
    public IActionResult GetHome()
    {
        return Ok(new
        {
            message = "API klienta dziala.",
            serverTimeUtc = DateTime.UtcNow
        });
    }

    [HttpGet("profile")]
    public async Task<ActionResult<ClientProfileDto>> GetProfile()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var profile = await _context.Klient
            .AsNoTracking()
            .Where(k => k.IdUzytkownika == userId)
            .Select(k => new ClientProfileDto
            {
                CustomerId = k.IdKlienta,
                Name = k.Nazwa,
                Email = k.Email,
                Phone = k.Telefon,
                Address = k.Adres,
                IsActive = k.CzyAktywny,
                CreatedAtUtc = k.UtworzonoUtc
            })
            .FirstOrDefaultAsync();

        if (profile is null)
        {
            return NotFound(new
            {
                message = "Brak powiazanego klienta dla zalogowanego uzytkownika.",
                userId
            });
        }

        return Ok(profile);
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ClientDashboardDto>> GetDashboard()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var customerId = await GetCustomerIdForUserAsync(userId);
        if (customerId is null)
        {
            return NotFound(new
            {
                message = "Brak powiazanego klienta dla zalogowanego uzytkownika.",
                userId
            });
        }

        var ordersQuery = _context.DokumentWZ
            .AsNoTracking()
            .Where(d => d.IdKlienta == customerId);

        var reservationsQuery = _context.Rezerwacja
            .AsNoTracking()
            .Where(r => r.IdUtworzyl == userId);

        var activeOrdersCount = await ordersQuery.CountAsync(d => d.Status != "Posted");
        var postedOrdersCount = await ordersQuery.CountAsync(d => d.Status == "Posted");
        var openReservationsCount = await reservationsQuery.CountAsync(r => r.Status != "Closed" && r.Status != "Cancelled");

        var recentOrders = await ordersQuery
            .OrderByDescending(d => d.DataWydaniaUtc)
            .ThenByDescending(d => d.Id)
            .Take(5)
            .Select(d => new ClientOrderListItemDto
            {
                OrderId = d.Id,
                Number = d.Numer,
                Status = d.Status,
                IssuedAtUtc = d.DataWydaniaUtc,
                PostedAtUtc = d.ZaksiegowanoUtc,
                WarehouseName = d.Magazyn.Nazwa,
                ItemsCount = d.Pozycje.Count,
                TotalQuantity = d.Pozycje.Sum(p => p.Ilosc)
            })
            .ToListAsync();

        var recentReservations = await reservationsQuery
            .OrderByDescending(r => r.UtworzonoUtc)
            .ThenByDescending(r => r.Id)
            .Take(5)
            .Select(r => new ClientReservationListItemDto
            {
                ReservationId = r.Id,
                Number = r.Numer,
                Status = r.Status,
                CreatedAtUtc = r.UtworzonoUtc,
                ExpiresAtUtc = r.WygasaUtc,
                WarehouseName = r.Magazyn.Nazwa,
                ItemsCount = r.Pozycje.Count,
                TotalQuantity = r.Pozycje.Sum(p => p.Ilosc)
            })
            .ToListAsync();

        return Ok(new ClientDashboardDto
        {
            ActiveOrdersCount = activeOrdersCount,
            PostedOrdersCount = postedOrdersCount,
            OpenReservationsCount = openReservationsCount,
            RecentOrders = recentOrders,
            RecentReservations = recentReservations
        });
    }

    [HttpGet("orders")]
    public async Task<ActionResult<List<ClientOrderListItemDto>>> GetOrders()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var customerId = await GetCustomerIdForUserAsync(userId);
        if (customerId is null)
        {
            return NotFound(new
            {
                message = "Brak powiazanego klienta dla zalogowanego uzytkownika.",
                userId
            });
        }

        var orders = await _context.DokumentWZ
            .AsNoTracking()
            .Where(d => d.IdKlienta == customerId)
            .OrderByDescending(d => d.DataWydaniaUtc)
            .ThenByDescending(d => d.Id)
            .Select(d => new ClientOrderListItemDto
            {
                OrderId = d.Id,
                Number = d.Numer,
                Status = d.Status,
                IssuedAtUtc = d.DataWydaniaUtc,
                PostedAtUtc = d.ZaksiegowanoUtc,
                WarehouseName = d.Magazyn.Nazwa,
                ItemsCount = d.Pozycje.Count,
                TotalQuantity = d.Pozycje.Sum(p => p.Ilosc)
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("orders/{orderId:int}")]
    public async Task<ActionResult<ClientOrderDetailsDto>> GetOrderDetails(int orderId)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var customerId = await GetCustomerIdForUserAsync(userId);
        if (customerId is null)
        {
            return NotFound(new
            {
                message = "Brak powiazanego klienta dla zalogowanego uzytkownika.",
                userId
            });
        }

        var order = await _context.DokumentWZ
            .AsNoTracking()
            .Where(d => d.Id == orderId && d.IdKlienta == customerId)
            .Select(d => new ClientOrderDetailsDto
            {
                OrderId = d.Id,
                Number = d.Numer,
                Status = d.Status,
                IssuedAtUtc = d.DataWydaniaUtc,
                PostedAtUtc = d.ZaksiegowanoUtc,
                WarehouseName = d.Magazyn.Nazwa,
                Note = d.Notatka,
                TotalQuantity = d.Pozycje.Sum(p => p.Ilosc),
                Items = d.Pozycje
                    .OrderBy(p => p.Lp)
                    .Select(p => new ClientOrderItemDto
                    {
                        ItemId = p.Id,
                        LineNo = p.Lp,
                        ProductId = p.IdProduktu,
                        ProductCode = p.Produkt.Kod,
                        ProductName = p.Produkt.Nazwa,
                        Quantity = p.Ilosc,
                        LocationId = p.IdLokacji,
                        LocationCode = p.Lokacja != null ? p.Lokacja.Kod : null
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (order is null)
        {
            return NotFound(new
            {
                message = "Nie znaleziono zamowienia klienta.",
                orderId
            });
        }

        return Ok(order);
    }

    [HttpGet("reservations")]
    public async Task<ActionResult<List<ClientReservationListItemDto>>> GetReservations()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var reservations = await _context.Rezerwacja
            .AsNoTracking()
            .Where(r => r.IdUtworzyl == userId)
            .OrderByDescending(r => r.UtworzonoUtc)
            .ThenByDescending(r => r.Id)
            .Select(r => new ClientReservationListItemDto
            {
                ReservationId = r.Id,
                Number = r.Numer,
                Status = r.Status,
                CreatedAtUtc = r.UtworzonoUtc,
                ExpiresAtUtc = r.WygasaUtc,
                WarehouseName = r.Magazyn.Nazwa,
                ItemsCount = r.Pozycje.Count,
                TotalQuantity = r.Pozycje.Sum(p => p.Ilosc)
            })
            .ToListAsync();

        return Ok(reservations);
    }

    [HttpGet("reservations/{reservationId:int}")]
    public async Task<ActionResult<ClientReservationDetailsDto>> GetReservationDetails(int reservationId)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var reservation = await _context.Rezerwacja
            .AsNoTracking()
            .Where(r => r.Id == reservationId && r.IdUtworzyl == userId)
            .Select(r => new ClientReservationDetailsDto
            {
                ReservationId = r.Id,
                Number = r.Numer,
                Status = r.Status,
                CreatedAtUtc = r.UtworzonoUtc,
                ExpiresAtUtc = r.WygasaUtc,
                WarehouseName = r.Magazyn.Nazwa,
                Note = r.Notatka,
                TotalQuantity = r.Pozycje.Sum(p => p.Ilosc),
                Items = r.Pozycje
                    .OrderBy(p => p.Lp)
                    .Select(p => new ClientReservationItemDto
                    {
                        ItemId = p.Id,
                        LineNo = p.Lp,
                        ProductId = p.IdProduktu,
                        ProductCode = p.Produkt.Kod,
                        ProductName = p.Produkt.Nazwa,
                        Quantity = p.Ilosc,
                        LocationId = p.IdLokacji,
                        LocationCode = p.Lokacja != null ? p.Lokacja.Kod : null
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (reservation is null)
        {
            return NotFound(new
            {
                message = "Nie znaleziono rezerwacji klienta.",
                reservationId
            });
        }

        return Ok(reservation);
    }

    [HttpGet("notifications")]
    public async Task<ActionResult<List<ClientNotificationDto>>> GetNotifications([FromQuery] int take = 20)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        take = Math.Clamp(take, 1, 100);

        var customerId = await GetCustomerIdForUserAsync(userId);
        if (customerId is null)
        {
            return NotFound(new
            {
                message = "Brak powiazanego klienta dla zalogowanego uzytkownika.",
                userId
            });
        }

        var customerProductIds = _context.DokumentWZ
            .Where(d => d.IdKlienta == customerId)
            .SelectMany(d => d.Pozycje.Select(p => p.IdProduktu))
            .Distinct();

        var notifications = await _context.Alert
            .AsNoTracking()
            .Where(a => customerProductIds.Contains(a.IdProduktu))
            .OrderByDescending(a => a.UtworzonoUtc)
            .ThenByDescending(a => a.Id)
            .Take(take)
            .Select(a => new ClientNotificationDto
            {
                NotificationId = a.Id,
                Severity = a.Waga,
                Message = a.Tresc,
                CreatedAtUtc = a.UtworzonoUtc,
                IsAcknowledged = a.CzyPotwierdzony,
                ProductCode = a.Produkt.Kod,
                ProductName = a.Produkt.Nazwa,
                WarehouseName = a.Magazyn.Nazwa
            })
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpGet("orders/{orderId:int}/attachments")]
    public async Task<ActionResult<List<ClientAttachmentDto>>> GetOrderAttachments(int orderId)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var customerId = await GetCustomerIdForUserAsync(userId);
        if (customerId is null)
        {
            return NotFound(new
            {
                message = "Brak powiazanego klienta dla zalogowanego uzytkownika.",
                userId
            });
        }

        var orderOwned = await _context.DokumentWZ
            .AsNoTracking()
            .AnyAsync(d => d.Id == orderId && d.IdKlienta == customerId);

        if (!orderOwned)
        {
            return NotFound(new
            {
                message = "Nie znaleziono zamowienia klienta.",
                orderId
            });
        }

        var attachments = await _context.ZalacznikDokumentu
            .AsNoTracking()
            .Where(a => a.IdDokumentu == orderId && a.TypDokumentu == "WZ")
            .OrderByDescending(a => a.WgranoUtc)
            .ThenByDescending(a => a.Id)
            .Select(a => new ClientAttachmentDto
            {
                AttachmentId = a.Id,
                DocumentType = a.TypDokumentu,
                DocumentId = a.IdDokumentu,
                FileName = a.NazwaPliku,
                ContentType = a.ContentType,
                FilePath = a.Sciezka,
                UploadedAtUtc = a.WgranoUtc
            })
            .ToListAsync();

        return Ok(attachments);
    }

    [HttpGet("attachments/{attachmentId:long}/download")]
    public async Task<IActionResult> DownloadAttachment(long attachmentId)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var customerId = await GetCustomerIdForUserAsync(userId);
        if (customerId is null)
        {
            return NotFound(new
            {
                message = "Brak powiazanego klienta dla zalogowanego uzytkownika.",
                userId
            });
        }

        var attachmentInfo = await (
            from a in _context.ZalacznikDokumentu.AsNoTracking()
            join d in _context.DokumentWZ.AsNoTracking() on a.IdDokumentu equals d.Id
            where a.Id == attachmentId
                  && a.TypDokumentu == "WZ"
                  && d.IdKlienta == customerId
            select new
            {
                a.Id,
                a.NazwaPliku,
                a.ContentType,
                a.Sciezka
            })
            .FirstOrDefaultAsync();

        if (attachmentInfo is null)
        {
            return NotFound(new
            {
                message = "Nie znaleziono zalacznika klienta.",
                attachmentId
            });
        }

        var path = attachmentInfo.Sciezka;
        if (!System.IO.Path.IsPathRooted(path))
        {
            path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Directory.GetCurrentDirectory(), path));
        }

        if (!System.IO.File.Exists(path))
        {
            return NotFound(new
            {
                message = "Plik zalacznika nie istnieje na dysku.",
                attachmentId,
                path
            });
        }

        var contentType = string.IsNullOrWhiteSpace(attachmentInfo.ContentType)
            ? "application/octet-stream"
            : attachmentInfo.ContentType;

        return PhysicalFile(path, contentType, attachmentInfo.NazwaPliku);
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out userId);
    }

    private async Task<int?> GetCustomerIdForUserAsync(int userId)
    {
        return await _context.Klient
            .AsNoTracking()
            .Where(k => k.IdUzytkownika == userId)
            .Select(k => (int?)k.IdKlienta)
            .FirstOrDefaultAsync();
    }
}
