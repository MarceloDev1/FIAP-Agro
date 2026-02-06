using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FIAP.Agro.Property.Application.DTOs;
using FIAP.Agro.Property.Domain.Entities;
using FIAP.Agro.Property.Infrastructure.Data;

namespace Agro.Property.Api.Controllers;

[ApiController]
[Route("properties")]
[Authorize]
public class PropertyController : ControllerBase
{
    private readonly PropertyDbContext _db;

    public PropertyController(PropertyDbContext db) => _db = db;

    private Guid GetOwnerId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var ownerId))
            throw new UnauthorizedAccessException("Token inválido (sub ausente).");

        return ownerId;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProperty(CreatePropertyRequest request)
    {
        var ownerId = GetOwnerId();

        var property = new FarmProperty
        {
            OwnerId = ownerId,
            Name = request.Name,
            Location = request.Location
        };

        _db.Properties.Add(property);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPropertyById), new { propertyId = property.Id },
            new PropertyResponse(property.Id, property.Name, property.Location));
    }

    [HttpGet]
    public async Task<IActionResult> GetMyProperties()
    {
        var ownerId = GetOwnerId();

        var items = await _db.Properties.AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .Select(p => new PropertyResponse(p.Id, p.Name, p.Location))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{propertyId:guid}")]
    public async Task<IActionResult> GetPropertyById(Guid propertyId)
    {
        var ownerId = GetOwnerId();

        var property = await _db.Properties.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.OwnerId == ownerId);

        if (property == null) return NotFound();

        return Ok(new PropertyResponse(property.Id, property.Name, property.Location));
    }

    [HttpPost("{propertyId:guid}/fields")]
    public async Task<IActionResult> CreateField(Guid propertyId, CreateFieldRequest request)
    {
        var ownerId = GetOwnerId();

        var propertyExists = await _db.Properties
            .AnyAsync(p => p.Id == propertyId && p.OwnerId == ownerId);

        if (!propertyExists) return NotFound("Propriedade não encontrada.");

        var field = new Field
        {
            PropertyId = propertyId,
            Name = request.Name,
            Crop = request.Crop,
            GeoJson = request.GeoJson
        };

        _db.Fields.Add(field);
        await _db.SaveChangesAsync();

        return Ok(new { field.Id, field.PropertyId, field.Name, field.Crop, field.GeoJson });
    }

    [HttpGet("{propertyId:guid}/fields")]
    public async Task<IActionResult> GetFields(Guid propertyId)
    {
        var ownerId = GetOwnerId();

        var propertyExists = await _db.Properties
            .AnyAsync(p => p.Id == propertyId && p.OwnerId == ownerId);

        if (!propertyExists) return NotFound("Propriedade não encontrada.");

        var fields = await _db.Fields.AsNoTracking()
            .Where(f => f.PropertyId == propertyId)
            .Select(f => new { f.Id, f.Name, f.Crop, f.GeoJson, f.CreatedAt })
            .ToListAsync();

        return Ok(fields);
    }
}