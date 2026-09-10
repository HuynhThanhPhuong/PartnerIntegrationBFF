using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Infrastructure.Messaging
{
    public class RabbitMqPublisher : ITransactionPublisher, IDisposable
    {
        private const string QueueName = "partner.transactions";
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        public RabbitMqPublisher(IConfiguration configuration)
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMq:HostName"],
                Port = int.Parse(configuration["RabbitMq:Port"]),
                UserName = configuration["RabbitMq:UserName"],
                Password = configuration["RabbitMq:Password"]
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
            _channel.QueueDeclareAsync(queue: QueueName, durable: true, exclusive: false, autoDelete: false)
                .GetAwaiter().GetResult();
        }

        public async Task Publish(Transaction transaction, CancellationToken cancellationToken = default)
        {
            var message = new TransactionMessage
            {
                Id = transaction.Id,
                PartnerId = transaction.PartnerId,
                TransactionReference = transaction.TransactionReference,
                Amount = transaction.Amount.Amount,
                Currency = transaction.Amount.Currency,
                Timestamp = transaction.Timestamp,
                Status = transaction.Status.ToString()
            };

            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            var properties = new BasicProperties
            {
                Persistent = true
            };

            await _channel.BasicPublishAsync(exchange: "", routingKey: QueueName, mandatory: false, basicProperties: properties, body: body, cancellationToken: cancellationToken);
        }

        public void Dispose()
        {
            _channel?.CloseAsync().GetAwaiter().GetResult();
            _connection?.CloseAsync().GetAwaiter().GetResult();
        }
    }
}
