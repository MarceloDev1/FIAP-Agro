using FIAP.Agro.Alert.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FIAP.Agro.Alert.Controllers;

[ApiController]
[Route("alerts")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly AlertDbContext _db;

    public AlertsController(AlertDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? type)
    {
        var q = _db.Alerts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(a => a.Status == status);

        if (!string.IsNullOrWhiteSpace(type))
            q = q.Where(a => a.Type == type);

        var result = await q
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("open")]
    public async Task<IActionResult> GetOpen()
    {
        var result = await _db.Alerts
            .Where(a => a.Status == "Open")
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("by-field/{fieldId:guid}")]
    public async Task<IActionResult> GetByField(Guid fieldId)
    {
        var result = await _db.Alerts
            .Where(a => a.FieldId == fieldId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(result);
    }

    [HttpPut("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id)
    {
        var alert = await _db.Alerts.FirstOrDefaultAsync(a => a.Id == id);
        if (alert is null) return NotFound();

        alert.Status = "Closed";
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
