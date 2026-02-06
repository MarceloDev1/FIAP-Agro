using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using FIAP.Agro.AlertWorker.Infrastructure.Data;
using FIAP.Agro.AlertWorker.Domain;

namespace FIAP.Agro.AlertWorker.Infrastructure.Messaging;

public class SensorReadingConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;

    private IConnection? _connection;
    private IModel? _channel;

    public SensorReadingConsumer(IServiceScopeFactory scopeFactory, IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _config = config;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var host = _config["RabbitMq:HostName"] ?? "localhost";
        var user = _config["RabbitMq:UserName"] ?? "guest";
        var pass = _config["RabbitMq:Password"] ?? "guest";
        var queue = _config["RabbitMq:Queue"] ?? "sensor.readings";

        var factory = new ConnectionFactory
        {
            HostName = host,
            UserName = user,
            Password = pass,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection("fiap-agro-alert-worker");
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false);
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queue = _config["RabbitMq:Queue"] ?? "sensor.readings";

        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                var evt = JsonSerializer.Deserialize<SensorReadingCreatedEvent>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (evt == null)
                {
                    _channel!.BasicAck(ea.DeliveryTag, multiple: false);
                    return;
                }

                await Handle(evt, stoppingToken);

                _channel!.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch
            {
                _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel!.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    private async Task Handle(SensorReadingCreatedEvent evt, CancellationToken ct)
    {
        if (evt.SoilMoisture >= 30) return;

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AlertDbContext>();

        var existsOpen = await db.Alerts.AnyAsync(a =>
            a.FieldId == evt.FieldId &&
            a.Type == "Drought" &&
            a.Status == "Open", ct);

        if (existsOpen) return;

        db.Alerts.Add(new AlertModel
        {
            FieldId = evt.FieldId,
            Type = "Drought",
            Status = "Open",
            Message = $"Alerta de seca: umidade do solo {evt.SoilMoisture}% (limiar 30%)."
        });

        await db.SaveChangesAsync(ct);
    }

    public override void Dispose()
    {
        try { _channel?.Close(); } catch { }
        try { _connection?.Close(); } catch { }
        base.Dispose();
    }
}
