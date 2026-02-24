using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class RuchMagazynowyService : BaseService, IRuchMagazynowyService
    {
        public RuchMagazynowyService(DataContext context) : base(context)
        {
        }

        public async Task<RuchMagazynowyIndexDto> GetIndexDataAsync(string? searchTerm)
        {
            var query = _context.RuchMagazynowy
                .Include(r => r.LokacjaDo)
                    .ThenInclude(l => l!.Magazyn)
                .Include(r => r.LokacjaZ)
                    .ThenInclude(l => l!.Magazyn)
                .Include(r => r.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .Include(r => r.Uzytkownik)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();

                if (DateTime.TryParse(term, out var parsedDate))
                {
                    var date = parsedDate.Date;
                    var nextDate = date.AddDays(1);
                    query = query.Where(r => r.UtworzonoUtc >= date && r.UtworzonoUtc < nextDate);
                }
                else
                {
                    query = query.Where(x =>
                        (x.Referencja != null && EF.Functions.Like(x.Referencja, $"%{term}%")) ||
                        (x.Notatka != null && EF.Functions.Like(x.Notatka, $"%{term}%")) ||
                        EF.Functions.Like(x.Produkt.Kod, $"%{term}%") ||
                        EF.Functions.Like(x.Produkt.Nazwa, $"%{term}%") ||
                        (x.LokacjaZ != null && EF.Functions.Like(x.LokacjaZ.Kod, $"%{term}%")) ||
                        (x.LokacjaDo != null && EF.Functions.Like(x.LokacjaDo.Kod, $"%{term}%")) ||
                        (x.LokacjaZ != null && x.LokacjaZ.Magazyn != null && EF.Functions.Like(x.LokacjaZ.Magazyn.Nazwa, $"%{term}%")) ||
                        (x.LokacjaDo != null && x.LokacjaDo.Magazyn != null && EF.Functions.Like(x.LokacjaDo.Magazyn.Nazwa, $"%{term}%")) ||
                        (x.Uzytkownik != null && x.Uzytkownik.Email != null && EF.Functions.Like(x.Uzytkownik.Email, $"%{term}%")));
                }
            }

            var items = await query
                .OrderByDescending(x => x.UtworzonoUtc)
                .ThenByDescending(x => x.IdRuchu)
                .ToListAsync();

            return new RuchMagazynowyIndexDto
            {
                SearchTerm = searchTerm,
                Items = items
            };
        }

        public async Task<RuchMagazynowyCreateResultDto> CreateAndApplyAsync(RuchMagazynowy ruchMagazynowy)
        {
            var errors = new List<RuchMagazynowyCreateErrorDto>();
            await ValidateMovementAsync(ruchMagazynowy, errors);
            if (errors.Count > 0)
            {
                return new RuchMagazynowyCreateResultDto { Success = false, Errors = errors };
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var applied = await ApplyMovementToStockAsync(ruchMagazynowy, errors);
                if (!applied)
                {
                    await transaction.RollbackAsync();
                    return new RuchMagazynowyCreateResultDto { Success = false, Errors = errors };
                }

                ruchMagazynowy.UtworzonoUtc = DateTime.UtcNow;
                _context.RuchMagazynowy.Add(ruchMagazynowy);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new RuchMagazynowyCreateResultDto { Success = true };
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                AddError(errors, string.Empty, "Nie udało sie zapisać ruchu i zaktualizować stanów magazynowych.");
                return new RuchMagazynowyCreateResultDto { Success = false, Errors = errors };
            }
        }

        public async Task<RuchMagazynowyCreateResultDto> UpdateAndReapplyAsync(int idRuchu, RuchMagazynowy ruchMagazynowy)
        {
            var errors = new List<RuchMagazynowyCreateErrorDto>();
            var existing = await _context.RuchMagazynowy.FirstOrDefaultAsync(x => x.IdRuchu == idRuchu);
            if (existing == null)
            {
                AddError(errors, string.Empty, "Nie znaleziono ruchu magazynowego do edycji.");
                return new RuchMagazynowyCreateResultDto { Success = false, Errors = errors };
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!await RevertMovementFromStockAsync(existing, errors))
                {
                    await transaction.RollbackAsync();
                    return new RuchMagazynowyCreateResultDto { Success = false, Errors = errors };
                }

                await ValidateMovementAsync(ruchMagazynowy, errors);
                if (errors.Count > 0)
                {
                    await transaction.RollbackAsync();
                    return new RuchMagazynowyCreateResultDto { Success = false, Errors = errors };
                }

                if (!await ApplyMovementToStockAsync(ruchMagazynowy, errors))
                {
                    await transaction.RollbackAsync();
                    return new RuchMagazynowyCreateResultDto { Success = false, Errors = errors };
                }

                existing.Typ = ruchMagazynowy.Typ;
                existing.IdProduktu = ruchMagazynowy.IdProduktu;
                existing.IdLokacjiZ = ruchMagazynowy.IdLokacjiZ;
                existing.IdLokacjiDo = ruchMagazynowy.IdLokacjiDo;
                existing.Ilosc = ruchMagazynowy.Ilosc;
                existing.Referencja = ruchMagazynowy.Referencja;
                existing.Notatka = ruchMagazynowy.Notatka;
                existing.IdUzytkownika = ruchMagazynowy.IdUzytkownika;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new RuchMagazynowyCreateResultDto { Success = true };
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                AddError(errors, string.Empty, "Nie udało sie zaktualizować ruchu i przeliczyć stanów magazynowych.");
                return new RuchMagazynowyCreateResultDto { Success = false, Errors = errors };
            }
        }

        public async Task<RuchMagazynowyCreateResultDto> DeleteAndRevertAsync(int idRuchu)
        {
            var errors = new List<RuchMagazynowyCreateErrorDto>();
            var existing = await _context.RuchMagazynowy.FirstOrDefaultAsync(x => x.IdRuchu == idRuchu);
            if (existing == null)
            {
                AddError(errors, string.Empty, "Nie znaleziono ruchu magazynowego do usunięcia.");
                return new RuchMagazynowyCreateResultDto { Success = false, Errors = errors };
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!await RevertMovementFromStockAsync(existing, errors))
                {
                    await transaction.RollbackAsync();
                    return new RuchMagazynowyCreateResultDto { Success = false, Errors = errors };
                }

                _context.RuchMagazynowy.Remove(existing);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new RuchMagazynowyCreateResultDto { Success = true };
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                AddError(errors, string.Empty, "Nie udało sie usunąć ruchu i cofnąć zmian stanów magazynowych.");
                return new RuchMagazynowyCreateResultDto { Success = false, Errors = errors };
            }
        }

        private async Task ValidateMovementAsync(RuchMagazynowy ruch, IList<RuchMagazynowyCreateErrorDto> errors)
        {
            if (ruch.Ilosc <= 0)
            {
                AddError(errors, nameof(RuchMagazynowy.Ilosc), "Ilość musi byc większa od zera.");
                return;
            }

            if (ruch.Typ == TypRuchuMagazynowego.Przyjęcie)
            {
                if (!ruch.IdLokacjiDo.HasValue)
                {
                    AddError(errors, nameof(RuchMagazynowy.IdLokacjiDo), "Dla przyjęcia wybierz lokacje docelową.");
                }
            }

            if (ruch.Typ == TypRuchuMagazynowego.Wydanie || ruch.Typ == TypRuchuMagazynowego.Przesunięcie)
            {
                if (!ruch.IdLokacjiZ.HasValue)
                {
                    AddError(errors, nameof(RuchMagazynowy.IdLokacjiZ), "Dla wydania/przesunięcia wybierz lokacje źródłową.");
                    return;
                }

                if (ruch.Typ == TypRuchuMagazynowego.Przesunięcie && !ruch.IdLokacjiDo.HasValue)
                {
                    AddError(errors, nameof(RuchMagazynowy.IdLokacjiDo), "Dla przesunięcia wybierz lokacje docelową.");
                }

                if (ruch.Typ == TypRuchuMagazynowego.Przesunięcie && ruch.IdLokacjiZ == ruch.IdLokacjiDo)
                {
                    AddError(errors, nameof(RuchMagazynowy.IdLokacjiDo), "Lokacja docelowa musi być inna niz źródłowa.");
                }

                var stan = await _context.StanMagazynowy
                    .AsNoTracking()
                    .Where(s => s.IdProduktu == ruch.IdProduktu && s.IdLokacji == ruch.IdLokacjiZ.Value)
                    .Select(s => new { s.Ilosc, ProduktKod = s.Produkt.Kod, LokacjaKod = s.Lokacja.Kod })
                    .FirstOrDefaultAsync();

                if (stan == null)
                {
                    AddError(errors, nameof(RuchMagazynowy.IdLokacjiZ), "W wybranej lokacji źródłowej nie ma stanu dla tego produktu.");
                    return;
                }

                if (stan.Ilosc < ruch.Ilosc)
                {
                    AddError(errors, nameof(RuchMagazynowy.Ilosc), $"Niewystarczający stan w lokacji {stan.LokacjaKod} dla produktu {stan.ProduktKod}. Dostępne: {stan.Ilosc.ToString("0.###")}.");
                }
            }
        }

        private async Task<bool> ApplyMovementToStockAsync(RuchMagazynowy ruch, IList<RuchMagazynowyCreateErrorDto> errors)
        {
            switch (ruch.Typ)
            {
                case TypRuchuMagazynowego.Przyjęcie:
                    if (!ruch.IdLokacjiDo.HasValue)
                    {
                        AddError(errors, nameof(RuchMagazynowy.IdLokacjiDo), "Dla przyjęcia wybierz lokacje docelową.");
                        return false;
                    }

                    await IncreaseStockAsync(ruch.IdProduktu, ruch.IdLokacjiDo.Value, ruch.Ilosc);
                    return true;

                case TypRuchuMagazynowego.Wydanie:
                    if (!ruch.IdLokacjiZ.HasValue)
                    {
                        AddError(errors, nameof(RuchMagazynowy.IdLokacjiZ), "Dla wydania wybierz lokacje źródłową.");
                        return false;
                    }

                    return await DecreaseStockAsync(ruch.IdProduktu, ruch.IdLokacjiZ.Value, ruch.Ilosc, errors);

                case TypRuchuMagazynowego.Przesunięcie:
                    if (!ruch.IdLokacjiZ.HasValue || !ruch.IdLokacjiDo.HasValue)
                    {
                        AddError(errors, string.Empty, "Dla przesunięcia wybierz lokacje źródłową i docelową.");
                        return false;
                    }

                    if (ruch.IdLokacjiZ.Value == ruch.IdLokacjiDo.Value)
                    {
                        AddError(errors, nameof(RuchMagazynowy.IdLokacjiDo), "Lokacja docelowa musi być inna niż źródłowa.");
                        return false;
                    }

                    if (!await DecreaseStockAsync(ruch.IdProduktu, ruch.IdLokacjiZ.Value, ruch.Ilosc, errors))
                    {
                        return false;
                    }

                    await IncreaseStockAsync(ruch.IdProduktu, ruch.IdLokacjiDo.Value, ruch.Ilosc);
                    return true;

                case TypRuchuMagazynowego.Korekta:
                case TypRuchuMagazynowego.Inwentaryzacja:
                    if (ruch.IdLokacjiZ.HasValue && ruch.IdLokacjiDo.HasValue && ruch.IdLokacjiZ.Value != ruch.IdLokacjiDo.Value)
                    {
                        AddError(errors, string.Empty, "Dla korekty/inwentaryzacji wybierz jedną lokacje (Z lub Do).");
                        return false;
                    }

                    if (ruch.IdLokacjiDo.HasValue && !ruch.IdLokacjiZ.HasValue)
                    {
                        await IncreaseStockAsync(ruch.IdProduktu, ruch.IdLokacjiDo.Value, ruch.Ilosc);
                        return true;
                    }

                    if (ruch.IdLokacjiZ.HasValue && !ruch.IdLokacjiDo.HasValue)
                    {
                        return await DecreaseStockAsync(ruch.IdProduktu, ruch.IdLokacjiZ.Value, ruch.Ilosc, errors);
                    }

                    AddError(errors, string.Empty, "Dla korekty/inwentaryzacji wybierz jedną lokacje (Z lub Do).");
                    return false;

                default:
                    AddError(errors, nameof(RuchMagazynowy.Typ), "Nieobsługiwalny typ ruchu.");
                    return false;
            }
        }

        private async Task<bool> RevertMovementFromStockAsync(RuchMagazynowy ruch, IList<RuchMagazynowyCreateErrorDto> errors)
        {
            // Revert old effect before reapplying edited movement or deleting it.
            switch (ruch.Typ)
            {
                case TypRuchuMagazynowego.Przyjęcie:
                    if (!ruch.IdLokacjiDo.HasValue)
                    {
                        AddError(errors, string.Empty, "Ruch przyjęcia ma nieprawidłową lokację docelową.");
                        return false;
                    }

                    return await DecreaseStockAsync(ruch.IdProduktu, ruch.IdLokacjiDo.Value, ruch.Ilosc, errors, nameof(RuchMagazynowy.IdLokacjiDo));

                case TypRuchuMagazynowego.Wydanie:
                    if (!ruch.IdLokacjiZ.HasValue)
                    {
                        AddError(errors, string.Empty, "Ruch wydania ma nieprawidłową lokację źródłową.");
                        return false;
                    }

                    await IncreaseStockAsync(ruch.IdProduktu, ruch.IdLokacjiZ.Value, ruch.Ilosc);
                    return true;

                case TypRuchuMagazynowego.Przesunięcie:
                    if (!ruch.IdLokacjiZ.HasValue || !ruch.IdLokacjiDo.HasValue)
                    {
                        AddError(errors, string.Empty, "Ruch przesunięcia ma nieprawidłowe lokacje.");
                        return false;
                    }

                    if (!await DecreaseStockAsync(ruch.IdProduktu, ruch.IdLokacjiDo.Value, ruch.Ilosc, errors, nameof(RuchMagazynowy.IdLokacjiDo)))
                    {
                        return false;
                    }

                    await IncreaseStockAsync(ruch.IdProduktu, ruch.IdLokacjiZ.Value, ruch.Ilosc);
                    return true;

                case TypRuchuMagazynowego.Korekta:
                case TypRuchuMagazynowego.Inwentaryzacja:
                    if (ruch.IdLokacjiDo.HasValue && !ruch.IdLokacjiZ.HasValue)
                    {
                        return await DecreaseStockAsync(ruch.IdProduktu, ruch.IdLokacjiDo.Value, ruch.Ilosc, errors, nameof(RuchMagazynowy.IdLokacjiDo));
                    }

                    if (ruch.IdLokacjiZ.HasValue && !ruch.IdLokacjiDo.HasValue)
                    {
                        await IncreaseStockAsync(ruch.IdProduktu, ruch.IdLokacjiZ.Value, ruch.Ilosc);
                        return true;
                    }

                    AddError(errors, string.Empty, "Ruch korekty/inwentaryzacji ma nieprawidłową konfigurację lokacji.");
                    return false;

                default:
                    AddError(errors, nameof(RuchMagazynowy.Typ), "Nieobsługiwalny typ ruchu.");
                    return false;
            }
        }

        private async Task IncreaseStockAsync(int idProduktu, int idLokacji, decimal ilosc)
        {
            var stan = await _context.StanMagazynowy
                .FirstOrDefaultAsync(s => s.IdProduktu == idProduktu && s.IdLokacji == idLokacji);

            if (stan == null)
            {
                stan = new StanMagazynowy
                {
                    IdProduktu = idProduktu,
                    IdLokacji = idLokacji,
                    Ilosc = 0m
                };
                _context.StanMagazynowy.Add(stan);
            }

            stan.Ilosc += ilosc;
        }

        private async Task<bool> DecreaseStockAsync(int idProduktu, int idLokacji, decimal ilosc, IList<RuchMagazynowyCreateErrorDto> errors, string errorKey = nameof(RuchMagazynowy.IdLokacjiZ))
        {
            var stan = await _context.StanMagazynowy
                .Include(s => s.Produkt)
                .Include(s => s.Lokacja)
                .FirstOrDefaultAsync(s => s.IdProduktu == idProduktu && s.IdLokacji == idLokacji);

            if (stan == null)
            {
                AddError(errors, errorKey, "W wybranej lokacji źródłowej nie ma stanu dla tego produktu.");
                return false;
            }

            if (stan.Ilosc < ilosc)
            {
                AddError(errors, nameof(RuchMagazynowy.Ilosc), $"Niewystarczający stan w lokacji {stan.Lokacja.Kod} dla produktu {stan.Produkt.Kod}. Dostępne: {stan.Ilosc.ToString("0.###")}.");
                return false;
            }

            stan.Ilosc -= ilosc;
            return true;
        }

        private static void AddError(ICollection<RuchMagazynowyCreateErrorDto> errors, string key, string message)
        {
            errors.Add(new RuchMagazynowyCreateErrorDto
            {
                Key = key,
                Message = message
            });
        }
    }
}
