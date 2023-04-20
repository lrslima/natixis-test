using ChatBot.Business.Interfaces;
using ChatBot.Business.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace ChatBot.Web.Controllers
{
    public class ChatController : Controller
    {
        private readonly IStockService _stockService;
        public ChatController(IStockService stockService)
        {
            _stockService = stockService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string message)
        {

            var stockData = await _stockService.GetStockBySymbol(message);

            return Ok();
        }

        [HttpGet("chatMessages/{count}")]
        public async Task<IActionResult> ChatMessages(int count)
        {
            var messages = _stockService.GetChatMessages(count);

            return Json(messages);
        }
    }
}
