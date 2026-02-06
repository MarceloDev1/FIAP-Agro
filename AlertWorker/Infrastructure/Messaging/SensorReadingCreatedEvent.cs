namespace FIAP.Agro.AlertWorker.Infrastructure.Messaging;

public class SensorReadingCreatedEvent
{
    public string EventType { get; set; } = default!;
    public Guid ReadingId { get; set; }
    public Guid FieldId { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal SoilMoisture { get; set; }
    public decimal Temperature { get; set; }
    public decimal RainfallMm { get; set; }
}