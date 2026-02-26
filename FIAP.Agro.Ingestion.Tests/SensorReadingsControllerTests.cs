using FIAP.Agro.Ingestion.Application.DTOs;
using FIAP.Agro.Ingestion.Controllers;
using FIAP.Agro.Ingestion.Infrastructure.Data;
using FIAP.Agro.Ingestion.Infrastructure.Messaging;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FIAP.Agro.Ingestion.Tests;

public class SensorReadingsControllerTests
{
    private static IngestionDbContext CreateInMemoryDb()
    {
        var opts = new DbContextOptionsBuilder<IngestionDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new IngestionDbContext(opts);
    }

    [Fact]
    public async Task Create_Deve_Validar_SoilMoisture()
    {
        using var db = CreateInMemoryDb();
        var publisherMock = new Mock<RabbitMqPublisher>(MockBehavior.Loose, new RabbitMqOptions());

        var controller = new SensorReadingsController(db, publisherMock.Object);

        var request = new CreateSensorReadingRequest(
            FieldId: Guid.NewGuid(),
            Timestamp: DateTime.UtcNow,
            SoilMoisture: -1,
            Temperature: 20,
            RainfallMm: 0
        );

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestObjectResult>();
        db.SensorReadings.Should().BeEmpty();
    }
}

