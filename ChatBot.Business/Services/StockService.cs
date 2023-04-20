using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ChatBot.Business.Interfaces;
using ChatBot.Business.Model;
using ChatBot.Infraestructure;
using CsvHelper;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ChatBot.Business.Services
{
    public class StockService : IStockService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IRabbitMQManager _rabbitMQManager;
        const string QUEUE_NAME = "chatbot";

        public StockService (IHttpClientFactory clientFactory, IRabbitMQManager rabbitMQManager)
        {
            _clientFactory = clientFactory;
            _rabbitMQManager = rabbitMQManager;
    }

        public async Task<StockModel> GetStockBySymbol(string symbol)
        {
            try
            {
                var client = _clientFactory.CreateClient();
                client.BaseAddress = new Uri($"https://stooq.com/q/l/?s={symbol}&f=sd2t2ohlcv&h&e=csv");

                var result = await client.GetAsync(client.BaseAddress);
                var content = await result.Content.ReadAsStringAsync();

                content = content.Replace("\n", ",");
                content = content.Replace("\r", string.Empty);

                var contentList = content.Split(',').Where(x => x != string.Empty).ToList();

                var headers = contentList.GetRange(0, 8);
                var values = contentList.GetRange(8, 8);

                var dict = new Dictionary<string, string>();
                for (int i = 0; i < values.Count; i++)
                {
                    dict.Add(headers[i], values[i]);
                }

                var stock = new StockModel()
                {
                    Symbol = dict.GetValueOrDefault("Symbol"),
                    Date = dict.GetValueOrDefault("Date"),
                    Time = dict.GetValueOrDefault("Time"),
                    Open = Convert.ToDouble(dict.GetValueOrDefault("Open"), CultureInfo.InvariantCulture),
                    High = Convert.ToDouble(dict.GetValueOrDefault("High"), CultureInfo.InvariantCulture),
                    Low = Convert.ToDouble(dict.GetValueOrDefault("Low"), CultureInfo.InvariantCulture),
                    Close = Convert.ToDouble(dict.GetValueOrDefault("Close"), CultureInfo.InvariantCulture),
                    Volume = Convert.ToInt32(dict.GetValueOrDefault("Volume"), CultureInfo.InvariantCulture)
                };

                var responseMessage = $"{stock.Symbol.ToUpper()} quote is {stock.Close} per share at {DateTime.Now.ToShortDateString}";

                _rabbitMQManager.PublishMessage(QUEUE_NAME, responseMessage);

                return stock;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<string> GetChatMessages(int count)
        {
            return _rabbitMQManager.GetMessagesFromQueue(QUEUE_NAME, count);
        }
    }
}