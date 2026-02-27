using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Data.Data;
using Data.Data.CMS;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Interfaces.Magazyn;
using Interfaces.Magazyn.Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Abstrakcja;

namespace Services.Magazyn
{
    public class WydrukDokumentuService : BaseService, IWydrukDokumentuService
    {
        private static readonly CultureInfo PlCulture = CultureInfo.GetCultureInfo("pl-PL");
        private static readonly Regex LoopRegex = new(@"\{\{#Items\}\}(.*?)\{\{\/Items\}\}", RegexOptions.Singleline | RegexOptions.Compiled);

        public WydrukDokumentuService(DataContext context) : base(context)
        {
        }

        public async Task<WydrukDokumentuResultDto> GenerujWydrukWzAsync(int idDokumentuWz, int? idSzablonu = null)
        {
            var dokument = await _context.DokumentWZ
                .AsNoTracking()
                .Include(x => x.Magazyn)
                .Include(x => x.Klient)
                .Include(x => x.Utworzyl)
                .Include(x => x.Pozycje)
                    .ThenInclude(x => x.Produkt)
                        .ThenInclude(x => x.DomyslnaJednostka)
                .Include(x => x.Pozycje)
                    .ThenInclude(x => x.Lokacja)
                        .ThenInclude(x => x!.Magazyn)
                .Include(x => x.Pozycje)
                    .ThenInclude(x => x.Partia)
                .FirstOrDefaultAsync(x => x.Id == idDokumentuWz);

            if (dokument == null)
            {
                throw new InvalidOperationException($"Nie znaleziono dokumentu WZ o ID={idDokumentuWz}.");
            }

            var szablon = await PobierzSzablonAsync("WZ", idSzablonu);
            var templatePath = ResolveTemplatePath(szablon.Sciezka);
            var outputBaseName = BuildOutputBaseName("WZ", dokument.Numer);
            var usedFallback = false;
            string? infoMessage = null;

            if (!File.Exists(templatePath))
            {
                usedFallback = true;
                infoMessage = $"Nie znaleziono pliku szablonu '{szablon.Sciezka}'. Wygenerowano awaryjny wydruk DOCX.";
                return AddMeta(await GenerateFallbackWzDocxAsync(szablon, dokument, outputBaseName), usedFallback, infoMessage);
            }

            var ext = Path.GetExtension(templatePath).ToLowerInvariant();
            var wzItems = dokument.Pozycje.OrderBy(x => x.Lp).ThenBy(x => x.Id).ToList();
            var wzHeaderMap = BuildWzHeaderMap(szablon, dokument, wzItems);
            var wzItemMaps = wzItems.Select(BuildWzItemMap).ToList();

            return ext switch
            {
                ".html" or ".htm" => AddMeta(await RenderTextTemplateAsync(templatePath, outputBaseName, "text/html; charset=utf-8", text => RenderTemplate(text, wzHeaderMap, wzItemMaps)), usedFallback, infoMessage),
                ".txt" or ".md" => AddMeta(await RenderTextTemplateAsync(templatePath, outputBaseName, "text/plain; charset=utf-8", text => RenderTemplate(text, wzHeaderMap, wzItemMaps)), usedFallback, infoMessage),
                ".docx" => AddMeta(await RenderDocxTemplateAsync(templatePath, outputBaseName, wzHeaderMap, wzItemMaps), usedFallback, infoMessage),
                _ => throw new InvalidOperationException($"Nieobslugiwane rozszerzenie szablonu '{ext}'. Uzyj .html, .txt lub .docx.")
            };
        }

        public async Task<WydrukDokumentuResultDto> GenerujWydrukPzAsync(int idDokumentuPz, int? idSzablonu = null)
        {
            var dokument = await _context.DokumentPZ
                .AsNoTracking()
                .Include(x => x.Magazyn)
                .Include(x => x.Dostawca)
                .Include(x => x.Utworzyl)
                .Include(x => x.Pozycje).ThenInclude(x => x.Produkt).ThenInclude(x => x.DomyslnaJednostka)
                .Include(x => x.Pozycje).ThenInclude(x => x.Lokacja).ThenInclude(x => x.Magazyn)
                .Include(x => x.Pozycje).ThenInclude(x => x.Partia)
                .FirstOrDefaultAsync(x => x.Id == idDokumentuPz);

            if (dokument == null)
            {
                throw new InvalidOperationException($"Nie znaleziono dokumentu PZ o ID={idDokumentuPz}.");
            }

            var szablon = await PobierzSzablonAsync("PZ", idSzablonu);
            var templatePath = ResolveTemplatePath(szablon.Sciezka);
            var outputBaseName = BuildOutputBaseName("PZ", dokument.Numer);

            if (!File.Exists(templatePath))
            {
                var info = $"Nie znaleziono pliku szablonu '{szablon.Sciezka}'. Wygenerowano awaryjny wydruk DOCX.";
                return AddMeta(await GenerateFallbackPzDocxAsync(szablon, dokument, outputBaseName), true, info);
            }

            var ext = Path.GetExtension(templatePath).ToLowerInvariant();
            var pzItems = dokument.Pozycje.OrderBy(x => x.Lp).ThenBy(x => x.Id).ToList();
            var pzHeaderMap = BuildPzHeaderMap(szablon, dokument, pzItems);
            var pzItemMaps = pzItems.Select(BuildPzItemMap).ToList();
            return ext switch
            {
                ".html" or ".htm" => await RenderTextTemplateAsync(templatePath, outputBaseName, "text/html; charset=utf-8", text => RenderTemplate(text, pzHeaderMap, pzItemMaps)),
                ".txt" or ".md" => await RenderTextTemplateAsync(templatePath, outputBaseName, "text/plain; charset=utf-8", text => RenderTemplate(text, pzHeaderMap, pzItemMaps)),
                ".docx" => await RenderDocxTemplateAsync(templatePath, outputBaseName, pzHeaderMap, pzItemMaps),
                _ => throw new InvalidOperationException($"Nieobslugiwane rozszerzenie szablonu '{ext}'. Uzyj .html, .txt lub .docx.")
            };
        }

