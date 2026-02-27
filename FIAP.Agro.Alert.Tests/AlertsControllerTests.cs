using FIAP.Agro.Alert.Controllers;
using FIAP.Agro.Alert.Domain;
using FIAP.Agro.Alert.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FIAP.Agro.Alert.Tests;

public class AlertsControllerTests
{
    private static AlertDbContext CreateInMemoryDb()
    {
        var opts = new DbContextOptionsBuilder<AlertDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AlertDbContext(opts);
    }

    [Fact]
    public async Task GetOpen_Deve_Retornar_Apenas_Alertas_Abertos()
    {
        using var db = CreateInMemoryDb();
        db.Alerts.AddRange(
            new AlertModel { Status = "Open", Type = "Drought", Message = "A" },
            new AlertModel { Status = "Closed", Type = "Drought", Message = "B" }
        );
        await db.SaveChangesAsync();

        var controller = new AlertsController(db);

        var result = await controller.GetOpen() as OkObjectResult;

        result.Should().NotBeNull();
        var list = result!.Value as IEnumerable<AlertModel>;
        list.Should().NotBeNull();
        list!.Should().OnlyContain(a => a.Status == "Open");
    }
}

