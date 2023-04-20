using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Infraestructure
{
    public interface IRabbitMQManager
    {
        public void PublishMessage(string queueName, string message);
        List<string> GetMessagesFromQueue(string queueName, int count);

    }
}
