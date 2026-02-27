using FIAP.Agro.Identity.Controllers;
using FIAP.Agro.Identity.Data;
using FIAP.Agro.Identity.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FIAP.Agro.Identity.Tests;

public class AuthControllerTests
{
    private static IdentityDbContext CreateInMemoryDb()
    {
        var opts = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new IdentityDbContext(opts);
    }

    private static IConfiguration CreateConfig()
    {
        var dict = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "uma-chave-grande-com-no-minimo-32-caracteres-123456",
            ["Jwt:Issuer"] = "fiap-agro-identity",
            ["Jwt:Audience"] = "fiap-agro"
        }!;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict!)
            .Build();
    }

    [Fact]
    public async Task Register_Deve_Criar_Usuario_Quando_Email_Novo()
    {
        using var db = CreateInMemoryDb();
        var config = CreateConfig();
        var controller = new AuthController(db, config);

        var request = new RegisterRequest("user@test.com", "SenhaSegura123!");

        var result = await controller.Register(request);

        result.Should().BeOfType<OkResult>();
        db.Users.Should().ContainSingle(u => u.Email == request.Email);
    }

    [Fact]
    public async Task Login_Deve_Retornar_Unauthorized_Para_Credenciais_Invalidas()
    {
        using var db = CreateInMemoryDb();
        var config = CreateConfig();
        var controller = new AuthController(db, config);

        // usuário não existe
        var request = new LoginRequest("nao-existe@test.com", "senha");

        var result = await controller.Login(request);

        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}

