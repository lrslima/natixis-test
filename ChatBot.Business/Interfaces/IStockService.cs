using ChatBot.Business.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Business.Interfaces
{
    public interface IStockService
    {
        public Task<StockModel> GetStockBySymbol(string symbol);
        public List<string> GetChatMessages(int count);
    }
}
