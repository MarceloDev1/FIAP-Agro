namespace FIAP.Agro.Ingestion.Infrastructure.Messaging
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; } = "localhost";
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string Exchange { get; set; } = "agro.events";
        public string RoutingKey { get; set; } = "sensor.reading.created";
    }
}