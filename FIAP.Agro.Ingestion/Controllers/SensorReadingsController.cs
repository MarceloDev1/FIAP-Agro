using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FIAP.Agro.Ingestion.Application.DTOs;
using FIAP.Agro.Ingestion.Domain.Entities;
using FIAP.Agro.Ingestion.Infrastructure.Data;
using FIAP.Agro.Ingestion.Infrastructure.Messaging;

using Microsoft.AspNetCore.Mvc;

namespace FIAP.Agro.Ingestion.Controllers;

[ApiController]
[Route("")]
[Authorize]
public class SensorReadingsController : ControllerBase
{
    private readonly IngestionDbContext _db;
    private readonly RabbitMqPublisher _publisher;

    public SensorReadingsController(IngestionDbContext db, RabbitMqPublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    [HttpPost("sensor-readings")]
    public async Task<IActionResult> Create(CreateSensorReadingRequest request)
    {
        // validações MVP (leves)
        if (request.SoilMoisture < 0 || request.SoilMoisture > 100)
            return BadRequest("SoilMoisture deve estar entre 0 e 100.");

        var reading = new SensorReading
        {
            FieldId = request.FieldId,
            Timestamp = request.Timestamp == default ? DateTime.UtcNow : request.Timestamp,
            SoilMoisture = request.SoilMoisture,
            Temperature = request.Temperature,
            RainfallMm = request.RainfallMm
        };

        _db.SensorReadings.Add(reading);
        await _db.SaveChangesAsync();

        // Evento para Alert Service
        _publisher.Publish(new
        {
            eventType = "SensorReadingCreated",
            readingId = reading.Id,
            fieldId = reading.FieldId,
            timestamp = reading.Timestamp,
            soilMoisture = reading.SoilMoisture,
            temperature = reading.Temperature,
            rainfallMm = reading.RainfallMm
        });

        return Ok(new
        {
            reading.Id,
            reading.FieldId,
            reading.Timestamp
        });
    }

    [HttpGet("fields/{fieldId:guid}/sensor-readings")]
    public async Task<IActionResult> GetByField(Guid fieldId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var query = _db.SensorReadings.AsNoTracking().Where(x => x.FieldId == fieldId);

        if (from.HasValue) query = query.Where(x => x.Timestamp >= from.Value);
        if (to.HasValue) query = query.Where(x => x.Timestamp <= to.Value);

        var items = await query
            .OrderBy(x => x.Timestamp)
            .Select(x => new
            {
                x.Timestamp,
                x.SoilMoisture,
                x.Temperature,
                x.RainfallMm
            })
            .ToListAsync();

        return Ok(items);
    }
}