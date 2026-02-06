using RabbitMQ.Client;
using System.Text.Json;
using System.Text;

namespace FIAP.Agro.Ingestion.Infrastructure.Messaging
{
    public class RabbitMqPublisher
    {
        private readonly RabbitMqOptions _opt;

        public RabbitMqPublisher(RabbitMqOptions opt) => _opt = opt;

        public void Publish(object message)
        {
            var factory = new ConnectionFactory
            {
                HostName = _opt.HostName,
                UserName = _opt.UserName,
                Password = _opt.Password
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: _opt.Exchange, type: ExchangeType.Topic, durable: true);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var props = channel.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2;

            channel.BasicPublish(
                exchange: _opt.Exchange,
                routingKey: _opt.RoutingKey,
                basicProperties: props,
                body: body
            );
        }
    }
}