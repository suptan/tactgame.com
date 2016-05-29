using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using tactgame.com.Helpers;
using tactgame.com.Models;

namespace tactgame.com.Controllers
{
    [Authorize(Roles ="Administrators")]
    public class GMController : Controller
    {
        private List<string> searchAll = new List<string>();
        private string stockNamesPath = string.Format("{0}\\{1}", System.Web.HttpContext.Current.Server.MapPath("~/App_Data"), ConfigurationManager.AppSettings["companies"]);
        private string marketDataPath = string.Format("{0}\\{1}\\{2}", System.Web.HttpContext.Current.Server.MapPath("~/App_Data"), ConfigurationManager.AppSettings["boards"], ConfigurationManager.AppSettings["market"]);
        private string turnPath = string.Format("{0}\\{1}\\{2}", System.Web.HttpContext.Current.Server.MapPath("~/App_Data"), ConfigurationManager.AppSettings["boards"], ConfigurationManager.AppSettings["currentTurn"]);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Search for all stocks in DB.
        /// </summary>
        /// <returns>The list of stock.</returns>
        //[ObjectFilter(Param = "input", RootType = typeof(StockModel[]))]
        [HttpPost]
        public JsonResult StockListSearch()
        {
            var csvData = new List<StockModel>();
            var stockNames = new List<string>();
            var csvFloder = stockNamesPath;

            try
            {
                // Find all stocks name
                foreach (var files in Directory.GetFiles(csvFloder))
                {
                    FileInfo info = new FileInfo(files);
                    stockNames.Add(info.Name.Replace(".csv", string.Empty));
                }
                // Create json of stocks name
                foreach(var name in stockNames)
                {
                    csvData.Add(new StockModel(name.ToUpper()));
                }
            }
            catch (Exception ex)
            {
                return JSONHelper.CreateJSONResult(false, ex);
            }
            
            //return new ObjectResult<StockModel[]>(csvData.ToArray());
            return JSONHelper.CreateJSONResult(true, csvData);
        }

        /// <summary>
        /// Seach for available stock for trade.
        /// </summary>
        /// <param name="currentTurn"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult MarketSearch()
        {
            try
            {
                return JSONHelper.CreateJSONResult(true, GetMarketStocks());
            }
            catch (Exception ex)
            {
                return JSONHelper.CreateJSONResult(false, ex);
            }

        }

        [HttpPost]
        public JsonResult ChangeStockInMarket(string stockName, bool isAdd)
        {
            var csvData = new List<StockModel>();
            var csvFile = marketDataPath;

            try
            {
                if (isAdd)
                {
                    // Add new stock to market
                    using(CSVHelper.CsvFileWriter writer = new CSVHelper.CsvFileWriter(csvFile))
                    {
                        writer.AddRow(string.Format("0,{0},0,0", stockName.ToLower()));
                    }
                }
                else
                {
                    // Get all stock in market
                    var market = GetMarketStocks();
                    // Remove stock from market
                    var stockToRemove = market.Where(p => p.Name.Equals(stockName.ToUpper())).First();
                    market.Remove(stockToRemove);
                    // Rewrite market data
                    using(CSVHelper.CsvFileWriter writer = new CSVHelper.CsvFileWriter(csvFile, false))
                    {
                        // Add market csv header
                        writer.AddRow("id,name,price,dividend");

                        foreach(var item in market)
                        {
                            writer.AddRow(string.Format("0,{0},{1},{2}", item.Name, item.Price, item.Dividend));
                        }
                    }

                    // Read stocks data (price, dividend) at current turn
                    //using (CSVHelper.CsvFileReader reader = new CSVHelper.CsvFileReader(csvFile))
                    //{
                    //    while (reader.ReadRow(searchAll))
                    //    {
                    //        if (!searchAll.Contains("id"))
                    //        {
                    //            // Create stock model data from csv
                    //            csvData.Add(new StockModel(int.Parse(searchAll[0]), searchAll[1].ToUpper(), decimal.Parse(searchAll[2]), decimal.Parse(searchAll[3])));
                    //        }
                    //    }
                    //}
                }

                // Find current turn
                int? turn = null;
                csvFile = turnPath;
                using (CSVHelper.CsvFileReader reader = new CSVHelper.CsvFileReader(csvFile))
                {
                    while (reader.ReadRow(searchAll))
                    {
                        if (!searchAll.Contains("turn"))
                        {
                            turn = int.Parse(searchAll[0]);
                        }
                    }
                }

                //Update market value
                if (turn.HasValue)
                {
                    UpdateMarketData(turn.Value);

                    csvData = GetMarketStocks();
                }
            }
            catch (Exception ex)
            {
                return JSONHelper.CreateJSONResult(false, ex);
            }

            return JSONHelper.CreateJSONResult(true, csvData);
        }


