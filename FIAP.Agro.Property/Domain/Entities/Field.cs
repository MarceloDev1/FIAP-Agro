namespace FIAP.Agro.Property.Domain.Entities
{

    public class Field
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PropertyId { get; set; }
        public FarmProperty Property { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Crop { get; set; } = default!;
        public string? GeoJson { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}