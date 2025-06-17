using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Vicinia.LoggingService.Data;
using Vicinia.LoggingService.Models;

namespace Vicinia.LoggingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoggingController : ControllerBase
{
    private readonly ILogger<LoggingController> _logger;
    private readonly LoggingDbContext _context;

    public LoggingController(ILogger<LoggingController> logger, LoggingDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateLog([FromBody] LogEntry logEntry)
    {
        try
        {
            _logger.LogInformation("Creating log entry for service: {ServiceName}", logEntry.ServiceName);

            logEntry.Id = Guid.NewGuid();
            logEntry.Timestamp = DateTime.UtcNow;
            logEntry.CreatedAt = DateTime.UtcNow;

            _context.LogEntries.Add(logEntry);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Log entry created with ID: {Id}", logEntry.Id);
            return CreatedAtAction(nameof(GetLog), new { id = logEntry.Id }, logEntry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating log entry");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs(
        [FromQuery] string? serviceName = null,
        [FromQuery] string? logLevel = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = _context.LogEntries.AsQueryable();

            if (!string.IsNullOrEmpty(serviceName))
                query = query.Where(l => l.ServiceName == serviceName);

            if (!string.IsNullOrEmpty(logLevel))
                query = query.Where(l => l.LogLevel == logLevel);

            if (fromDate.HasValue)
                query = query.Where(l => l.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.Timestamp <= toDate.Value);

            query = query.OrderByDescending(l => l.Timestamp);

            var totalCount = await query.CountAsync();
            var logs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new
            {
                Logs = logs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting logs");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLog(Guid id)
    {
        try
        {
            var log = await _context.LogEntries.FindAsync(id);
            if (log == null)
            {
                return NotFound();
            }

            return Ok(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting log with ID: {Id}", id);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpGet("service/{serviceName}")]
    public async Task<IActionResult> GetServiceLogs(string serviceName, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = _context.LogEntries
                .Where(l => l.ServiceName == serviceName)
                .OrderByDescending(l => l.Timestamp);

            var totalCount = await query.CountAsync();
            var logs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new
            {
                Logs = logs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting logs for service: {ServiceName}", serviceName);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }
} 