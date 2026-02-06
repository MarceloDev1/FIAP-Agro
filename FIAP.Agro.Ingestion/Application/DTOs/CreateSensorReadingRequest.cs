namespace FIAP.Agro.Ingestion.Application.DTOs;
public record CreateSensorReadingRequest(
    Guid FieldId,
    DateTime Timestamp,
    decimal SoilMoisture,
    decimal Temperature,
    decimal RainfallMm
);