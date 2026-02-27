using Data.Data;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;
using System.Globalization;
using System.Text.Json;

namespace Services.Magazyn
{
    public class LogAudytuService : BaseService, ILogAudytuService
    {
        private static readonly Dictionary<string, string> FieldLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["IdProduktu"] = "Produkt",
            ["ProductId"] = "Produkt",
            ["IdLokacji"] = "Lokacja",
            ["LocationId"] = "Lokacja",
            ["IdMagazynu"] = "Magazyn",
            ["WarehouseId"] = "Magazyn",
            ["IdKategorii"] = "Kategoria",
            ["CategoryId"] = "Kategoria",
            ["IdDostawcy"] = "Dostawca",
            ["SupplierId"] = "Dostawca",
            ["IdKlienta"] = "Klient",
            ["CustomerId"] = "Klient",
            ["IdUzytkownika"] = "Użytkownik",
            ["UserId"] = "Użytkownik",
            ["IdRoli"] = "Rola",
            ["RoleId"] = "Rola",
            ["Ilosc"] = "Ilość",
            ["Quantity"] = "Ilość",
            ["CzyAktywna"] = "Aktywna",
            ["CzyAktywny"] = "Aktywny",
            ["IsActive"] = "Aktywny",
            ["IdUtworzyl"] = "Utworzył",
            ["CreatedByUserId"] = "Utworzył",
            ["UtworzonoUtc"] = "Utworzono",
            ["CreatedAt"] = "Utworzono",
            ["WygasaUtc"] = "Wygasa",
            ["ExpiresAt"] = "Wygasa",
            ["ZaksiegowanoUtc"] = "Zaksięgowano",
            ["PostedAt"] = "Zaksięgowano",
            ["DataWydaniaUtc"] = "Data wydania",
            ["IssuedAt"] = "Data wydania",
            ["DataPrzyjeciaUtc"] = "Data przyjęcia",
            ["ReceivedAt"] = "Data przyjęcia",
            ["DataUtc"] = "Data dokumentu",
            ["Numer"] = "Numer",
            ["DocumentNo"] = "Numer",
            ["Notatka"] = "Notatka",
            ["Note"] = "Notatka",
            ["Status"] = "Status",
            ["IdLokacjiZ"] = "Lokacja źródłowa",
            ["IdLokacjiDo"] = "Lokacja docelowa",
            ["IdRuchu"] = "ID ruchu",
            ["Referencja"] = "Referencja",
            ["Typ"] = "Typ ruchu"
        };

        public LogAudytuService(DataContext context) : base(context)
        {
        }

