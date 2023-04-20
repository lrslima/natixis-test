using ChatBot.Business.Interfaces;
using ChatBot.Business.Model;
using ChatBot.Business.Services;
using ChatBot.Infraestructure;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ChatBot.Tests
{
    public class StockServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IRabbitMQManager> _rabbitMQManagerMock;
        private readonly IStockService _stockService;

        public StockServiceTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _rabbitMQManagerMock = new Mock<IRabbitMQManager>();

            _stockService = new StockService(_httpClientFactoryMock.Object, _rabbitMQManagerMock.Object);
        }

        [Fact]
        public async Task GetStockBySymbol_ReturnsStockModel_WhenSymbolIsValid()
        {
            // Arrange
            var symbol = "aapl.us";
            var handler = new MockedHttpMessageHandler("Symbol, Date, Time, Open, High, Low, Close, Volume\r\naapl.us, 2023 - 04 - 20, 19:38:12,166.09,167.87,165.89,167.6029,22267762\r\n");
            var httpClient = new HttpClient(handler);
            var expectedStock = new StockModel
            {
                Symbol = "aapl.us",
                Date = "2023-04-20",
                Time = "17:01:00",
                Open = 133.1,
                High = 133.1,
                Low = 133.1,
                Close = 133.1,
                Volume = 0
            };

            _httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() => httpClient);

            // Act
            var result = await _stockService.GetStockBySymbol(symbol);

            // Assert
            Assert.Equal(expectedStock.Symbol, result.Symbol);
        }
    }

    public class MockedHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _response;

        public MockedHttpMessageHandler(string response)
        {
            _response = response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_response),
            };

            return await Task.FromResult(response);
        }
    }
}