        public async Task<WydrukDokumentuResultDto> GenerujWydrukMmAsync(int idDokumentuMm, int? idSzablonu = null)
        {
            var dokument = await _context.DokumentMM
                .AsNoTracking()
                .Include(x => x.Magazyn)
                .Include(x => x.Utworzyl)
                .Include(x => x.Pozycje).ThenInclude(x => x.Produkt).ThenInclude(x => x.DomyslnaJednostka)
                .Include(x => x.Pozycje).ThenInclude(x => x.LokacjaZ).ThenInclude(x => x.Magazyn)
                .Include(x => x.Pozycje).ThenInclude(x => x.LokacjaDo).ThenInclude(x => x.Magazyn)
                .Include(x => x.Pozycje).ThenInclude(x => x.Partia)
                .FirstOrDefaultAsync(x => x.Id == idDokumentuMm);

            if (dokument == null)
            {
                throw new InvalidOperationException($"Nie znaleziono dokumentu MM o ID={idDokumentuMm}.");
            }

            var szablon = await PobierzSzablonAsync("MM", idSzablonu);
            var templatePath = ResolveTemplatePath(szablon.Sciezka);
            var outputBaseName = BuildOutputBaseName("MM", dokument.Numer);

            if (!File.Exists(templatePath))
            {
                var info = $"Nie znaleziono pliku szablonu '{szablon.Sciezka}'. Wygenerowano awaryjny wydruk DOCX.";
                return AddMeta(await GenerateFallbackMmDocxAsync(szablon, dokument, outputBaseName), true, info);
            }

            var ext = Path.GetExtension(templatePath).ToLowerInvariant();
            var mmItems = dokument.Pozycje.OrderBy(x => x.Lp).ThenBy(x => x.Id).ToList();
            var mmHeaderMap = BuildMmHeaderMap(szablon, dokument, mmItems);
            var mmItemMaps = mmItems.Select(BuildMmItemMap).ToList();
            return ext switch
            {
                ".html" or ".htm" => await RenderTextTemplateAsync(templatePath, outputBaseName, "text/html; charset=utf-8", text => RenderTemplate(text, mmHeaderMap, mmItemMaps)),
                ".txt" or ".md" => await RenderTextTemplateAsync(templatePath, outputBaseName, "text/plain; charset=utf-8", text => RenderTemplate(text, mmHeaderMap, mmItemMaps)),
                ".docx" => await RenderDocxTemplateAsync(templatePath, outputBaseName, mmHeaderMap, mmItemMaps),
                _ => throw new InvalidOperationException($"Nieobslugiwane rozszerzenie szablonu '{ext}'. Uzyj .html, .txt lub .docx.")
            };
        }

        private static WydrukDokumentuResultDto AddMeta(WydrukDokumentuResultDto result, bool usedFallback, string? infoMessage)
        {
            result.UzytoSzablonuAwaryjnego = usedFallback;
            result.KomunikatInformacyjny = infoMessage;

            if (usedFallback &&
                !string.IsNullOrWhiteSpace(infoMessage) &&
                result.ContentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) &&
                result.Content.Length > 0)
            {
                var html = Encoding.UTF8.GetString(result.Content);
                var banner = $"<div style=\"margin:12px 16px;padding:10px 12px;border:1px solid #f5c2c7;background:#f8d7da;color:#842029;border-radius:6px;font-family:Segoe UI,Arial,sans-serif;font-size:13px;\"><strong>Uzyto szablonu awaryjnego.</strong> {System.Net.WebUtility.HtmlEncode(infoMessage)}</div>";

                var bodyTagIndex = html.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
                if (bodyTagIndex >= 0)
                {
                    var bodyCloseIndex = html.IndexOf('>', bodyTagIndex);
                    if (bodyCloseIndex >= 0)
                    {
                        html = html.Insert(bodyCloseIndex + 1, banner);
                    }
                    else
                    {
                        html = banner + html;
                    }
                }
                else
                {
                    html = banner + html;
                }

                result.Content = Encoding.UTF8.GetBytes(html);
            }

            return result;
        }

        private async Task<SzablonWydruku> PobierzSzablonAsync(string typDokumentu, int? idSzablonu)
        {
            var query = _context.SzablonWydruku
                .AsNoTracking()
                .Where(x => x.TypDokumentu == typDokumentu);

            SzablonWydruku? szablon;

            if (idSzablonu.HasValue)
            {
                szablon = await query.FirstOrDefaultAsync(x => x.Id == idSzablonu.Value);
                if (szablon == null)
                {
                    throw new InvalidOperationException($"Nie znaleziono szablonu wydruku ID={idSzablonu.Value} dla typu '{typDokumentu}'.");
                }

                return szablon;
            }

            szablon = await query
                .Where(x => x.CzyAktywny)
                .OrderByDescending(x => x.WgranoUtc)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            if (szablon == null)
            {
                throw new InvalidOperationException($"Brak aktywnego szablonu wydruku dla typu '{typDokumentu}'.");
            }

            return szablon;
        }

