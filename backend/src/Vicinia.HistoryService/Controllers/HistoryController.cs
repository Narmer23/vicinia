using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Vicinia.HistoryService.Data;
using Vicinia.HistoryService.Models;

namespace Vicinia.HistoryService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly ILogger<HistoryController> _logger;
    private readonly HistoryDbContext _context;

    public HistoryController(ILogger<HistoryController> logger, HistoryDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserHistory(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting history for user: {UserId}", userId);

            var query = _context.SearchHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.SearchDate);

            var totalCount = await query.CountAsync();
            var histories = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new
            {
                Histories = histories,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting history for user: {UserId}", userId);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateHistory([FromBody] SearchHistory history)
    {
        try
        {
            _logger.LogInformation("Creating history entry for user: {UserId}", history.UserId);

            history.Id = Guid.NewGuid();
            history.SearchDate = DateTime.UtcNow;
            history.CreatedAt = DateTime.UtcNow;

            _context.SearchHistories.Add(history);
            await _context.SaveChangesAsync();

            _logger.LogInformation("History entry created with ID: {Id}", history.Id);
            return CreatedAtAction(nameof(GetHistory), new { id = history.Id }, history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating history entry");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetHistory(Guid id)
    {
        try
        {
            var history = await _context.SearchHistories.FindAsync(id);
            if (history == null)
            {
                return NotFound();
            }

            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting history with ID: {Id}", id);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHistory(Guid id)
    {
        try
        {
            var history = await _context.SearchHistories.FindAsync(id);
            if (history == null)
            {
                return NotFound();
            }

            _context.SearchHistories.Remove(history);
            await _context.SaveChangesAsync();

            _logger.LogInformation("History entry deleted with ID: {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting history with ID: {Id}", id);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }
} 