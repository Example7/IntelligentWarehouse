using ClosedXML.Excel;
using Data.Data;
using Data.Data.Magazyn;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Services.Abstrakcja;
using System.Data;
using System.Globalization;

namespace Services.Magazyn
{
    public class RaportMagazynowyService : BaseService, IRaportMagazynowyService
    {
        private static readonly CultureInfo PlCulture = CultureInfo.GetCultureInfo("pl-PL");

        public RaportMagazynowyService(DataContext context) : base(context)
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<RaportPropozycjeZamowienDto> GetRaportPropozycjiZamowienAsync(string? searchTerm, int? idMagazynu, int? idDostawcy = null, int? idLokacjiPrzyjecia = null)
        {
            var magazyny = await _context.Magazyn
                .AsNoTracking()
                .OrderBy(m => m.Nazwa)
                .Select(m => new RaportMagazynSelectOptionDto
                {
                    Value = m.IdMagazynu,
                    Text = m.Nazwa
                })
                .ToListAsync();

            var dostawcy = await _context.Dostawca
                .AsNoTracking()
                .Where(d => d.CzyAktywny)
                .OrderBy(d => d.Nazwa)
                .Select(d => new RaportMagazynSelectOptionDto
                {
                    Value = d.IdDostawcy,
                    Text = d.Nazwa
                })
                .ToListAsync();

            var lokacjePrzyjeciaQuery = _context.Lokacja
                .AsNoTracking()
                .Where(l => l.CzyAktywna);

            if (idMagazynu.HasValue)
            {
                lokacjePrzyjeciaQuery = lokacjePrzyjeciaQuery.Where(l => l.IdMagazynu == idMagazynu.Value);
            }

            var lokacjePrzyjecia = await lokacjePrzyjeciaQuery
                .OrderBy(l => l.Magazyn.Nazwa)
                .ThenBy(l => l.Kod)
                .Select(l => new RaportMagazynSelectOptionDto
                {
                    Value = l.IdLokacji,
                    Text = (l.Magazyn != null ? l.Magazyn.Nazwa + " / " : string.Empty) + l.Kod
                })
                .ToListAsync();

            var rows = new List<RaportPropozycjeZamowienRowDto>();
            var connection = _context.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            if (shouldClose)
            {
                await connection.OpenAsync();
            }

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = "dbo.usp_GenerateReorderSuggestions";
                command.CommandType = CommandType.StoredProcedure;

                var warehouseParam = command.CreateParameter();
                warehouseParam.ParameterName = "@WarehouseId";
                warehouseParam.Value = idMagazynu.HasValue ? idMagazynu.Value : DBNull.Value;
                command.Parameters.Add(warehouseParam);

                var searchParam = command.CreateParameter();
                searchParam.ParameterName = "@SearchTerm";
                searchParam.Value = string.IsNullOrWhiteSpace(searchTerm) ? DBNull.Value : searchTerm!.Trim();
                command.Parameters.Add(searchParam);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    rows.Add(new RaportPropozycjeZamowienRowDto
                    {
                        IdProduktu = reader.GetInt32(reader.GetOrdinal("ProductId")),
                        ProduktKod = reader.GetString(reader.GetOrdinal("SKU")),
                        ProduktNazwa = reader.GetString(reader.GetOrdinal("ProductName")),
                        KategoriaNazwa = reader["CategoryName"] as string ?? "-",
                        Jednostka = reader["UomCode"] as string ?? "j.m.",
                        IdMagazynu = reader.GetInt32(reader.GetOrdinal("WarehouseId")),
                        MagazynNazwa = reader.GetString(reader.GetOrdinal("WarehouseName")),
                        StanFizyczny = reader.GetDecimal(reader.GetOrdinal("PhysicalQty")),
                        ZarezerwowaneAktywnie = reader.GetDecimal(reader.GetOrdinal("ReservedActiveQty")),
                        ZarezerwowaneWzDraft = reader.GetDecimal(reader.GetOrdinal("ReservedDraftWzQty")),
                        DostepneDoRezerwacji = reader.GetDecimal(reader.GetOrdinal("AvailableQty")),
                        StanMinimalny = reader.GetDecimal(reader.GetOrdinal("MinStock")),
                        PunktPonownegoZamowienia = reader.GetDecimal(reader.GetOrdinal("ReorderPoint")),
                        IloscPonownegoZamowienia = reader.GetDecimal(reader.GetOrdinal("ReorderQty")),
                        ProponowanaIloscZamowienia = reader.GetDecimal(reader.GetOrdinal("SuggestedOrderQty")),
                        BrakDoRop = reader.GetDecimal(reader.GetOrdinal("ShortageToRop"))
                    });
                }
            }
            finally
            {
                if (shouldClose)
                {
                    await connection.CloseAsync();
                }
            }

