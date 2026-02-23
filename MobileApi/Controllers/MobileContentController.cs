using Data.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileApi.Models.Content;

namespace MobileApi.Controllers;

[ApiController]
[Route("api/mobile/content")]
public class MobileContentController : ControllerBase
{
    private readonly DataContext _context;

    public MobileContentController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("news")]
    [AllowAnonymous]
    public async Task<ActionResult<List<MobileNewsItemDto>>> GetNews()
    {
        var news = await _context.Aktualnosc
            .AsNoTracking()
            .OrderBy(x => x.Pozycja)
            .ThenBy(x => x.Nazwa)
            .Select(x => new MobileNewsItemDto
            {
                Id = x.IdAktualnosci,
                LinkTitle = x.TytulLinku,
                Title = x.Nazwa,
                Content = x.Tresc,
                Position = x.Pozycja
            })
            .ToListAsync();

        return Ok(news);
    }

    [HttpGet("pages")]
    [AllowAnonymous]
    public async Task<ActionResult<List<MobilePageListItemDto>>> GetPages()
    {
        var pages = await _context.Strona
            .AsNoTracking()
            .OrderBy(x => x.Pozycja)
            .ThenBy(x => x.Nazwa)
            .Select(x => new MobilePageListItemDto
            {
                Id = x.IdStrony,
                Slug = x.TytulLinku,
                Title = x.Nazwa,
                Position = x.Pozycja
            })
            .ToListAsync();

        return Ok(pages);
    }

    [HttpGet("pages/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<MobilePageDetailsDto>> GetPageBySlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return BadRequest("Slug jest wymagany.");
        }

        var normalized = slug.Trim();

        var page = await _context.Strona
            .AsNoTracking()
            .Where(x => x.TytulLinku == normalized)
            .Select(x => new MobilePageDetailsDto
            {
                Id = x.IdStrony,
                Slug = x.TytulLinku,
                Title = x.Nazwa,
                Content = x.Tresc,
                Position = x.Pozycja
            })
            .FirstOrDefaultAsync();

        if (page is null)
        {
            return NotFound(new { message = "Nie znaleziono strony CMS.", slug = normalized });
        }

        return Ok(page);
    }
}
