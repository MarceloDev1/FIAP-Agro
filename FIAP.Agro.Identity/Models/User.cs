namespace FIAP.Agro.Identity.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string Role { get; set; } = "Produtor";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}