            return new RaportPropozycjeZamowienDto
            {
                SearchTerm = searchTerm,
                IdMagazynu = idMagazynu,
                IdDostawcy = idDostawcy,
                IdLokacjiPrzyjecia = idLokacjiPrzyjecia,
                WygenerowanoUtc = DateTime.UtcNow,
                Magazyny = magazyny,
                Dostawcy = dostawcy,
                LokacjePrzyjecia = lokacjePrzyjecia,
                Rows = rows
            };
        }

        public async Task<RaportStanyMagazynoweDto> GetRaportStanowAsync(string? searchTerm, int? idMagazynu)
        {
            var rows = await GetRowsAsync(searchTerm, idMagazynu);
            var magazyny = await _context.Magazyn
                .AsNoTracking()
                .OrderBy(m => m.Nazwa)
                .Select(m => new RaportMagazynSelectOptionDto
                {
                    Value = m.IdMagazynu,
                    Text = m.Nazwa
                })
                .ToListAsync();

            var summary = rows
                .GroupBy(r => r.Jednostka)
                .OrderBy(g => g.Key)
                .Select(g => new RaportStanyMagazynoweSummaryDto
                {
                    Jednostka = g.Key,
                    Ilosc = g.Sum(x => x.Ilosc),
                    ZarezerwowaneLokacyjnie = g.Sum(x => x.ZarezerwowaneLokacyjnie),
                    Dostepne = g.Sum(x => x.Dostepne)
                })
                .ToList();

            return new RaportStanyMagazynoweDto
            {
                SearchTerm = searchTerm,
                IdMagazynu = idMagazynu,
                Magazyny = magazyny,
                Rows = rows,
                SumaWgJednostki = summary,
                SumaWgJednostkiLabel = summary.Count == 0
                    ? "-"
                    : string.Join(", ", summary.Select(x => $"{FormatQty(x.Ilosc)} {x.Jednostka}")),
                WygenerowanoUtc = DateTime.UtcNow
            };
        }

        public async Task<byte[]> ExportRaportStanowExcelAsync(string? searchTerm, int? idMagazynu)
        {
            var model = await GetRaportStanowAsync(searchTerm, idMagazynu);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Stany");

            ws.Cell(1, 1).Value = "Raport stanów magazynowych";
            ws.Cell(2, 1).Value = "Wygenerowano";
            ws.Cell(2, 2).Value = model.WygenerowanoUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", PlCulture);
            ws.Cell(3, 1).Value = "Filtr magazynu";
            ws.Cell(3, 2).Value = model.IdMagazynu.HasValue
                ? model.Magazyny.FirstOrDefault(x => x.Value == model.IdMagazynu.Value)?.Text ?? $"ID={model.IdMagazynu.Value}"
                : "Wszystkie";
            ws.Cell(4, 1).Value = "Szukaj";
            ws.Cell(4, 2).Value = string.IsNullOrWhiteSpace(model.SearchTerm) ? "-" : model.SearchTerm;

            var headerRow = 6;
            var headers = new[]
            {
                "SKU", "Produkt", "Magazyn", "Lokacja", "Ilość", "Zarezerwowane (lok.)", "Dostępne", "JM"
            };

            for (var i = 0; i < headers.Length; i++)
            {
                ws.Cell(headerRow, i + 1).Value = headers[i];
                ws.Cell(headerRow, i + 1).Style.Font.Bold = true;
                ws.Cell(headerRow, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF1FF");
            }

            var rowIndex = headerRow + 1;
            foreach (var row in model.Rows)
            {
                ws.Cell(rowIndex, 1).Value = row.ProduktKod;
                ws.Cell(rowIndex, 2).Value = row.ProduktNazwa;
                ws.Cell(rowIndex, 3).Value = row.MagazynNazwa;
                ws.Cell(rowIndex, 4).Value = row.LokacjaKod;
                ws.Cell(rowIndex, 5).Value = row.Ilosc;
                ws.Cell(rowIndex, 6).Value = row.ZarezerwowaneLokacyjnie;
                ws.Cell(rowIndex, 7).Value = row.Dostepne;
                ws.Cell(rowIndex, 8).Value = row.Jednostka;
                rowIndex++;
            }

            if (model.Rows.Count > 0)
            {
                ws.Range(headerRow, 1, rowIndex - 1, headers.Length).CreateTable();
            }

            ws.Columns(5, 7).Style.NumberFormat.Format = "0.###";
            ws.Columns().AdjustToContents();

            var summarySheet = workbook.Worksheets.Add("PodsumowanieJM");
            summarySheet.Cell(1, 1).Value = "Jednostka";
            summarySheet.Cell(1, 2).Value = "Ilość";
            summarySheet.Cell(1, 3).Value = "Zarezerwowane (lok.)";
            summarySheet.Cell(1, 4).Value = "Dostępne";
            for (var i = 1; i <= 4; i++)
            {
                summarySheet.Cell(1, i).Style.Font.Bold = true;
                summarySheet.Cell(1, i).Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF1FF");
            }

            for (var i = 0; i < model.SumaWgJednostki.Count; i++)
            {
                var s = model.SumaWgJednostki[i];
                summarySheet.Cell(i + 2, 1).Value = s.Jednostka;
                summarySheet.Cell(i + 2, 2).Value = s.Ilosc;
                summarySheet.Cell(i + 2, 3).Value = s.ZarezerwowaneLokacyjnie;
                summarySheet.Cell(i + 2, 4).Value = s.Dostepne;
            }
            summarySheet.Columns(2, 4).Style.NumberFormat.Format = "0.###";
            summarySheet.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        public async Task<byte[]> ExportRaportStanowPdfAsync(string? searchTerm, int? idMagazynu)
        {
            var model = await GetRaportStanowAsync(searchTerm, idMagazynu);
            var generatedLocal = model.WygenerowanoUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", PlCulture);
            var warehouseLabel = model.IdMagazynu.HasValue
                ? model.Magazyny.FirstOrDefault(x => x.Value == model.IdMagazynu.Value)?.Text ?? $"ID={model.IdMagazynu.Value}"
                : "Wszystkie magazyny";
            var searchLabel = string.IsNullOrWhiteSpace(model.SearchTerm) ? "-" : model.SearchTerm!;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Raport stanów magazynowych").FontSize(18).SemiBold();
                        col.Item().Text($"Wygenerowano: {generatedLocal}");
                        col.Item().Text($"Magazyn: {warehouseLabel}");
                        col.Item().Text($"Szukaj: {searchLabel}");
                        col.Item().PaddingTop(4).Text($"Suma wg JM: {model.SumaWgJednostkiLabel}");
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.2f); // SKU
                            columns.RelativeColumn(2.0f); // Produkt
                            columns.RelativeColumn(1.6f); // Magazyn
                            columns.RelativeColumn(1.0f); // Lokacja
                            columns.RelativeColumn(0.8f); // Ilosc
                            columns.RelativeColumn(1.1f); // Rez
                            columns.RelativeColumn(0.9f); // Dostepne
                            columns.RelativeColumn(0.6f); // JM
                        });

                        void HeaderCell(string text) => table.Cell().Background(Colors.Blue.Lighten4).Padding(4).Text(text).SemiBold();
                        HeaderCell("SKU");
                        HeaderCell("Produkt");
                        HeaderCell("Magazyn");
                        HeaderCell("Lokacja");
                        HeaderCell("Ilość");
                        HeaderCell("Rez. (lok.)");
                        HeaderCell("Dostępne");
                        HeaderCell("JM");

                        foreach (var row in model.Rows)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(row.ProduktKod);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(row.ProduktNazwa);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(row.MagazynNazwa);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(row.LokacjaKod);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text(FormatQty(row.Ilosc));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text(FormatQty(row.ZarezerwowaneLokacyjnie));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text(FormatQty(row.Dostepne));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(row.Jednostka);
                        }

                        if (model.Rows.Count == 0)
                        {
                            table.Cell().ColumnSpan(8).Padding(8).Text("Brak danych dla wybranych filtrów.").Italic();
                        }
                    });

                    page.Footer().AlignRight().Text(x =>
                    {
                        x.Span("Strona ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

        public async Task<RaportRuchyMagazynoweDto> GetRaportRuchowAsync(string? searchTerm, int? idMagazynu, TypRuchuMagazynowego? typ, DateTime? dataOd, DateTime? dataDo)
        {
            var rows = await GetMovementRowsAsync(searchTerm, idMagazynu, typ, dataOd, dataDo);
            var magazyny = await _context.Magazyn
                .AsNoTracking()
                .OrderBy(m => m.Nazwa)
                .Select(m => new RaportMagazynSelectOptionDto { Value = m.IdMagazynu, Text = m.Nazwa })
                .ToListAsync();

            var summary = rows
                .GroupBy(x => x.Typ)
                .OrderBy(x => (int)x.Key)
                .Select(g => new RaportRuchyMagazynoweTypeSummaryDto
                {
                    Typ = g.Key,
                    LiczbaRuchow = g.Count()
                })
                .ToList();

            return new RaportRuchyMagazynoweDto
            {
                SearchTerm = searchTerm,
                IdMagazynu = idMagazynu,
                Typ = typ,
                DataOd = dataOd,
                DataDo = dataDo,
                WygenerowanoUtc = DateTime.UtcNow,
                Magazyny = magazyny,
                Rows = rows,
                PodsumowanieTypow = summary
            };
        }

        public async Task<byte[]> ExportRaportRuchowExcelAsync(string? searchTerm, int? idMagazynu, TypRuchuMagazynowego? typ, DateTime? dataOd, DateTime? dataDo)
        {
            var model = await GetRaportRuchowAsync(searchTerm, idMagazynu, typ, dataOd, dataDo);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Ruchy");

            ws.Cell(1, 1).Value = "Raport ruchów magazynowych";
            ws.Cell(2, 1).Value = "Wygenerowano";
            ws.Cell(2, 2).Value = model.WygenerowanoUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", PlCulture);
            ws.Cell(3, 1).Value = "Magazyn";
            ws.Cell(3, 2).Value = model.IdMagazynu.HasValue
                ? model.Magazyny.FirstOrDefault(x => x.Value == model.IdMagazynu.Value)?.Text ?? $"ID={model.IdMagazynu.Value}"
                : "Wszystkie";
            ws.Cell(4, 1).Value = "Typ";
            ws.Cell(4, 2).Value = model.Typ?.ToString() ?? "Wszystkie";
            ws.Cell(5, 1).Value = "Zakres";
            ws.Cell(5, 2).Value = $"{FormatDate(model.DataOd)} - {FormatDate(model.DataDo)}";

            var headerRow = 7;
            var headers = new[]
            {
                "Typ", "SKU", "Produkt", "Z (magazyn/lokacja)", "Do (magazyn/lokacja)", "Ilość", "JM", "Referencja", "Utworzono", "Użytkownik", "Notatka"
            };
            for (var i = 0; i < headers.Length; i++)
            {
                ws.Cell(headerRow, i + 1).Value = headers[i];
                ws.Cell(headerRow, i + 1).Style.Font.Bold = true;
                ws.Cell(headerRow, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF1FF");
            }

            var rowIndex = headerRow + 1;
            foreach (var row in model.Rows)
            {
                ws.Cell(rowIndex, 1).Value = row.Typ.ToString();
                ws.Cell(rowIndex, 2).Value = row.ProduktKod;
                ws.Cell(rowIndex, 3).Value = row.ProduktNazwa;
                ws.Cell(rowIndex, 4).Value = $"{NormalizeDash(row.MagazynZNazwa)} / {NormalizeDash(row.LokacjaZKod)}";
                ws.Cell(rowIndex, 5).Value = $"{NormalizeDash(row.MagazynDoNazwa)} / {NormalizeDash(row.LokacjaDoKod)}";
                ws.Cell(rowIndex, 6).Value = row.Ilosc;
                ws.Cell(rowIndex, 7).Value = row.Jednostka;
                ws.Cell(rowIndex, 8).Value = row.Referencja ?? string.Empty;
                ws.Cell(rowIndex, 9).Value = row.UtworzonoUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", PlCulture);
                ws.Cell(rowIndex, 10).Value = row.UzytkownikLabel;
                ws.Cell(rowIndex, 11).Value = row.Notatka ?? string.Empty;
                rowIndex++;
            }

            if (model.Rows.Count > 0)
            {
                ws.Range(headerRow, 1, rowIndex - 1, headers.Length).CreateTable();
            }
            ws.Column(6).Style.NumberFormat.Format = "0.###";
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        public async Task<byte[]> ExportRaportRuchowPdfAsync(string? searchTerm, int? idMagazynu, TypRuchuMagazynowego? typ, DateTime? dataOd, DateTime? dataDo)
        {
            var model = await GetRaportRuchowAsync(searchTerm, idMagazynu, typ, dataOd, dataDo);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(16);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Raport ruchów magazynowych").FontSize(16).SemiBold();
                        col.Item().Text($"Wygenerowano: {model.WygenerowanoUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
                        col.Item().Text($"Magazyn: {(model.IdMagazynu.HasValue ? model.Magazyny.FirstOrDefault(x => x.Value == model.IdMagazynu.Value)?.Text ?? $"ID={model.IdMagazynu}" : "Wszystkie")}");
                        col.Item().Text($"Typ: {(model.Typ?.ToString() ?? "Wszystkie")} | Zakres: {FormatDate(model.DataOd)} - {FormatDate(model.DataDo)}");
                    });

                    page.Content().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(0.9f); // typ
                            columns.RelativeColumn(1.2f); // sku
                            columns.RelativeColumn(1.8f); // produkt
                            columns.RelativeColumn(1.5f); // z
                            columns.RelativeColumn(1.5f); // do
                            columns.RelativeColumn(0.8f); // qty
                            columns.RelativeColumn(0.6f); // jm
                            columns.RelativeColumn(1.2f); // ref
                            columns.RelativeColumn(1.2f); // time
                            columns.RelativeColumn(1.6f); // user
                        });

                        void Head(string t) => table.Cell().Background(Colors.Blue.Lighten4).Padding(3).Text(t).SemiBold();
                        Head("Typ"); Head("SKU"); Head("Produkt"); Head("Z"); Head("Do"); Head("Ilość"); Head("JM"); Head("Ref."); Head("Utworzono"); Head("Użytk.");

                        foreach (var row in model.Rows)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(row.Typ.ToString());
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(row.ProduktKod);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(row.ProduktNazwa);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text($"{NormalizeDash(row.LokacjaZKod)}");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text($"{NormalizeDash(row.LokacjaDoKod)}");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).AlignRight().Text(FormatQty(row.Ilosc));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(row.Jednostka);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(row.Referencja ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(row.UtworzonoUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(2).Text(row.UzytkownikLabel);
                        }

                        if (model.Rows.Count == 0)
                        {
                            table.Cell().ColumnSpan(10).Padding(6).Text("Brak danych dla wybranych filtrów.").Italic();
                        }
                    });

                    page.Footer().AlignRight().Text(x =>
                    {
                        x.Span("Strona ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

        private async Task<List<RaportStanyMagazynoweRowDto>> GetRowsAsync(string? searchTerm, int? idMagazynu)
        {
            var query = _context.StanMagazynowy
                .AsNoTracking()
                .Include(s => s.Lokacja)
                    .ThenInclude(l => l.Magazyn)
                .Include(s => s.Produkt)
                    .ThenInclude(p => p.DomyslnaJednostka)
                .AsQueryable();

            if (idMagazynu.HasValue)
            {
                query = query.Where(s => s.Lokacja.IdMagazynu == idMagazynu.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(s =>
                    EF.Functions.Like(s.Produkt.Kod, $"%{term}%") ||
                    EF.Functions.Like(s.Produkt.Nazwa, $"%{term}%") ||
                    EF.Functions.Like(s.Lokacja.Kod, $"%{term}%") ||
                    (s.Lokacja.Opis != null && EF.Functions.Like(s.Lokacja.Opis, $"%{term}%")) ||
                    EF.Functions.Like(s.Lokacja.Magazyn.Nazwa, $"%{term}%"));
            }

            var items = await query.ToListAsync();
            var keys = items.Select(x => new { x.IdProduktu, x.IdLokacji }).Distinct().ToList();
            var reservedMap = new Dictionary<string, decimal>();

            if (keys.Count > 0)
            {
                var productIds = keys.Select(x => x.IdProduktu).Distinct().ToList();
                var locationIds = keys.Select(x => x.IdLokacji).Distinct().ToList();

                var reservations = await _context.PozycjaRezerwacji
                    .AsNoTracking()
                    .Where(p =>
                        p.IdLokacji.HasValue &&
                        p.Rezerwacja.Status == "Active" &&
                        productIds.Contains(p.IdProduktu) &&
                        locationIds.Contains(p.IdLokacji.Value))
                    .GroupBy(p => new { p.IdProduktu, IdLokacji = p.IdLokacji!.Value })
                    .Select(g => new
                    {
                        g.Key.IdProduktu,
                        g.Key.IdLokacji,
                        Qty = g.Sum(x => x.Ilosc)
                    })
                    .ToListAsync();

                reservedMap = reservations.ToDictionary(
                    x => BuildKey(x.IdProduktu, x.IdLokacji),
                    x => x.Qty);
            }

            return items
                .Select(item =>
                {
                    var key = BuildKey(item.IdProduktu, item.IdLokacji);
                    var reserved = reservedMap.TryGetValue(key, out var r) ? r : 0m;
                    return new RaportStanyMagazynoweRowDto
                    {
                        IdStanu = item.IdStanu,
                        IdProduktu = item.IdProduktu,
                        IdLokacji = item.IdLokacji,
                        ProduktKod = item.Produkt?.Kod ?? "-",
                        ProduktNazwa = item.Produkt?.Nazwa ?? "-",
                        MagazynNazwa = item.Lokacja?.Magazyn?.Nazwa ?? "-",
                        LokacjaKod = item.Lokacja?.Kod ?? "-",
                        Ilosc = item.Ilosc,
                        ZarezerwowaneLokacyjnie = reserved,
                        Dostepne = item.Ilosc - reserved,
                        Jednostka = item.Produkt?.DomyslnaJednostka?.Kod ?? "j.m."
                    };
                })
                .OrderBy(x => x.ProduktKod)
                .ThenBy(x => x.MagazynNazwa)
                .ThenBy(x => x.LokacjaKod)
                .ToList();
        }

        private async Task<List<RaportRuchyMagazynoweRowDto>> GetMovementRowsAsync(string? searchTerm, int? idMagazynu, TypRuchuMagazynowego? typ, DateTime? dataOd, DateTime? dataDo)
        {
            var query = _context.RuchMagazynowy
                .AsNoTracking()
                .Include(r => r.Produkt).ThenInclude(p => p.DomyslnaJednostka)
                .Include(r => r.LokacjaZ).ThenInclude(l => l!.Magazyn)
                .Include(r => r.LokacjaDo).ThenInclude(l => l!.Magazyn)
                .Include(r => r.Uzytkownik)
                .AsQueryable();

            if (typ.HasValue)
            {
                query = query.Where(r => r.Typ == typ.Value);
            }

            if (idMagazynu.HasValue)
            {
                query = query.Where(r =>
                    (r.LokacjaZ != null && r.LokacjaZ.IdMagazynu == idMagazynu.Value) ||
                    (r.LokacjaDo != null && r.LokacjaDo.IdMagazynu == idMagazynu.Value));
            }

            if (dataOd.HasValue)
            {
                var from = dataOd.Value.Date;
                query = query.Where(r => r.UtworzonoUtc >= from);
            }

            if (dataDo.HasValue)
            {
                var toExclusive = dataDo.Value.Date.AddDays(1);
                query = query.Where(r => r.UtworzonoUtc < toExclusive);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim();
                query = query.Where(r =>
                    (r.Referencja != null && EF.Functions.Like(r.Referencja, $"%{term}%")) ||
                    (r.Notatka != null && EF.Functions.Like(r.Notatka, $"%{term}%")) ||
                    EF.Functions.Like(r.Produkt.Kod, $"%{term}%") ||
                    EF.Functions.Like(r.Produkt.Nazwa, $"%{term}%") ||
                    (r.LokacjaZ != null && EF.Functions.Like(r.LokacjaZ.Kod, $"%{term}%")) ||
                    (r.LokacjaDo != null && EF.Functions.Like(r.LokacjaDo.Kod, $"%{term}%")) ||
                    (r.LokacjaZ != null && r.LokacjaZ.Magazyn != null && EF.Functions.Like(r.LokacjaZ.Magazyn.Nazwa, $"%{term}%")) ||
                    (r.LokacjaDo != null && r.LokacjaDo.Magazyn != null && EF.Functions.Like(r.LokacjaDo.Magazyn.Nazwa, $"%{term}%")) ||
                    (r.Uzytkownik != null && EF.Functions.Like(r.Uzytkownik.Email, $"%{term}%")));
            }

            var items = await query
                .OrderByDescending(r => r.UtworzonoUtc)
                .ThenByDescending(r => r.IdRuchu)
                .ToListAsync();

            return items.Select(r => new RaportRuchyMagazynoweRowDto
            {
                IdRuchu = r.IdRuchu,
                Typ = r.Typ,
                IdProduktu = r.IdProduktu,
                ProduktKod = r.Produkt?.Kod ?? "-",
                ProduktNazwa = r.Produkt?.Nazwa ?? "-",
                Jednostka = r.Produkt?.DomyslnaJednostka?.Kod ?? "j.m.",
                Ilosc = r.Ilosc,
                IdLokacjiZ = r.IdLokacjiZ,
                LokacjaZKod = r.LokacjaZ?.Kod ?? "-",
                MagazynZNazwa = r.LokacjaZ?.Magazyn?.Nazwa ?? "-",
                IdLokacjiDo = r.IdLokacjiDo,
                LokacjaDoKod = r.LokacjaDo?.Kod ?? "-",
                MagazynDoNazwa = r.LokacjaDo?.Magazyn?.Nazwa ?? "-",
                Referencja = r.Referencja,
                Notatka = r.Notatka,
                UtworzonoUtc = r.UtworzonoUtc,
                UzytkownikLabel = r.Uzytkownik?.Email ?? r.Uzytkownik?.Login ?? "-"
            }).ToList();
        }

        private static string BuildKey(int idProduktu, int idLokacji) => $"{idProduktu}:{idLokacji}";
        private static string FormatQty(decimal value) => value.ToString("0.###", PlCulture);
        private static string FormatDate(DateTime? value) => value.HasValue ? value.Value.ToString("yyyy-MM-dd", PlCulture) : "-";
        private static string NormalizeDash(string? value) => string.IsNullOrWhiteSpace(value) || value == "-" ? "-" : value;
    }
}
