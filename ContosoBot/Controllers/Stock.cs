using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ContosoBot.Controllers
{
    public class Stock : ApiController
    {
        public static async Task<string> GetStock(string StockSymbol)
        {
            double? dblStockValue = await StockYahoo.GetStockRateAsync(StockSymbol);
            if (dblStockValue == null)
            {
                //return string.Format("This \"{0}\" is not an valid stock symbol", StockSymbol);
                return null; // invalid stock symbol
            }
            else
            {
                return string.Format("{0},{1}", StockSymbol, dblStockValue);
            }
        }
    }
}