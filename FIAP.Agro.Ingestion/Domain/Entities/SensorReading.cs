namespace FIAP.Agro.Ingestion.Domain.Entities
{
    public class SensorReading
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid FieldId { get; set; }          // vem do Property (talhão)
        public DateTime Timestamp { get; set; }    // momento da leitura

        public decimal SoilMoisture { get; set; }  // % (0-100)
        public decimal Temperature { get; set; }   // °C
        public decimal RainfallMm { get; set; }    // mm

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