        [HttpPost]
        public JsonResult PlayersSearch()
        {
            var csvData = new List<PlayerModel>();
            var csvFloder = System.Web.HttpContext.Current.Server.MapPath("~/App_Data") + "\\" + ConfigurationManager.AppSettings["players"];

            try
            {
                // Get player info
                foreach (var files in Directory.GetFiles(csvFloder))
                {
                    FileInfo info = new FileInfo(files);
                    var fileName = Path.GetFileName(info.FullName);
                    // Skip player account info
                    if (fileName.Equals("player.csv"))
                        continue;
                    // Player's portfolio path
                    var csvFile = string.Format("{0}\\{1}", csvFloder, fileName);
                    // Read player's portfolio data
                    using (CSVHelper.CsvFileReader reader = new CSVHelper.CsvFileReader(csvFile))
                    {
                        if (fileName.ToLower().Contains("stock"))
                        {
                            var stocks = new List<StockModel>();
                            var playerId = 0;

                            while (reader.ReadRow(searchAll))
                            {
                                if (!searchAll[0].ToLower().Equals("id"))
                                {
                                    stocks.Add(new StockModel(int.Parse(searchAll[0]),
                                    searchAll[1].ToUpper(),
                                    decimal.Parse(searchAll[2]),
                                    decimal.Parse(searchAll[3]),
                                    int.Parse(searchAll[4])));

                                    playerId = int.Parse(searchAll[5]);
                                }
                            }

                            csvData.Where(p => p.ID == playerId).First().Stocks = stocks;
                        }
                        else
                        {
                            while (reader.ReadRow(searchAll))
                            {
                                if (!searchAll[0].ToLower().Equals("id"))
                                {
                                    var player = new PlayerModel(int.Parse(searchAll[0]), searchAll[1], decimal.Parse(searchAll[2]), decimal.Parse(searchAll[3]));
                                    csvData.Add(player);
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                return JSONHelper.CreateJSONResult(false, ex);
            }

            return JSONHelper.CreateJSONResult(true, csvData);
        }

        #region Game Control

        [HttpPost]
        public JsonResult NextTurn()
        {
            int? turn = null;
            var csvFile = turnPath;

            try
            {
                // Get current game turn
                using (CSVHelper.CsvFileReader reader = new CSVHelper.CsvFileReader(csvFile))
                {
                    while (reader.ReadRow(searchAll))
                    {
                        if (!searchAll.Contains("turn"))
                        {
                            turn = int.Parse(searchAll[0]) + 1;
                        }
                    }
                }

                // Update game turn
                if (turn.HasValue)
                {
                    using (CSVHelper.CsvFileWriter writer = new CSVHelper.CsvFileWriter(csvFile, false))
                    {
                        // Write csv column
                        writer.AddRow("turn");
                        // Write turn data
                        writer.AddRow(turn.Value.ToString());
                    }

                    UpdateMarketData(turn.Value);
                }


            }
            catch (Exception ex)
            {
                return JSONHelper.CreateJSONResult(false, ex);
            }

            return JSONHelper.CreateJSONResult(true, turn);
        }

        [HttpPost]
        public JsonResult CurrentTurn()
        {
            int? turn = null;
            var csvFile = turnPath;

            try
            {
                // Find current turn
                using (CSVHelper.CsvFileReader reader = new CSVHelper.CsvFileReader(csvFile))
                {
                    while (reader.ReadRow(searchAll))
                    {
                        if (!searchAll.Contains("turn"))
                        {
                            turn = int.Parse(searchAll[0]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return JSONHelper.CreateJSONResult(false, ex);
            }

            return JSONHelper.CreateJSONResult(true, turn);
        }

        [HttpPost]
        public JsonResult ResetTurn()
        {
            int turn = 1;
            var csvFile = turnPath;

            try
            {
                // Reset game turn
                using (CSVHelper.CsvFileWriter writer = new CSVHelper.CsvFileWriter(csvFile, false))
                {
                    // Write csv column
                    writer.AddRow("turn");
                    // Write turn
                    writer.AddRow(turn.ToString());
                }

                UpdateMarketData(turn);
            }
            catch (Exception ex)
            {
                return JSONHelper.CreateJSONResult(false, ex);
            }

            return JSONHelper.CreateJSONResult(true, turn);
        }

        [HttpPost]
        public JsonResult MarketClose()
        {
            try
            {
                ChangeMarketStatus(0);
            }
            catch (Exception ex)
            {
                return JSONHelper.CreateJSONResult(false, ex);
            }

            return JSONHelper.CreateJSONResult(true, true);
        }

        [HttpPost]
        public JsonResult MarketOpen()
        {
            try
            {
                ChangeMarketStatus(1);
            }
            catch (Exception ex)
            {
                return JSONHelper.CreateJSONResult(false, ex);
            }

            return JSONHelper.CreateJSONResult(true, true);
        }

        #endregion

        #region Private Method

        private List<StockModel> GetMarketStocks()
        {
            var csvData = new List<StockModel>();
            var csvFile = marketDataPath;

            try
            {
                // Read stocks data (price, dividend) at current turn
                using (CSVHelper.CsvFileReader reader = new CSVHelper.CsvFileReader(csvFile))
                {
                    while (reader.ReadRow(searchAll))
                    {
                        if (!searchAll.Contains("id"))
                        {
                            // Create stock model data from csv
                            csvData.Add(new StockModel(int.Parse(searchAll[0]), searchAll[1].ToUpper(), decimal.Parse(searchAll[2]), decimal.Parse(searchAll[3])));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return csvData;
        }

        /// <summary>
        /// Update all stock data that in market
        /// </summary>
        /// <param name="turn">The board turn</param>
        private void UpdateMarketData(int turn)
        {
            turn--;
            //--------------------//
            // Update market data //
            //--------------------//
            var stockNames = new List<string>();
            var csvFile = marketDataPath;
            // Read current market stock
            using (CSVHelper.CsvFileReader reader = new CSVHelper.CsvFileReader(csvFile))
            {
                while (reader.ReadRow(searchAll))
                {
                    if (!searchAll.Contains("id"))
                    {
                        // Create stock model data from csv
                        //csvData.Add(new StockModel(int.Parse(searchAll[0]), searchAll[1].ToUpper(), decimal.Parse(searchAll[2]), decimal.Parse(searchAll[3])));
                        stockNames.Add(searchAll[1].ToUpper());
                    }
                }
            }
            // Update stocks price in market accoring to current turn
            var csvFloder = stockNamesPath;
            var stocksData = new List<string>();
            var stocksPriceAvg = new List<decimal>();
            var stocksDivAvg = new List<decimal>();
            // Find all stocks name
            foreach (var files in Directory.GetFiles(csvFloder))
            {
                FileInfo info = new FileInfo(files);
                var stockName = info.Name.Replace(".csv", string.Empty).ToUpper();

                if (stockNames.Contains(stockName))
                {
                    using (CSVHelper.CsvFileReader reader = new CSVHelper.CsvFileReader(info.FullName))
                    {
                        while (reader.ReadRow(searchAll))
                        {
                            if (!searchAll.Contains("id") && searchAll[0].Equals((turn).ToString()))
                            {
                                // Set stock id
                                searchAll[0] = stocksData.Count.ToString();
                                // Create stock model data from csv
                                stocksData.Add(string.Join(",", searchAll.ToArray()));
                                // Add stock price/dividend to find market index price/dividend
                                stocksPriceAvg.Add(decimal.Parse(searchAll[2]));
                                stocksDivAvg.Add(decimal.Parse(searchAll[3]));
                                break;
                            }
                        }
                    }

                }
            }
            // Calculate market index
            var indexPrice = stocksPriceAvg.Count == 0 ? 0 : (stocksPriceAvg.Sum() / stocksPriceAvg.Count);
            var indexDiv = stocksDivAvg.Count == 0 ? 0 : (stocksDivAvg.Sum() / stocksDivAvg.Count);
            var indexStock = string.Format("{0},index,{1},{2}", stocksData.Count, indexPrice, indexDiv);
            stocksData.Add(indexStock);
            // Write new market data
            using (CSVHelper.CsvFileWriter writer = new CSVHelper.CsvFileWriter(csvFile, false))
            {
                // Write csv column
                string marketCols = "id,name,price,dividend";
                //var row = new List<string>() { "id,name,price,dividend" };
                writer.AddRow(marketCols);

                foreach (var item in stocksData)
                {
                    writer.AddRow(item);
                }
            }

        }

        /// <summary>
        /// Change market close/open status
        /// </summary>
        /// <param name="status">0 - close, 1 - open</param>
        private void ChangeMarketStatus(int status)
        {
            // Find market status file
            var csvFile = string.Format("{0}\\{1}\\{2}", System.Web.HttpContext.Current.Server.MapPath("~/App_Data"), ConfigurationManager.AppSettings["boards"], ConfigurationManager.AppSettings["marketStatus"]);
            
            using (CSVHelper.CsvFileWriter writer = new CSVHelper.CsvFileWriter(csvFile, false))
            {
                var row = new List<string>();
                // Write csv column
                row.Add("status");
                writer.WriteRow(row);
                // Write turn data
                row.Clear();
                row.Add(status.ToString());
                writer.WriteRow(row);
            }

        }
        
        #endregion
    }
}