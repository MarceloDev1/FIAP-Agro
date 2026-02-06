namespace FIAP.Agro.AlertWorker.Domain;

public class AlertModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FieldId { get; set; }
    public string Type { get; set; } = "Drought";
    public string Message { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Open";
}