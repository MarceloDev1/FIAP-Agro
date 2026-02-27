using FIAP.Agro.AlertWorker.Domain;
using FIAP.Agro.AlertWorker.Infrastructure.Data;
using FIAP.Agro.AlertWorker.Infrastructure.Messaging;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FIAP.Agro.AlertWorker.Tests;

public class SensorReadingConsumerTests
{
    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        IServiceCollection serviceCollection = services.AddDbContext<AlertDbContext>(static _ =>
        {
            _.UseInMemoryDatabase(Guid.NewGuid().ToString());
        });
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Handle_Deve_Criar_Alerta_Quando_SoilMoisture_Abaixo_De_30()
    {
        var provider = CreateServiceProvider();

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        scopeMock.SetupGet(s => s.ServiceProvider).Returns(provider);
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();

        var consumer = new SensorReadingConsumer(scopeFactoryMock.Object, config);

        var evt = new SensorReadingCreatedEvent
        {
            FieldId = Guid.NewGuid(),
            SoilMoisture = 10
        };

        var handle = typeof(SensorReadingConsumer)
            .GetMethod("Handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        await (Task)handle.Invoke(consumer, new object[] { evt, CancellationToken.None })!;

        var db = provider.GetRequiredService<AlertDbContext>();
        db.Alerts.Should().ContainSingle(a => a.FieldId == evt.FieldId && a.Status == "Open");
    }
}