        public async Task<LogAudytuIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.LogAudytu
                .AsNoTracking()
                .Include(l => l.Uzytkownik)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(l =>
                    EF.Functions.Like(l.Akcja, $"%{term}%") ||
                    EF.Functions.Like(l.Encja, $"%{term}%") ||
                    (l.IdEncji != null && EF.Functions.Like(l.IdEncji, $"%{term}%")) ||
                    (l.Uzytkownik != null && EF.Functions.Like(l.Uzytkownik.Email, $"%{term}%")));
            }

            var logs = await query
                .OrderByDescending(l => l.KiedyUtc)
                .ThenByDescending(l => l.Id)
                .ToListAsync();

            return new LogAudytuIndexDto
            {
                SearchTerm = searchTerm,
                Items = logs.Select(l => new LogAudytuIndexItemDto { Log = l }).ToList()
            };
        }

        public async Task<LogAudytuDetailsDto?> GetDetailsDataAsync(long idLogu)
        {
            var log = await _context.LogAudytu
                .AsNoTracking()
                .Include(l => l.Uzytkownik)
                .FirstOrDefaultAsync(l => l.Id == idLogu);

            if (log == null)
            {
                return null;
            }

            var changes = await BuildChangeRowsAsync(log);

            return new LogAudytuDetailsDto
            {
                Log = log,
                Changes = changes
            };
        }

        public async Task<LogAudytuDeleteDto?> GetDeleteDataAsync(long idLogu)
        {
            var details = await GetDetailsDataAsync(idLogu);
            if (details == null)
            {
                return null;
            }

            return new LogAudytuDeleteDto { Log = details.Log };
        }

        private async Task<IList<LogAudytuChangeDto>> BuildChangeRowsAsync(Data.Data.Magazyn.LogAudytu log)
        {
            var oldMap = ParseFlatJson(log.StareJson);
            var newMap = ParseFlatJson(log.NoweJson);
            var primaryKeyFields = ParseEntityKeyFieldNames(log.IdEncji);

            var keys = oldMap.Keys
                .Union(newMap.Keys, StringComparer.OrdinalIgnoreCase)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            var displayMap = await BuildDisplayValueMapAsync(oldMap, newMap);
            var rows = new List<LogAudytuChangeDto>();

            foreach (var key in keys)
            {
                if (ShouldHideFieldInChanges(key, log.Akcja, primaryKeyFields))
                {
                    continue;
                }

                oldMap.TryGetValue(key, out var oldValue);
                newMap.TryGetValue(key, out var newValue);

                if (string.Equals(oldValue, newValue, StringComparison.Ordinal))
                {
                    continue;
                }

                rows.Add(new LogAudytuChangeDto
                {
                    FieldKey = key,
                    FieldLabel = ResolveFieldLabel(key),
                    OldValue = FormatValueForTable(oldValue),
                    NewValue = FormatValueForTable(newValue),
                    OldDisplayValue = ResolveDisplayValue(displayMap, key, oldValue),
                    NewDisplayValue = ResolveDisplayValue(displayMap, key, newValue),
                    ChangeType = ResolveChangeType(oldValue, newValue)
                });
            }

            return rows;
        }

        private async Task<Dictionary<string, Dictionary<int, string>>> BuildDisplayValueMapAsync(
            IReadOnlyDictionary<string, string?> oldMap,
            IReadOnlyDictionary<string, string?> newMap)
        {
            var all = oldMap
                .Concat(newMap)
                .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Value).ToList(), StringComparer.OrdinalIgnoreCase);

            var map = new Dictionary<string, Dictionary<int, string>>(StringComparer.OrdinalIgnoreCase);

            var productIds = ExtractIntIds(all, "IdProduktu", "ProductId");
            if (productIds.Count > 0)
            {
                map["IdProduktu"] = await _context.Produkt
                    .AsNoTracking()
                    .Where(x => productIds.Contains(x.IdProduktu))
                    .ToDictionaryAsync(x => x.IdProduktu, x => $"{x.Kod} - {x.Nazwa}");
            }

            var locationIds = ExtractIntIds(all, "IdLokacji", "LocationId");
            if (locationIds.Count > 0)
            {
                map["IdLokacji"] = await _context.Lokacja
                    .AsNoTracking()
                    .Include(x => x.Magazyn)
                    .Where(x => locationIds.Contains(x.IdLokacji))
                    .ToDictionaryAsync(x => x.IdLokacji, x => $"{x.Kod} ({x.Magazyn.Nazwa})");
            }

            var warehouseIds = ExtractIntIds(all, "IdMagazynu", "WarehouseId");
            if (warehouseIds.Count > 0)
            {
                map["IdMagazynu"] = await _context.Magazyn
                    .AsNoTracking()
                    .Where(x => warehouseIds.Contains(x.IdMagazynu))
                    .ToDictionaryAsync(x => x.IdMagazynu, x => x.Nazwa);
            }

            var userIds = ExtractIntIds(all, "IdUzytkownika", "UserId", "IdUtworzyl", "CreatedByUserId");
            if (userIds.Count > 0)
            {
                map["IdUzytkownika"] = await _context.Uzytkownik
                    .AsNoTracking()
                    .Where(x => userIds.Contains(x.IdUzytkownika))
                    .ToDictionaryAsync(x => x.IdUzytkownika, x => $"{x.Login} ({x.Email})");
            }

            var categoryIds = ExtractIntIds(all, "IdKategorii", "CategoryId");
            if (categoryIds.Count > 0)
            {
                map["IdKategorii"] = await _context.Kategoria
                    .AsNoTracking()
                    .Where(x => categoryIds.Contains(x.IdKategorii))
                    .ToDictionaryAsync(x => x.IdKategorii, x => x.Nazwa);
            }

            var supplierIds = ExtractIntIds(all, "IdDostawcy", "SupplierId");
            if (supplierIds.Count > 0)
            {
                map["IdDostawcy"] = await _context.Dostawca
                    .AsNoTracking()
                    .Where(x => supplierIds.Contains(x.IdDostawcy))
                    .ToDictionaryAsync(x => x.IdDostawcy, x => x.Nazwa);
            }

            var customerIds = ExtractIntIds(all, "IdKlienta", "CustomerId");
            if (customerIds.Count > 0)
            {
                map["IdKlienta"] = await _context.Klient
                    .AsNoTracking()
                    .Where(x => customerIds.Contains(x.IdKlienta))
                    .ToDictionaryAsync(x => x.IdKlienta, x => x.Nazwa);
            }

            var roleIds = ExtractIntIds(all, "IdRoli", "RoleId");
            if (roleIds.Count > 0)
            {
                map["IdRoli"] = await _context.Rola
                    .AsNoTracking()
                    .Where(x => roleIds.Contains(x.IdRoli))
                    .ToDictionaryAsync(x => x.IdRoli, x => x.Nazwa);
            }

            return map;
        }

        private static HashSet<int> ExtractIntIds(Dictionary<string, List<string?>> all, params string[] keys)
        {
            var values = new HashSet<int>();
            foreach (var key in keys)
            {
                if (!all.TryGetValue(key, out var list))
                {
                    continue;
                }

                foreach (var value in list)
                {
                    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                    {
                        values.Add(id);
                    }
                }
            }

            return values;
        }

        private static string ResolveFieldLabel(string key)
        {
            return FieldLabels.TryGetValue(key, out var label) ? label : key;
        }

        private static bool ShouldHideFieldInChanges(string fieldKey, string? action, ISet<string> primaryKeyFields)
        {
            if (primaryKeyFields.Count == 0)
            {
                return false;
            }

            if (!string.Equals(action, "CREATE", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(action, "DELETE", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return primaryKeyFields.Contains(fieldKey);
        }

        private static HashSet<string> ParseEntityKeyFieldNames(string? entityId)
        {
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(entityId))
            {
                return keys;
            }

            var parts = entityId.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in parts)
            {
                var separatorIndex = part.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                var field = part[..separatorIndex].Trim();
                if (field.Length > 0)
                {
                    keys.Add(field);
                }
            }

            return keys;
        }

        private static string? FormatValueForTable(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (bool.TryParse(value, out var boolValue))
            {
                return boolValue ? "Tak" : "Nie";
            }

            if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
            {
                return dto.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            }

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
            {
                var local = dt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToLocalTime() : dt.ToLocalTime();
                return local.ToString("yyyy-MM-dd HH:mm:ss");
            }

            if (value.Equals("Draft", StringComparison.OrdinalIgnoreCase))
            {
                return "Wstępny";
            }

            if (value.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                return "Aktywna";
            }

            if (value.Equals("Posted", StringComparison.OrdinalIgnoreCase))
            {
                return "Zatwierdzony";
            }

            if (value.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                return "Anulowany";
            }

            return value;
        }

        private static string? ResolveDisplayValue(
            IReadOnlyDictionary<string, Dictionary<int, string>> displayMap,
            string fieldKey,
            string? rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return null;
            }

            if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
            {
                return null;
            }

            if (displayMap.TryGetValue(fieldKey, out var byId) && byId.TryGetValue(id, out var label))
            {
                return label;
            }

            // Support aliases (e.g. ProductId and IdProduktu).
            if (fieldKey.Equals("ProductId", StringComparison.OrdinalIgnoreCase) &&
                displayMap.TryGetValue("IdProduktu", out byId) &&
                byId.TryGetValue(id, out label))
            {
                return label;
            }

            if (fieldKey.Equals("LocationId", StringComparison.OrdinalIgnoreCase) &&
                displayMap.TryGetValue("IdLokacji", out byId) &&
                byId.TryGetValue(id, out label))
            {
                return label;
            }

            if (fieldKey.Equals("WarehouseId", StringComparison.OrdinalIgnoreCase) &&
                displayMap.TryGetValue("IdMagazynu", out byId) &&
                byId.TryGetValue(id, out label))
            {
                return label;
            }

            if (fieldKey.Equals("UserId", StringComparison.OrdinalIgnoreCase) &&
                displayMap.TryGetValue("IdUzytkownika", out byId) &&
                byId.TryGetValue(id, out label))
            {
                return label;
            }

            if (fieldKey.Equals("IdUtworzyl", StringComparison.OrdinalIgnoreCase) &&
                displayMap.TryGetValue("IdUzytkownika", out byId) &&
                byId.TryGetValue(id, out label))
            {
                return label;
            }

            if (fieldKey.Equals("CreatedByUserId", StringComparison.OrdinalIgnoreCase) &&
                displayMap.TryGetValue("IdUzytkownika", out byId) &&
                byId.TryGetValue(id, out label))
            {
                return label;
            }

            if (fieldKey.Equals("CategoryId", StringComparison.OrdinalIgnoreCase) &&
                displayMap.TryGetValue("IdKategorii", out byId) &&
                byId.TryGetValue(id, out label))
            {
                return label;
            }

            if (fieldKey.Equals("SupplierId", StringComparison.OrdinalIgnoreCase) &&
                displayMap.TryGetValue("IdDostawcy", out byId) &&
                byId.TryGetValue(id, out label))
            {
                return label;
            }

            if (fieldKey.Equals("CustomerId", StringComparison.OrdinalIgnoreCase) &&
                displayMap.TryGetValue("IdKlienta", out byId) &&
                byId.TryGetValue(id, out label))
            {
                return label;
            }

            if (fieldKey.Equals("RoleId", StringComparison.OrdinalIgnoreCase) &&
                displayMap.TryGetValue("IdRoli", out byId) &&
                byId.TryGetValue(id, out label))
            {
                return label;
            }

            return null;
        }

        private static string ResolveChangeType(string? oldValue, string? newValue)
        {
            var hasOld = !string.IsNullOrWhiteSpace(oldValue) && oldValue != "-";
            var hasNew = !string.IsNullOrWhiteSpace(newValue) && newValue != "-";

            if (!hasOld && hasNew)
            {
                return "Dodane";
            }

            if (hasOld && !hasNew)
            {
                return "Usunięte";
            }

            return "Zmienione";
        }

        private static Dictionary<string, string?> ParseFlatJson(string? json)
        {
            var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(json))
            {
                return result;
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return result;
                }

                foreach (var property in doc.RootElement.EnumerateObject())
                {
                    result[property.Name] = JsonElementToString(property.Value);
                }
            }
            catch
            {
                return result;
            }

            return result;
        }

        private static string? JsonElementToString(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => element.GetRawText()
            };
        }
    }
}