        private static string ResolveTemplatePath(string configuredPath)
        {
            var path = (configuredPath ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidOperationException("Szablon nie ma ustawionej sciezki pliku.");
            }

            // Compatibility with legacy DB values like "/templates/wz/file.docx".
            // We map them to project-local "Templates/..." paths.
            var normalizedRelative = path.Replace('\\', '/').Trim();
            if (normalizedRelative.StartsWith("/templates/", StringComparison.OrdinalIgnoreCase))
            {
                normalizedRelative = "Templates/" + normalizedRelative["/templates/".Length..];
            }
            else if (normalizedRelative.StartsWith("templates/", StringComparison.OrdinalIgnoreCase))
            {
                normalizedRelative = "Templates/" + normalizedRelative["templates/".Length..];
            }

            if (Path.IsPathRooted(normalizedRelative))
            {
                return normalizedRelative;
            }

            var currentDirCandidate = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), normalizedRelative));
            if (File.Exists(currentDirCandidate))
            {
                return currentDirCandidate;
            }

            var baseDirCandidate = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, normalizedRelative));
            return baseDirCandidate;
        }

        private async Task<WydrukDokumentuResultDto> RenderTextTemplateAsync(
            string templatePath,
            string outputBaseName,
            string contentType,
            Func<string, string> render)
        {
            var templateText = await File.ReadAllTextAsync(templatePath, Encoding.UTF8);
            var rendered = render(templateText);
            var ext = Path.GetExtension(templatePath);
            var bytes = Encoding.UTF8.GetBytes(rendered);

            return new WydrukDokumentuResultDto
            {
                Content = bytes,
                ContentType = contentType,
                FileName = $"{outputBaseName}{ext}"
            };
        }

        private async Task<WydrukDokumentuResultDto> RenderDocxTemplateAsync(
            string templatePath,
            string outputBaseName,
            Func<string, string> render)
        {
            using var output = new MemoryStream();
            await using (var fileStream = File.OpenRead(templatePath))
            {
                await fileStream.CopyToAsync(output);
            }

            output.Position = 0;

            using (var wordDoc = WordprocessingDocument.Open(output, true))
            {
                var mainPart = wordDoc.MainDocumentPart
                    ?? throw new InvalidOperationException("Szablon DOCX nie zawiera MainDocumentPart.");

                await RenderTemplateInOpenXmlPartAsync(mainPart, render);

                foreach (var headerPart in mainPart.HeaderParts)
                {
                    await RenderTemplateInOpenXmlPartAsync(headerPart, render);
                }

                foreach (var footerPart in mainPart.FooterParts)
                {
                    await RenderTemplateInOpenXmlPartAsync(footerPart, render);
                }

                mainPart.Document?.Save();
            }

            return new WydrukDokumentuResultDto
            {
                Content = output.ToArray(),
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                FileName = $"{outputBaseName}.docx"
            };
        }

        private async Task<WydrukDokumentuResultDto> RenderDocxTemplateAsync(
            string templatePath,
            string outputBaseName,
            Dictionary<string, string> headerMap,
            List<Dictionary<string, string>> itemMaps)
        {
            using var output = new MemoryStream();
            await using (var fileStream = File.OpenRead(templatePath))
            {
                await fileStream.CopyToAsync(output);
            }

            output.Position = 0;

            using (var wordDoc = WordprocessingDocument.Open(output, true))
            {
                var mainPart = wordDoc.MainDocumentPart
                    ?? throw new InvalidOperationException("Szablon DOCX nie zawiera MainDocumentPart.");

                RenderTemplateInOpenXmlPart(mainPart, headerMap, itemMaps);

                foreach (var headerPart in mainPart.HeaderParts)
                {
                    RenderTemplateInOpenXmlPart(headerPart, headerMap, itemMaps);
                }

                foreach (var footerPart in mainPart.FooterParts)
                {
                    RenderTemplateInOpenXmlPart(footerPart, headerMap, itemMaps);
                }

                mainPart.Document?.Save();
            }

            return new WydrukDokumentuResultDto
            {
                Content = output.ToArray(),
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                FileName = $"{outputBaseName}.docx"
            };
        }

        private async Task RenderTemplateInOpenXmlPartAsync(OpenXmlPart part, Func<string, string> render)
        {
            await using var stream = part.GetStream(FileMode.Open, FileAccess.ReadWrite);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
            var xml = await reader.ReadToEndAsync();
            var renderedXml = render(xml);

            stream.SetLength(0);
            stream.Position = 0;

            await using var writer = new StreamWriter(stream, new UTF8Encoding(false), bufferSize: 1024, leaveOpen: true);
            await writer.WriteAsync(renderedXml);
            await writer.FlushAsync();
        }

        private void RenderTemplateInOpenXmlPart(
            OpenXmlPart part,
            Dictionary<string, string> headerMap,
            List<Dictionary<string, string>> itemMaps)
        {
            var root = part.RootElement;
            if (root == null)
            {
                return;
            }

            ExpandDocxItemLoops(root, headerMap, itemMaps);
            ReplacePlaceholdersInElement(root, headerMap);
            if (!headerMap.TryGetValue("Note", out var noteValue) || string.IsNullOrWhiteSpace(noteValue))
            {
                RemoveEmptyNoteSectionForStandardTemplates(root);
            }
            root.Save();
        }

        private void ExpandDocxItemLoops(
            OpenXmlElement root,
            Dictionary<string, string> headerMap,
            List<Dictionary<string, string>> itemMaps)
        {
            var rowsWithLoopMarkers = root
                .Descendants<TableRow>()
                .Where(ContainsLoopMarkers)
                .ToList();

            foreach (var templateRow in rowsWithLoopMarkers)
            {
                var parent = templateRow.Parent;
                if (parent == null)
                {
                    continue;
                }

                if (itemMaps.Count == 0)
                {
                    templateRow.Remove();
                    continue;
                }

                foreach (var itemMap in itemMaps)
                {
                    var clone = (TableRow)templateRow.CloneNode(true);
                    ReplacePlaceholdersInElement(clone, headerMap);
                    ReplacePlaceholdersInElement(clone, itemMap);
                    RemoveLoopMarkers(clone);
                    parent.InsertBefore(clone, templateRow);
                }

                templateRow.Remove();
            }
        }

        private static bool ContainsLoopMarkers(TableRow row)
        {
            var text = string.Concat(row.Descendants<Text>().Select(t => t.Text));
            return text.Contains("{{#Items}}", StringComparison.Ordinal) &&
                   text.Contains("{{/Items}}", StringComparison.Ordinal);
        }

        private static void RemoveLoopMarkers(OpenXmlElement element)
        {
            foreach (var paragraph in element.Descendants<Paragraph>())
            {
                var textNodes = paragraph.Descendants<Text>().ToList();
                if (textNodes.Count == 0)
                {
                    continue;
                }

                var current = string.Concat(textNodes.Select(x => x.Text ?? string.Empty));
                var cleaned = current
                    .Replace("{{#Items}}", string.Empty, StringComparison.Ordinal)
                    .Replace("{{/Items}}", string.Empty, StringComparison.Ordinal);

                if (cleaned == current)
                {
                    continue;
                }

                textNodes[0].Text = cleaned;
                for (var i = 1; i < textNodes.Count; i++)
                {
                    textNodes[i].Text = string.Empty;
                }
            }
        }

        private static void ReplacePlaceholdersInElement(OpenXmlElement element, Dictionary<string, string> map)
        {
            if (map.Count == 0)
            {
                return;
            }

            foreach (var paragraph in element.Descendants<Paragraph>())
            {
                var textNodes = paragraph.Descendants<Text>().ToList();
                if (textNodes.Count == 0)
                {
                    continue;
                }

                var current = string.Concat(textNodes.Select(x => x.Text ?? string.Empty));
                var replaced = current;

                foreach (var kv in map)
                {
                    replaced = replaced.Replace("{{" + kv.Key + "}}", kv.Value ?? string.Empty, StringComparison.Ordinal);
                }

                if (replaced == current)
                {
                    continue;
                }

                textNodes[0].Text = replaced;
                for (var i = 1; i < textNodes.Count; i++)
                {
                    textNodes[i].Text = string.Empty;
                }
            }
        }

        private static void RemoveEmptyNoteSectionForStandardTemplates(OpenXmlElement root)
        {
            var tables = root.Descendants<Table>().ToList();
            foreach (var table in tables)
            {
                var flattened = NormalizeTableText(table);
                if (flattened.Equals("Uwagi", StringComparison.OrdinalIgnoreCase) ||
                    flattened.StartsWith("Notatka", StringComparison.OrdinalIgnoreCase))
                {
                    RemoveAdjacentEmptyParagraphs(table);
                    table.Remove();
                }
            }
        }

        private static string NormalizeTableText(Table table)
        {
            var allText = string.Join(" ", table.Descendants<Text>().Select(x => x.Text ?? string.Empty));
            return Regex.Replace(allText, @"\s+", " ").Trim();
        }

        private static void RemoveAdjacentEmptyParagraphs(OpenXmlElement element)
        {
            while (element.PreviousSibling<Paragraph>() is Paragraph prev && IsBlankParagraph(prev))
            {
                prev.Remove();
            }

            while (element.NextSibling<Paragraph>() is Paragraph next && IsBlankParagraph(next))
            {
                next.Remove();
            }
        }

        private static bool IsBlankParagraph(Paragraph paragraph)
        {
            var text = string.Concat(paragraph.Descendants<Text>().Select(x => x.Text));
            return string.IsNullOrWhiteSpace(text);
        }

        private Task<WydrukDokumentuResultDto> GenerateFallbackWzDocxAsync(
            SzablonWydruku szablon,
            Data.Data.Magazyn.DokumentWZ dokument,
            string outputBaseName)
        {
            using var output = new MemoryStream();

            using (var wordDoc = WordprocessingDocument.Create(output, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document();

                var body = new Body();
                mainPart.Document.Append(body);

                body.Append(CreateParagraph("DOKUMENT WZ", bold: true, fontSizeHalfPoints: "56"));
                body.Append(CreateParagraph($"Numer dokumentu: {dokument.Numer}", bold: true, fontSizeHalfPoints: "32"));
                body.Append(CreateParagraph($"Status: {dokument.Status} | Wydano: {dokument.DataWydaniaUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}", fontSizeHalfPoints: "24"));
                body.Append(CreateSpacerParagraph(120));

                body.Append(CreateKeyValueTable(new[]
                {
                    ("Magazyn", dokument.Magazyn?.Nazwa ?? "-"),
                    ("ID magazynu", dokument.IdMagazynu.ToString(CultureInfo.InvariantCulture)),
                    ("Klient", dokument.Klient?.Nazwa ?? "-"),
                    ("Email klienta", dokument.Klient?.Email ?? "-"),
                    ("Telefon klienta", dokument.Klient?.Telefon ?? "-"),
                    ("Utworzyl", dokument.Utworzyl?.Email ?? dokument.Utworzyl?.Login ?? "-"),
                    ("Zaksiegowano", dokument.ZaksiegowanoUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "-"),
                    ("Wygenerowano", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                }, "Dane dokumentu"));

                body.Append(CreateSpacerParagraph(120));

                if (!string.IsNullOrWhiteSpace(dokument.Notatka))
                {
                    body.Append(CreateKeyValueTable(new[]
                    {
                        ("Notatka", dokument.Notatka!)
                    }, "Uwagi"));
                    body.Append(CreateSpacerParagraph(120));
                }

                var itemsTable = CreateItemsTable(dokument.Pozycje.OrderBy(x => x.Lp).ThenBy(x => x.Id).ToList());
                body.Append(itemsTable);

                body.Append(CreateSpacerParagraph(120));

                body.Append(CreateKeyValueTable(new[]
                {
                    ("Liczba pozycji", dokument.Pozycje.Count.ToString(CultureInfo.InvariantCulture)),
                    ("Suma ilosci", dokument.Pozycje.Sum(x => x.Ilosc).ToString("0.###", PlCulture)),
                    ("Szablon (rekord DB)", $"{szablon.Nazwa} v{szablon.Wersja}"),
                    ("Tryb", "Awaryjny wydruk DOCX (brak pliku szablonu na dysku)")
                }, "Podsumowanie"));

                body.Append(CreateSpacerParagraph(80));
                body.Append(CreateParagraph("Intelligent Warehouse | Wydruk awaryjny DOCX", fontSizeHalfPoints: "20", colorHex: "6B7280"));

                body.Append(CreateSectionPropertiesA4Landscape());

                mainPart.Document.Save();
            }

            return Task.FromResult(new WydrukDokumentuResultDto
            {
                Content = output.ToArray(),
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                FileName = $"{outputBaseName}.docx"
            });
        }

        private Task<WydrukDokumentuResultDto> GenerateFallbackPzDocxAsync(
            SzablonWydruku szablon,
            Data.Data.Magazyn.DokumentPZ dokument,
            string outputBaseName)
        {
            using var output = new MemoryStream();
            using (var wordDoc = WordprocessingDocument.Create(output, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document(new Body());
                var body = mainPart.Document.Body!;

                body.Append(CreateParagraph("DOKUMENT PZ", bold: true, fontSizeHalfPoints: "56"));
                body.Append(CreateParagraph($"Numer dokumentu: {dokument.Numer}", bold: true, fontSizeHalfPoints: "32"));
                body.Append(CreateParagraph($"Status: {dokument.Status} | Przyjecie: {dokument.DataPrzyjeciaUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}", fontSizeHalfPoints: "24"));
                body.Append(CreateSpacerParagraph(120));

                body.Append(CreateKeyValueTable(new[]
                {
                    ("Magazyn", dokument.Magazyn?.Nazwa ?? "-"),
                    ("Dostawca", dokument.Dostawca?.Nazwa ?? "-"),
                    ("Email dostawcy", dokument.Dostawca?.Email ?? "-"),
                    ("Telefon dostawcy", dokument.Dostawca?.Telefon ?? "-"),
                    ("NIP dostawcy", dokument.Dostawca?.NIP ?? "-"),
                    ("Utworzyl", dokument.Utworzyl?.Email ?? dokument.Utworzyl?.Login ?? "-"),
                    ("Zaksiegowano", dokument.ZaksiegowanoUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "-"),
                    ("Wygenerowano", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                }, "Dane dokumentu"));
                body.Append(CreateSpacerParagraph(120));

                if (!string.IsNullOrWhiteSpace(dokument.Notatka))
                {
                    body.Append(CreateKeyValueTable(new[] { ("Notatka", dokument.Notatka!) }, "Uwagi"));
                    body.Append(CreateSpacerParagraph(120));
                }

                body.Append(CreateItemsTablePz(dokument.Pozycje.OrderBy(x => x.Lp).ThenBy(x => x.Id).ToList()));
                body.Append(CreateSpacerParagraph(120));

                var sumaWartosci = dokument.Pozycje.Sum(x => (x.CenaJednostkowa ?? 0m) * x.Ilosc);
                body.Append(CreateKeyValueTable(new[]
                {
                    ("Liczba pozycji", dokument.Pozycje.Count.ToString(CultureInfo.InvariantCulture)),
                    ("Suma ilosci", dokument.Pozycje.Sum(x => x.Ilosc).ToString("0.###", PlCulture)),
                    ("Suma wartosci", sumaWartosci.ToString("0.00", PlCulture)),
                    ("Szablon (rekord DB)", $"{szablon.Nazwa} v{szablon.Wersja}"),
                    ("Tryb", "Awaryjny wydruk DOCX (brak pliku szablonu na dysku)")
                }, "Podsumowanie"));

                body.Append(CreateSectionPropertiesA4Landscape());
                mainPart.Document.Save();
            }

            return Task.FromResult(new WydrukDokumentuResultDto
            {
                Content = output.ToArray(),
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                FileName = $"{outputBaseName}.docx"
            });
        }

        private Task<WydrukDokumentuResultDto> GenerateFallbackMmDocxAsync(
            SzablonWydruku szablon,
            Data.Data.Magazyn.DokumentMM dokument,
            string outputBaseName)
        {
            using var output = new MemoryStream();
            using (var wordDoc = WordprocessingDocument.Create(output, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document(new Body());
                var body = mainPart.Document.Body!;

                body.Append(CreateParagraph("DOKUMENT MM", bold: true, fontSizeHalfPoints: "56"));
                body.Append(CreateParagraph($"Numer dokumentu: {dokument.Numer}", bold: true, fontSizeHalfPoints: "32"));
                body.Append(CreateParagraph($"Status: {dokument.Status} | Przesuniecie: {dokument.DataUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}", fontSizeHalfPoints: "24"));
                body.Append(CreateSpacerParagraph(120));

                body.Append(CreateKeyValueTable(new[]
                {
                    ("Magazyn", dokument.Magazyn?.Nazwa ?? "-"),
                    ("Utworzyl", dokument.Utworzyl?.Email ?? dokument.Utworzyl?.Login ?? "-"),
                    ("Zaksiegowano", dokument.ZaksiegowanoUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "-"),
                    ("Wygenerowano", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                }, "Dane dokumentu"));
                body.Append(CreateSpacerParagraph(120));

                if (!string.IsNullOrWhiteSpace(dokument.Notatka))
                {
                    body.Append(CreateKeyValueTable(new[] { ("Notatka", dokument.Notatka!) }, "Uwagi"));
                    body.Append(CreateSpacerParagraph(120));
                }

                body.Append(CreateItemsTableMm(dokument.Pozycje.OrderBy(x => x.Lp).ThenBy(x => x.Id).ToList()));
                body.Append(CreateSpacerParagraph(120));
                body.Append(CreateKeyValueTable(new[]
                {
                    ("Liczba pozycji", dokument.Pozycje.Count.ToString(CultureInfo.InvariantCulture)),
                    ("Suma ilosci", dokument.Pozycje.Sum(x => x.Ilosc).ToString("0.###", PlCulture)),
                    ("Szablon (rekord DB)", $"{szablon.Nazwa} v{szablon.Wersja}"),
                    ("Tryb", "Awaryjny wydruk DOCX (brak pliku szablonu na dysku)")
                }, "Podsumowanie"));

                body.Append(CreateSectionPropertiesA4Landscape());
                mainPart.Document.Save();
            }

            return Task.FromResult(new WydrukDokumentuResultDto
            {
                Content = output.ToArray(),
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                FileName = $"{outputBaseName}.docx"
            });
        }

        private static Paragraph CreateParagraph(
            string text,
            bool bold = false,
            string? fontSizeHalfPoints = null,
            string? colorHex = null,
            JustificationValues? justification = null)
        {
            var runProperties = new RunProperties();
            if (bold)
            {
                runProperties.Append(new Bold());
            }

            if (!string.IsNullOrWhiteSpace(fontSizeHalfPoints))
            {
                runProperties.Append(new FontSize { Val = fontSizeHalfPoints });
            }

            if (!string.IsNullOrWhiteSpace(colorHex))
            {
                runProperties.Append(new Color { Val = colorHex });
            }

            var paragraph = new Paragraph(
                new ParagraphProperties(
                    new SpacingBetweenLines { After = "80", Line = "240", LineRule = LineSpacingRuleValues.Auto }),
                new Run(
                    runProperties,
                    new Text(text ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve }));

            if (justification.HasValue)
            {
                var pPr = paragraph.GetFirstChild<ParagraphProperties>() ?? paragraph.PrependChild(new ParagraphProperties());
                pPr.Justification = new Justification { Val = justification.Value };
            }

            return paragraph;
        }

        private static Paragraph CreateSpacerParagraph(int after)
        {
            return new Paragraph(
                new ParagraphProperties(
                    new SpacingBetweenLines { After = after.ToString(CultureInfo.InvariantCulture) }),
                new Run(new Text(string.Empty)));
        }

        private static Table CreateKeyValueTable(IEnumerable<(string Key, string Value)> rows, string title)
        {
            var table = new Table();
            table.Append(CreateTableProperties(widthPct: "5000"));
            table.Append(CreateTableGrid(3500, 11500));

            table.Append(new TableRow(
                CreateTextCell(title, widthTwips: 15000, bold: true, fontSizeHalfPoints: "24", shadeHex: "EAF1FF", gridSpan: 2)));

            foreach (var (key, value) in rows)
            {
                var row = new TableRow(
                    CreateTextCell(key, widthTwips: 3500, bold: true, shadeHex: "F8FAFC", colorHex: "334155"),
                    CreateTextCell(value, widthTwips: 11500));
                table.Append(row);
            }

            return table;
        }

        private Table CreateItemsTable(IReadOnlyList<Data.Data.Magazyn.PozycjaWZ> items)
        {
            var table = new Table();
            table.Append(CreateTableProperties(widthPct: "5000"));
            table.Append(CreateTableGrid(700, 1800, 3800, 1500, 1300, 2500, 900, 700));

            table.Append(new TableRow(
                CreateTextCell("Lp", widthTwips: 600, bold: true, shadeHex: "DCE8FF", center: true),
                CreateTextCell("Kod produktu", widthTwips: 1700, bold: true, shadeHex: "DCE8FF"),
                CreateTextCell("Nazwa produktu", widthTwips: 3000, bold: true, shadeHex: "DCE8FF"),
                CreateTextCell("Partia", widthTwips: 1400, bold: true, shadeHex: "DCE8FF"),
                CreateTextCell("Lokacja", widthTwips: 1200, bold: true, shadeHex: "DCE8FF"),
                CreateTextCell("Magazyn lokacji", widthTwips: 2000, bold: true, shadeHex: "DCE8FF"),
                CreateTextCell("Ilosc", widthTwips: 900, bold: true, shadeHex: "DCE8FF", center: true),
                CreateTextCell("JM", widthTwips: 600, bold: true, shadeHex: "DCE8FF", center: true)));

            if (items.Count == 0)
            {
                table.Append(new TableRow(
                    CreateTextCell("Brak pozycji dokumentu.", 11400, colorHex: "6B7280", gridSpan: 8)));
                return table;
            }

            foreach (var item in items)
            {
                table.Append(new TableRow(
                    CreateTextCell(item.Lp.ToString(CultureInfo.InvariantCulture), widthTwips: 600, center: true),
                    CreateTextCell(item.Produkt?.Kod ?? "-", widthTwips: 1700),
                    CreateTextCell(item.Produkt?.Nazwa ?? "-", widthTwips: 3000),
                    CreateTextCell(item.Partia?.NumerPartii ?? "-", widthTwips: 1400),
                    CreateTextCell(item.Lokacja?.Kod ?? "-", widthTwips: 1200),
                    CreateTextCell(item.Lokacja?.Magazyn?.Nazwa ?? "-", widthTwips: 2000),
                    CreateTextCell(item.Ilosc.ToString("0.###", PlCulture), widthTwips: 900, center: true),
                    CreateTextCell(item.Produkt?.DomyslnaJednostka?.Kod ?? "j.m.", widthTwips: 600, center: true)));
            }

            return table;
        }

        private Table CreateItemsTablePz(IReadOnlyList<Data.Data.Magazyn.PozycjaPZ> items)
        {
            var table = new Table();
            table.Append(CreateTableProperties(widthPct: "5000"));
            table.Append(CreateTableGrid(600, 1600, 3600, 1400, 1300, 900, 600, 1000, 1200));

            table.Append(new TableRow(
                CreateTextCell("Lp", widthTwips: 500, bold: true, shadeHex: "DCE8FF", center: true),
                CreateTextCell("Kod", widthTwips: 1400, bold: true, shadeHex: "DCE8FF"),
                CreateTextCell("Nazwa produktu", widthTwips: 2500, bold: true, shadeHex: "DCE8FF"),
                CreateTextCell("Partia", widthTwips: 1200, bold: true, shadeHex: "DCE8FF"),
                CreateTextCell("Lokacja", widthTwips: 1100, bold: true, shadeHex: "DCE8FF"),
                CreateTextCell("Ilosc", widthTwips: 800, bold: true, shadeHex: "DCE8FF", center: true),
                CreateTextCell("JM", widthTwips: 500, bold: true, shadeHex: "DCE8FF", center: true),
                CreateTextCell("Cena", widthTwips: 900, bold: true, shadeHex: "DCE8FF", center: true),
                CreateTextCell("Wartosc", widthTwips: 1000, bold: true, shadeHex: "DCE8FF", center: true)));

            if (items.Count == 0)
            {
                table.Append(new TableRow(CreateTextCell("Brak pozycji dokumentu.", widthTwips: 10000, colorHex: "6B7280", gridSpan: 9)));
                return table;
            }

            foreach (var item in items)
            {
                var jm = item.Produkt?.DomyslnaJednostka?.Kod ?? "j.m.";
                var wartosc = (item.CenaJednostkowa ?? 0m) * item.Ilosc;
                table.Append(new TableRow(
                    CreateTextCell(item.Lp.ToString(CultureInfo.InvariantCulture), widthTwips: 500, center: true),
                    CreateTextCell(item.Produkt?.Kod ?? "-", widthTwips: 1400),
                    CreateTextCell(item.Produkt?.Nazwa ?? "-", widthTwips: 2500),
                    CreateTextCell(item.Partia?.NumerPartii ?? "-", widthTwips: 1200),
                    CreateTextCell(item.Lokacja?.Kod ?? "-", widthTwips: 1100),
                    CreateTextCell(item.Ilosc.ToString("0.###", PlCulture), widthTwips: 800, center: true),
                    CreateTextCell(jm, widthTwips: 500, center: true),
                    CreateTextCell((item.CenaJednostkowa ?? 0m).ToString("0.00", PlCulture), widthTwips: 900, center: true),
                    CreateTextCell(wartosc.ToString("0.00", PlCulture), widthTwips: 1000, center: true)));
            }

            return table;
        }

        private Table CreateItemsTableMm(IReadOnlyList<Data.Data.Magazyn.PozycjaMM> items)
        {
            var table = new Table();
            table.Append(CreateTableProperties(widthPct: "5000"));
            table.Append(CreateTableGrid(600, 1600, 3300, 1200, 1400, 1400, 900, 600));

            table.Append(new TableRow(
                CreateTextCell("Lp", widthTwips: 500, bold: true, shadeHex: "DCE8FF", center: true),
                CreateTextCell("Kod", widthTwips: 1400, bold: true, shadeHex: "DCE8FF"),
                CreateTextCell("Nazwa produktu", widthTwips: 2400, bold: true, shadeHex: "DCE8FF"),
                CreateTextCell("Partia", widthTwips: 1100, bold: true, shadeHex: "DCE8FF"),
                CreateTextCell("Lokacja Z", widthTwips: 1200, bold: true, shadeHex: "DCE8FF"),
                CreateTextCell("Lokacja DO", widthTwips: 1200, bold: true, shadeHex: "DCE8FF"),
                CreateTextCell("Ilosc", widthTwips: 800, bold: true, shadeHex: "DCE8FF", center: true),
                CreateTextCell("JM", widthTwips: 500, bold: true, shadeHex: "DCE8FF", center: true)));

            if (items.Count == 0)
            {
                table.Append(new TableRow(CreateTextCell("Brak pozycji dokumentu.", widthTwips: 9100, colorHex: "6B7280", gridSpan: 8)));
                return table;
            }

            foreach (var item in items)
            {
                var jm = item.Produkt?.DomyslnaJednostka?.Kod ?? "j.m.";
                table.Append(new TableRow(
                    CreateTextCell(item.Lp.ToString(CultureInfo.InvariantCulture), widthTwips: 500, center: true),
                    CreateTextCell(item.Produkt?.Kod ?? "-", widthTwips: 1400),
                    CreateTextCell(item.Produkt?.Nazwa ?? "-", widthTwips: 2400),
                    CreateTextCell(item.Partia?.NumerPartii ?? "-", widthTwips: 1100),
                    CreateTextCell(item.LokacjaZ?.Kod ?? "-", widthTwips: 1200),
                    CreateTextCell(item.LokacjaDo?.Kod ?? "-", widthTwips: 1200),
                    CreateTextCell(item.Ilosc.ToString("0.###", PlCulture), widthTwips: 800, center: true),
                    CreateTextCell(jm, widthTwips: 500, center: true)));
            }

            return table;
        }

        private static TableProperties CreateTableProperties(string widthPct)
        {
            return new TableProperties(
                new TableStyle { Val = "TableGrid" },
                new TableWidth { Width = widthPct, Type = TableWidthUnitValues.Pct },
                new TableLayout { Type = TableLayoutValues.Fixed },
                new TableLook
                {
                    FirstRow = OnOffValue.FromBoolean(true),
                    LastRow = OnOffValue.FromBoolean(false),
                    FirstColumn = OnOffValue.FromBoolean(false),
                    LastColumn = OnOffValue.FromBoolean(false),
                    NoHorizontalBand = OnOffValue.FromBoolean(false),
                    NoVerticalBand = OnOffValue.FromBoolean(true)
                },
                new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Color = "D1D5DB", Size = 4 },
                    new BottomBorder { Val = BorderValues.Single, Color = "D1D5DB", Size = 4 },
                    new LeftBorder { Val = BorderValues.Single, Color = "D1D5DB", Size = 4 },
                    new RightBorder { Val = BorderValues.Single, Color = "D1D5DB", Size = 4 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Color = "E5E7EB", Size = 2 },
                    new InsideVerticalBorder { Val = BorderValues.Single, Color = "E5E7EB", Size = 2 }));
        }

        private static TableGrid CreateTableGrid(params int[] widthsTwips)
        {
            var grid = new TableGrid();
            foreach (var width in widthsTwips)
            {
                grid.Append(new GridColumn { Width = width.ToString(CultureInfo.InvariantCulture) });
            }

            return grid;
        }

        private static TableCell CreateTextCell(
            string text,
            int widthTwips,
            bool bold = false,
            bool center = false,
            string? shadeHex = null,
            string? colorHex = null,
            string? fontSizeHalfPoints = "22",
            int gridSpan = 1)
        {
            var runProperties = new RunProperties();
            if (bold)
            {
                runProperties.Append(new Bold());
            }

            if (!string.IsNullOrWhiteSpace(fontSizeHalfPoints))
            {
                runProperties.Append(new FontSize { Val = fontSizeHalfPoints });
            }

            if (!string.IsNullOrWhiteSpace(colorHex))
            {
                runProperties.Append(new Color { Val = colorHex });
            }

            var paragraph = new Paragraph(
                new ParagraphProperties(
                    new SpacingBetweenLines { Before = "0", After = "0", Line = "240", LineRule = LineSpacingRuleValues.Auto },
                    new Justification { Val = center ? JustificationValues.Center : JustificationValues.Left }),
                new Run(
                    runProperties,
                    new Text(text ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve }));

            var cellProps = new TableCellProperties(
                new TableCellWidth { Width = widthTwips.ToString(CultureInfo.InvariantCulture), Type = TableWidthUnitValues.Dxa },
                new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center });

            if (!string.IsNullOrWhiteSpace(shadeHex))
            {
                cellProps.Append(new Shading { Val = ShadingPatternValues.Clear, Fill = shadeHex });
            }

            if (gridSpan > 1)
            {
                cellProps.Append(new GridSpan { Val = gridSpan });
            }

            return new TableCell(cellProps, paragraph);
        }

        private static SectionProperties CreateSectionPropertiesA4Landscape()
        {
            return new SectionProperties(
                new PageSize
                {
                    Width = 16838,  // A4 landscape
                    Height = 11906,
                    Orient = PageOrientationValues.Landscape
                },
                new PageMargin
                {
                    Top = 720,
                    Right = 720,
                    Bottom = 720,
                    Left = 720,
                    Header = 450,
                    Footer = 450,
                    Gutter = 0
                });
        }

        private string RenderWzTemplate(string template, SzablonWydruku szablon, Data.Data.Magazyn.DokumentWZ dokument)
        {
            var items = dokument.Pozycje.OrderBy(x => x.Lp).ThenBy(x => x.Id).ToList();
            return RenderTemplate(template, BuildWzHeaderMap(szablon, dokument, items), items.Select(BuildWzItemMap).ToList());
        }

        private string RenderPzTemplate(string template, SzablonWydruku szablon, Data.Data.Magazyn.DokumentPZ dokument)
        {
            var items = dokument.Pozycje.OrderBy(x => x.Lp).ThenBy(x => x.Id).ToList();
            return RenderTemplate(template, BuildPzHeaderMap(szablon, dokument, items), items.Select(BuildPzItemMap).ToList());
        }

        private string RenderMmTemplate(string template, SzablonWydruku szablon, Data.Data.Magazyn.DokumentMM dokument)
        {
            var items = dokument.Pozycje.OrderBy(x => x.Lp).ThenBy(x => x.Id).ToList();
            return RenderTemplate(template, BuildMmHeaderMap(szablon, dokument, items), items.Select(BuildMmItemMap).ToList());
        }

        private string RenderTemplate(
            string template,
            Dictionary<string, string> headerMap,
            List<Dictionary<string, string>> itemMaps)
        {
            var rendered = LoopRegex.Replace(template, match =>
            {
                var rowTemplate = match.Groups[1].Value;
                if (itemMaps.Count == 0)
                {
                    return string.Empty;
                }

                var sb = new StringBuilder();
                foreach (var itemMap in itemMaps)
                {
                    var row = rowTemplate;
                    foreach (var kv in itemMap)
                    {
                        row = row.Replace("{{" + kv.Key + "}}", kv.Value ?? string.Empty, StringComparison.Ordinal);
                    }

                    foreach (var kv in headerMap)
                    {
                        row = row.Replace("{{" + kv.Key + "}}", kv.Value ?? string.Empty, StringComparison.Ordinal);
                    }

                    sb.Append(row);
                }

                return sb.ToString();
            });

            foreach (var kv in headerMap)
            {
                rendered = rendered.Replace("{{" + kv.Key + "}}", kv.Value ?? string.Empty, StringComparison.Ordinal);
            }

            return rendered;
        }

        private Dictionary<string, string> BuildWzHeaderMap(
            SzablonWydruku szablon,
            Data.Data.Magazyn.DokumentWZ dokument,
            List<Data.Data.Magazyn.PozycjaWZ> items)
        {
            var sumaIlosci = items.Sum(x => x.Ilosc);

            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["TemplateName"] = szablon.Nazwa,
                ["TemplateVersion"] = szablon.Wersja,
                ["GeneratedAt"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", PlCulture),
                ["GeneratedAtUtc"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                ["DocType"] = "WZ",
                ["DocumentId"] = dokument.Id.ToString(CultureInfo.InvariantCulture),
                ["DocumentNo"] = dokument.Numer,
                ["Status"] = dokument.Status,
                ["IssuedAt"] = dokument.DataWydaniaUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", PlCulture),
                ["PostedAt"] = dokument.ZaksiegowanoUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", PlCulture) ?? "-",
                ["WarehouseName"] = dokument.Magazyn?.Nazwa ?? "-",
                ["WarehouseId"] = dokument.IdMagazynu.ToString(CultureInfo.InvariantCulture),
                ["CustomerName"] = dokument.Klient?.Nazwa ?? "-",
                ["CustomerEmail"] = dokument.Klient?.Email ?? "-",
                ["CustomerPhone"] = dokument.Klient?.Telefon ?? "-",
                ["CreatedBy"] = dokument.Utworzyl?.Email ?? dokument.Utworzyl?.Login ?? dokument.IdUtworzyl.ToString(CultureInfo.InvariantCulture),
                ["Note"] = dokument.Notatka ?? string.Empty,
                ["ItemsCount"] = items.Count.ToString(CultureInfo.InvariantCulture),
                ["TotalQty"] = FormatQuantity(sumaIlosci)
            };
        }

        private static Dictionary<string, string> BuildWzItemMap(Data.Data.Magazyn.PozycjaWZ item)
        {
            var jm = item.Produkt?.DomyslnaJednostka?.Kod ?? "j.m.";

            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ItemId"] = item.Id.ToString(CultureInfo.InvariantCulture),
                ["LineNo"] = item.Lp.ToString(CultureInfo.InvariantCulture),
                ["ProductId"] = item.IdProduktu.ToString(CultureInfo.InvariantCulture),
                ["ProductCode"] = item.Produkt?.Kod ?? "-",
                ["ProductName"] = item.Produkt?.Nazwa ?? "-",
                ["Quantity"] = FormatQuantity(item.Ilosc),
                ["QuantityWithUnit"] = $"{FormatQuantity(item.Ilosc)} {jm}",
                ["Unit"] = jm,
                ["LocationCode"] = item.Lokacja?.Kod ?? "-",
                ["LocationName"] = item.Lokacja?.Opis ?? "-",
                ["LocationWarehouse"] = item.Lokacja?.Magazyn?.Nazwa ?? "-",
                ["BatchNo"] = item.Partia?.NumerPartii ?? "-"
            };
        }

        private Dictionary<string, string> BuildPzHeaderMap(
            SzablonWydruku szablon,
            Data.Data.Magazyn.DokumentPZ dokument,
            List<Data.Data.Magazyn.PozycjaPZ> items)
        {
            var sumaIlosci = items.Sum(x => x.Ilosc);
            var sumaWartosci = items.Sum(x => (x.CenaJednostkowa ?? 0m) * x.Ilosc);

            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["TemplateName"] = szablon.Nazwa,
                ["TemplateVersion"] = szablon.Wersja,
                ["GeneratedAt"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", PlCulture),
                ["GeneratedAtUtc"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                ["DocType"] = "PZ",
                ["DocumentId"] = dokument.Id.ToString(CultureInfo.InvariantCulture),
                ["DocumentNo"] = dokument.Numer,
                ["Status"] = dokument.Status,
                ["IssuedAt"] = dokument.DataPrzyjeciaUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", PlCulture),
                ["PostedAt"] = dokument.ZaksiegowanoUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", PlCulture) ?? "-",
                ["WarehouseName"] = dokument.Magazyn?.Nazwa ?? "-",
                ["WarehouseId"] = dokument.IdMagazynu.ToString(CultureInfo.InvariantCulture),
                ["SupplierName"] = dokument.Dostawca?.Nazwa ?? "-",
                ["SupplierEmail"] = dokument.Dostawca?.Email ?? "-",
                ["SupplierPhone"] = dokument.Dostawca?.Telefon ?? "-",
                ["SupplierTaxId"] = dokument.Dostawca?.NIP ?? "-",
                ["CreatedBy"] = dokument.Utworzyl?.Email ?? dokument.Utworzyl?.Login ?? dokument.IdUtworzyl.ToString(CultureInfo.InvariantCulture),
                ["Note"] = dokument.Notatka ?? string.Empty,
                ["ItemsCount"] = items.Count.ToString(CultureInfo.InvariantCulture),
                ["TotalQty"] = FormatQuantity(sumaIlosci),
                ["TotalValue"] = FormatMoney(sumaWartosci)
            };
        }

        private static Dictionary<string, string> BuildPzItemMap(Data.Data.Magazyn.PozycjaPZ item)
        {
            var jm = item.Produkt?.DomyslnaJednostka?.Kod ?? "j.m.";
            var lineValue = (item.CenaJednostkowa ?? 0m) * item.Ilosc;
            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ItemId"] = item.Id.ToString(CultureInfo.InvariantCulture),
                ["LineNo"] = item.Lp.ToString(CultureInfo.InvariantCulture),
                ["ProductId"] = item.IdProduktu.ToString(CultureInfo.InvariantCulture),
                ["ProductCode"] = item.Produkt?.Kod ?? "-",
                ["ProductName"] = item.Produkt?.Nazwa ?? "-",
                ["Quantity"] = FormatQuantity(item.Ilosc),
                ["QuantityWithUnit"] = $"{FormatQuantity(item.Ilosc)} {jm}",
                ["Unit"] = jm,
                ["LocationCode"] = item.Lokacja?.Kod ?? "-",
                ["LocationName"] = item.Lokacja?.Opis ?? "-",
                ["LocationWarehouse"] = item.Lokacja?.Magazyn?.Nazwa ?? "-",
                ["BatchNo"] = item.Partia?.NumerPartii ?? "-",
                ["UnitPrice"] = FormatMoney(item.CenaJednostkowa ?? 0m),
                ["LineValue"] = FormatMoney(lineValue)
            };
        }

        private Dictionary<string, string> BuildMmHeaderMap(
            SzablonWydruku szablon,
            Data.Data.Magazyn.DokumentMM dokument,
            List<Data.Data.Magazyn.PozycjaMM> items)
        {
            var sumaIlosci = items.Sum(x => x.Ilosc);
            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["TemplateName"] = szablon.Nazwa,
                ["TemplateVersion"] = szablon.Wersja,
                ["GeneratedAt"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", PlCulture),
                ["GeneratedAtUtc"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                ["DocType"] = "MM",
                ["DocumentId"] = dokument.Id.ToString(CultureInfo.InvariantCulture),
                ["DocumentNo"] = dokument.Numer,
                ["Status"] = dokument.Status,
                ["IssuedAt"] = dokument.DataUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", PlCulture),
                ["PostedAt"] = dokument.ZaksiegowanoUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", PlCulture) ?? "-",
                ["WarehouseName"] = dokument.Magazyn?.Nazwa ?? "-",
                ["WarehouseId"] = dokument.IdMagazynu.ToString(CultureInfo.InvariantCulture),
                ["CreatedBy"] = dokument.Utworzyl?.Email ?? dokument.Utworzyl?.Login ?? dokument.IdUtworzyl.ToString(CultureInfo.InvariantCulture),
                ["Note"] = dokument.Notatka ?? string.Empty,
                ["ItemsCount"] = items.Count.ToString(CultureInfo.InvariantCulture),
                ["TotalQty"] = FormatQuantity(sumaIlosci)
            };
        }

        private static Dictionary<string, string> BuildMmItemMap(Data.Data.Magazyn.PozycjaMM item)
        {
            var jm = item.Produkt?.DomyslnaJednostka?.Kod ?? "j.m.";
            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ItemId"] = item.Id.ToString(CultureInfo.InvariantCulture),
                ["LineNo"] = item.Lp.ToString(CultureInfo.InvariantCulture),
                ["ProductId"] = item.IdProduktu.ToString(CultureInfo.InvariantCulture),
                ["ProductCode"] = item.Produkt?.Kod ?? "-",
                ["ProductName"] = item.Produkt?.Nazwa ?? "-",
                ["Quantity"] = FormatQuantity(item.Ilosc),
                ["QuantityWithUnit"] = $"{FormatQuantity(item.Ilosc)} {jm}",
                ["Unit"] = jm,
                ["FromLocationCode"] = item.LokacjaZ?.Kod ?? "-",
                ["FromWarehouseName"] = item.LokacjaZ?.Magazyn?.Nazwa ?? "-",
                ["ToLocationCode"] = item.LokacjaDo?.Kod ?? "-",
                ["ToWarehouseName"] = item.LokacjaDo?.Magazyn?.Nazwa ?? "-",
                ["BatchNo"] = item.Partia?.NumerPartii ?? "-"
            };
        }

        private static string SanitizeFileName(string value)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = new string((value ?? "document")
                .Select(ch => invalid.Contains(ch) ? '_' : ch)
                .ToArray());

            return string.IsNullOrWhiteSpace(sanitized) ? "document" : sanitized;
        }

        private static string BuildOutputBaseName(string docType, string documentNumber)
        {
            var normalizedType = (docType ?? string.Empty).Trim().ToUpperInvariant();
            var sanitizedNumber = SanitizeFileName(documentNumber);
            var numberUpper = sanitizedNumber.ToUpperInvariant();
            var hasTypePrefix =
                numberUpper.StartsWith($"{normalizedType}_", StringComparison.Ordinal) ||
                numberUpper.StartsWith($"{normalizedType}-", StringComparison.Ordinal);

            var core = hasTypePrefix ? sanitizedNumber : $"{normalizedType}_{sanitizedNumber}";
            return $"{core}_{DateTime.Now:yyyyMMdd_HHmmss}";
        }

        private static string FormatQuantity(decimal value) => value.ToString("0.00", PlCulture);

        private static string FormatMoney(decimal value) => value.ToString("0.00", PlCulture);
    }
}
