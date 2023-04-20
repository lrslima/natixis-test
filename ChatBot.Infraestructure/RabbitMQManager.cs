using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Infraestructure
{
    public class RabbitMQManager : IRabbitMQManager
    {

        private readonly string _rabbitMQUri;

        public RabbitMQManager(IOptions<RabbitMQOptions> rabbitMQOptions)
        {
            _rabbitMQUri = rabbitMQOptions.Value.Uri;
        }

        public void PublishMessage(string queueName, string message)
        {
            using var connection = new ConnectionFactory() { Uri = new Uri(_rabbitMQUri) }.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: null,
                                 body: body);
        }

        public List<string> GetMessagesFromQueue(string queueName, int count)
        {
            var messages = new List<string>();

            using var connection = new ConnectionFactory() { Uri = new Uri(_rabbitMQUri) }.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                messages.Add(message);

                if (messages.Count > count)
                {
                    messages.RemoveAt(0);
                }
            };

            channel.BasicConsume(queue: queueName,
                                 autoAck: false,
                                 consumer: consumer);

            return messages;
        }

    }
}
