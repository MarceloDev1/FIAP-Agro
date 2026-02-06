namespace FIAP.Agro.Property.Domain.Entities
{
    public class FarmProperty
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OwnerId { get; set; } 
        public string Name { get; set; } = default!;
        public string? Location { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Field> Fields { get; set; } = new();
    }

}
