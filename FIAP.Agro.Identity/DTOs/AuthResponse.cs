namespace FIAP.Agro.Identity.DTOs
{
    public record AuthResponse(string AccessToken, int ExpiresIn, Guid UserId, string Email, string Role);
}
