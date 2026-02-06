namespace FIAP.Agro.Property.Application.DTOs;


public record CreateFieldRequest(string Name, string Crop, string? GeoJson);