using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Agro.Property.Api.Controllers;
using FIAP.Agro.Property.Application.DTOs;
using FIAP.Agro.Property.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FIAP.Agro.Property.Tests;

public class PropertyControllerTests
{
    private static PropertyDbContext CreateInMemoryDb()
    {
        var opts = new DbContextOptionsBuilder<PropertyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PropertyDbContext(opts);
    }

    private static PropertyController CreateController(PropertyDbContext db, Guid ownerId)
    {
        var controller = new PropertyController(db);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, ownerId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        return controller;
    }

    [Fact]
    public async Task CreateProperty_Deve_Salvar_Propriedade_Com_OwnerId_Do_Token()
    {
        using var db = CreateInMemoryDb();
        var ownerId = Guid.NewGuid();
        var controller = CreateController(db, ownerId);

        var request = new CreatePropertyRequest("Fazenda Teste", "Local");

        var result = await controller.CreateProperty(request) as CreatedAtActionResult;

        result.Should().NotBeNull();
        db.Properties.Should().ContainSingle(p => p.OwnerId == ownerId && p.Name == request.Name);
    }
}

