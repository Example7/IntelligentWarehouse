using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileApi.Models.Client;

namespace MobileApi.Controllers;

[ApiController]
[Route("api/client")]
[Authorize(Roles = "Client,Klient")]
public class ClientAppController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IRezerwacjaService _rezerwacjaService;

    public ClientAppController(DataContext context, IRezerwacjaService rezerwacjaService)
    {
        _context = context;
        _rezerwacjaService = rezerwacjaService;
    }

    [HttpGet("home")]
    public IActionResult GetHome()
    {
        return Ok(new
        {
            message = "API klienta działa.",
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
                message = "Brak powiązanego klienta dla zalogowanego użytkownika.",
                userId
            });
        }

        return Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<ActionResult<ClientProfileDto>> UpdateProfile([FromBody] UpdateClientProfileRequestDto request)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var klient = await _context.Klient
            .Include(k => k.Uzytkownik)
            .FirstOrDefaultAsync(k => k.IdUzytkownika == userId);

        if (klient is null)
        {
            return NotFound(new
            {
                message = "Brak powiazanego klienta dla zalogowanego uzytkownika.",
                userId
            });
        }

        var name = (request.Name ?? string.Empty).Trim();
        var email = (request.Email ?? string.Empty).Trim();
        var phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        var address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { message = "Nazwa jest wymagana." });
        }

        if (name.Length > 250)
        {
            return BadRequest(new { message = "Nazwa może mieć maksymalnie 250 znakow." });
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { message = "Email jest wymagany." });
        }

        if (email.Length > 120)
        {
            return BadRequest(new { message = "Email może mieć maksymalnie 120 znakow." });
        }

        var emailValidator = new EmailAddressAttribute();
        if (!emailValidator.IsValid(email))
        {
            return BadRequest(new { message = "Podaj poprawny adres e-mail." });
        }

        if (phone is { Length: > 60 })
        {
            return BadRequest(new { message = "Telefon może mieć maksymalnie 60 znakow." });
        }

        if (address is { Length: > 400 })
        {
            return BadRequest(new { message = "Adres może mieć maksymalnie 400 znakow." });
        }

        if (klient.IdUzytkownika.HasValue)
        {
            var existingUserWithEmail = await _context.Uzytkownik
                .AsNoTracking()
                .AnyAsync(u => u.IdUzytkownika != klient.IdUzytkownika.Value && u.Email == email);

            if (existingUserWithEmail)
            {
                return Conflict(new { message = $"Email '{email}' jest juz zajety." });
            }
        }

        klient.Nazwa = name;
        klient.Email = email;
        klient.Telefon = phone;
        klient.Adres = address;

        if (klient.Uzytkownik is not null)
        {
            klient.Uzytkownik.Email = email;
        }

        await _context.SaveChangesAsync();

        return Ok(new ClientProfileDto
        {
            CustomerId = klient.IdKlienta,
            Name = klient.Nazwa,
            Email = klient.Email,
            Phone = klient.Telefon,
            Address = klient.Adres,
            IsActive = klient.CzyAktywny,
            CreatedAtUtc = klient.UtworzonoUtc
        });
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
                message = "Brak powiązanego klienta dla zalogowanego użytkownika.",
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
                message = "Brak powiązanego klienta dla zalogowanego użytkownika.",
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
                message = "Brak powiązanego klienta dla zalogowanego użytkownika.",
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
                message = "Nie znaleziono zamówienia klienta.",
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

    [HttpGet("lookups/warehouses")]
    public async Task<ActionResult<List<ClientWarehouseLookupDto>>> GetWarehouseLookups()
    {
        var warehouses = await _context.Magazyn
            .AsNoTracking()
            .Where(w => w.CzyAktywny)
            .OrderBy(w => w.Nazwa)
            .Select(w => new ClientWarehouseLookupDto
            {
                WarehouseId = w.IdMagazynu,
                Name = w.Nazwa
            })
            .ToListAsync();

        return Ok(warehouses);
    }

    [HttpGet("lookups/products")]
    public async Task<ActionResult<List<ClientProductLookupDto>>> GetReservableProducts([FromQuery] string? q, [FromQuery] int? warehouseId, [FromQuery] int take = 20)
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
                message = "Brak powiązanego klienta dla zalogowanego użytkownika.",
                userId
            });
        }

        take = Math.Clamp(take, 5, 50);
        var term = q?.Trim();

        // Prefer products already seen on the customer's WZ documents; fallback to active catalog search.
        var customerProductsQuery = _context.DokumentWZ
            .AsNoTracking()
            .Where(d => d.IdKlienta == customerId)
            .SelectMany(d => d.Pozycje.Select(p => p.Produkt))
            .Where(p => p.CzyAktywny)
            .Distinct();

        if (!string.IsNullOrWhiteSpace(term))
        {
            customerProductsQuery = customerProductsQuery.Where(p =>
                EF.Functions.Like(p.Kod, $"%{term}%") ||
                EF.Functions.Like(p.Nazwa, $"%{term}%"));
        }

        var customerProducts = await customerProductsQuery
            .OrderBy(p => p.Nazwa)
            .ThenBy(p => p.Kod)
            .Take(take)
            .Select(p => new ClientProductLookupDto
            {
                ProductId = p.IdProduktu,
                Code = p.Kod,
                Name = p.Nazwa,
                DefaultUom = p.DomyslnaJednostka.Nazwa,
                AvailableQuantity = warehouseId == null
                    ? null
                    : (
                        (_context.StanMagazynowy
                            .Where(s => s.IdProduktu == p.IdProduktu && s.Lokacja.IdMagazynu == warehouseId.Value)
                            .Select(s => (decimal?)s.Ilosc)
                            .Sum() ?? 0m)
                        -
                        (_context.PozycjaRezerwacji
                            .Where(pr =>
                                pr.IdProduktu == p.IdProduktu &&
                                pr.Rezerwacja.IdMagazynu == warehouseId.Value &&
                                pr.Rezerwacja.Status == "Active")
                            .Select(pr => (decimal?)pr.Ilosc)
                            .Sum() ?? 0m)
                        -
                        (_context.PozycjaWZ
                            .Where(pw =>
                                pw.IdProduktu == p.IdProduktu &&
                                pw.Dokument.IdMagazynu == warehouseId.Value &&
                                pw.Dokument.Status == "Draft")
                            .Select(pw => (decimal?)pw.Ilosc)
                            .Sum() ?? 0m)
                    )
            })
            .ToListAsync();

        var fallbackQuery = _context.Produkt
            .AsNoTracking()
            .Where(p => p.CzyAktywny);

        if (!string.IsNullOrWhiteSpace(term))
        {
            fallbackQuery = fallbackQuery.Where(p =>
                EF.Functions.Like(p.Kod, $"%{term}%") ||
                EF.Functions.Like(p.Nazwa, $"%{term}%"));
        }

        var fallback = await fallbackQuery
            .OrderBy(p => p.Nazwa)
            .ThenBy(p => p.Kod)
            .Take(take)
            .Select(p => new ClientProductLookupDto
            {
                ProductId = p.IdProduktu,
                Code = p.Kod,
                Name = p.Nazwa,
                DefaultUom = p.DomyslnaJednostka.Nazwa,
                AvailableQuantity = warehouseId == null
                    ? null
                    : (
                        (_context.StanMagazynowy
                            .Where(s => s.IdProduktu == p.IdProduktu && s.Lokacja.IdMagazynu == warehouseId.Value)
                            .Select(s => (decimal?)s.Ilosc)
                            .Sum() ?? 0m)
                        -
                        (_context.PozycjaRezerwacji
                            .Where(pr =>
                                pr.IdProduktu == p.IdProduktu &&
                                pr.Rezerwacja.IdMagazynu == warehouseId.Value &&
                                pr.Rezerwacja.Status == "Active")
                            .Select(pr => (decimal?)pr.Ilosc)
                            .Sum() ?? 0m)
                        -
                        (_context.PozycjaWZ
                            .Where(pw =>
                                pw.IdProduktu == p.IdProduktu &&
                                pw.Dokument.IdMagazynu == warehouseId.Value &&
                                pw.Dokument.Status == "Draft")
                            .Select(pw => (decimal?)pw.Ilosc)
                            .Sum() ?? 0m)
                    )
            })
            .ToListAsync();

        if (customerProducts.Count == 0)
        {
            return Ok(fallback);
        }

        var merged = customerProducts
            .Concat(fallback.Where(p => customerProducts.All(cp => cp.ProductId != p.ProductId)))
            .Take(take)
            .ToList();

        return Ok(merged);
    }

    [HttpPost("reservations")]
    public async Task<ActionResult<ClientCreateReservationResponseDto>> CreateReservation([FromBody] ClientCreateReservationRequestDto request)
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
                message = "Brak powiązanego klienta dla zalogowanego użytkownika.",
                userId
            });
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var effectiveExpiresAtUtc = request.ExpiresAtUtc ?? DateTime.UtcNow.AddHours(24);

        var result = await _rezerwacjaService.CreateClientDraftAsync(new RezerwacjaCreateClientCommandDto
        {
            UserId = userId,
            WarehouseId = request.WarehouseId,
            ExpiresAtUtc = effectiveExpiresAtUtc,
            Note = request.Note,
            Items = request.Items.Select(i => new RezerwacjaCreateClientItemCommandDto
            {
                ProductId = i.ProductId,
                LocationId = i.LocationId,
                Quantity = i.Quantity
            }).ToList()
        });

        if (!result.Success || result.ReservationId is null || result.Number is null || result.Status is null || result.CreatedAtUtc is null)
        {
            return BadRequest(new { message = result.Message ?? "Nie udało się utworzyć rezerwacji." });
        }

        var activationResult = await _rezerwacjaService.ActivateAsync(result.ReservationId.Value);
        var autoActivationSucceeded = activationResult.Success;
        var finalStatus = autoActivationSucceeded ? "Active" : result.Status;
        var autoActivationMessage = autoActivationSucceeded
            ? "Rezerwacja została automatycznie potwierdzona."
            : (string.IsNullOrWhiteSpace(activationResult.Message)
                ? "Rezerwacja została utworzona i oczekuje na potwierdzenie magazynu."
                : activationResult.Message);

        return CreatedAtAction(nameof(GetReservationDetails), new { reservationId = result.ReservationId.Value }, new ClientCreateReservationResponseDto
        {
            ReservationId = result.ReservationId.Value,
            Number = result.Number,
            Status = finalStatus!,
            CreatedAtUtc = result.CreatedAtUtc.Value,
            ExpiresAtUtc = effectiveExpiresAtUtc,
            AutoActivationAttempted = true,
            AutoActivationSucceeded = autoActivationSucceeded,
            AutoActivationMessage = autoActivationMessage
        });
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
                message = "Brak powiązanego klienta dla zalogowanego użytkownika.",
                userId
            });
        }

        var reservationNotifications = await _context.Rezerwacja
            .AsNoTracking()
            .Where(r => r.IdUtworzyl == userId)
            .OrderByDescending(r => r.UtworzonoUtc)
            .ThenByDescending(r => r.Id)
            .Take(take)
            .Select(r => new
            {
                r.Id,
                r.Numer,
                r.Status,
                r.UtworzonoUtc,
                r.WygasaUtc,
                WarehouseName = r.Magazyn.Nazwa
            })
            .ToListAsync();

        var orderNotifications = await _context.DokumentWZ
            .AsNoTracking()
            .Where(d => d.IdKlienta == customerId)
            .OrderByDescending(d => d.ZaksiegowanoUtc ?? d.DataWydaniaUtc)
            .ThenByDescending(d => d.Id)
            .Take(take)
            .Select(d => new
            {
                d.Id,
                d.Numer,
                d.Status,
                d.DataWydaniaUtc,
                d.ZaksiegowanoUtc,
                WarehouseName = d.Magazyn.Nazwa
            })
            .ToListAsync();

        var notifications = new List<ClientNotificationDto>(take * 2);

        notifications.AddRange(reservationNotifications.Select(r =>
        {
            var (severity, message) = BuildReservationNotification(r.Status, r.WygasaUtc);
            return new ClientNotificationDto
            {
                NotificationId = 1_000_000_000L + r.Id,
                Severity = severity,
                Title = $"Rezerwacja {r.Numer}",
                Message = message,
                Status = r.Status,
                DocumentType = "Reservation",
                DocumentNumber = r.Numer,
                CreatedAtUtc = r.UtworzonoUtc,
                IsAcknowledged = IsReservationStatusFinal(r.Status),
                WarehouseName = r.WarehouseName
            };
        }));

        notifications.AddRange(orderNotifications.Select(d =>
        {
            var (severity, message) = BuildOrderNotification(d.Status);
            return new ClientNotificationDto
            {
                NotificationId = 2_000_000_000L + d.Id,
                Severity = severity,
                Title = $"WZ {d.Numer}",
                Message = message,
                Status = d.Status,
                DocumentType = "WZ",
                DocumentNumber = d.Numer,
                CreatedAtUtc = d.ZaksiegowanoUtc ?? d.DataWydaniaUtc,
                IsAcknowledged = IsOrderStatusFinal(d.Status),
                WarehouseName = d.WarehouseName
            };
        }));

        return Ok(notifications
            .OrderByDescending(n => n.CreatedAtUtc)
            .ThenByDescending(n => n.NotificationId)
            .Take(take)
            .ToList());
    }

    private static (string Severity, string Message) BuildReservationNotification(string? status, DateTime? expiresAtUtc)
    {
        return (status ?? string.Empty).Trim() switch
        {
            "Active" or "Accepted" => ("GOOD", "Rezerwacja została potwierdzona przez magazyn."),
            "Released" or "ConvertedToWz" or "Completed" => ("GOOD", "Rezerwacja została przekazana do realizacji (WZ)."),
            "Rejected" => ("DANGER", "Rezerwacja została odrzucona. Sprawdź szczegóły lub skontaktuj się z obsługą."),
            "Cancelled" => ("WARN", "Rezerwacja została anulowana."),
            "Expired" => ("WARN", "Rezerwacja wygasła przed potwierdzeniem."),
            "Draft" => ("WARN", expiresAtUtc.HasValue
                ? $"Rezerwacja oczekuje na potwierdzenie magazynu (ważna do {expiresAtUtc.Value:yyyy-MM-dd HH:mm})."
                : "Rezerwacja oczekuje na potwierdzenie magazynu."),
            _ => ("INFO", "Aktualizacja statusu rezerwacji.")
        };
    }

    private static (string Severity, string Message) BuildOrderNotification(string? status)
    {
        return (status ?? string.Empty).Trim() switch
        {
            "Posted" => ("GOOD", "Dokument WZ został zaksięgowany."),
            "Issued" => ("GOOD", "Towar został wydany."),
            "Draft" => ("INFO", "Dokument WZ został utworzony i oczekuje na finalizację."),
            "Cancelled" => ("WARN", "Dokument WZ został anulowany."),
            _ => ("INFO", "Aktualizacja dokumentu WZ.")
        };
    }

    private static bool IsReservationStatusFinal(string? status) =>
        (status ?? string.Empty).Trim() is "Released" or "ConvertedToWz" or "Completed" or "Rejected" or "Cancelled" or "Expired";

    private static bool IsOrderStatusFinal(string? status) =>
        (status ?? string.Empty).Trim() is "Posted" or "Cancelled";

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
                message = "Brak powiązanego klienta dla zalogowanego użytkownika.",
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
                message = "Nie znaleziono zamówienia klienta.",
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
                message = "Brak powiązanego klienta dla zalogowanego użytkownika.",
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
                message = "Nie znaleziono załącznika klienta.",
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
                message = "Plik załącznika nie istnieje na dysku.",
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
