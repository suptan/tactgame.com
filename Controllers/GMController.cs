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
    public class GMController : Controller
    {
        private List<string> searchAll = new List<string>();
        private string stockNamesPath = string.Format("{0}\\{1}", System.Web.HttpContext.Current.Server.MapPath("~/App_Data"), ConfigurationManager.AppSettings["companies"]);
        private string marketDataPath = string.Format("{0}\\{1}\\{2}", System.Web.HttpContext.Current.Server.MapPath("~/App_Data"), ConfigurationManager.AppSettings["boards"], ConfigurationManager.AppSettings["market"]);
        private string turnPath = string.Format("{0}\\{1}\\{2}", System.Web.HttpContext.Current.Server.MapPath("~/App_Data"), ConfigurationManager.AppSettings["boards"], ConfigurationManager.AppSettings["currentTurn"]);
        private readonly string PLAYER_FLODER = string.Format(@"{0}\{1}", System.Web.HttpContext.Current.Server.MapPath("~/App_Data"), ConfigurationManager.AppSettings["players"]);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public ActionResult Index()
        {
            //var verifyUser = CheckUnAuthorize();
            //if (verifyUser != null)
            //    return verifyUser;

            var userid = Session["USER_ID"].ToString();
            var username = Session["USER_NAME"].ToString();

            if (!string.IsNullOrEmpty(userid) && !string.IsNullOrEmpty(username))
            {
                // Check role
                using (var reader = new CSVHelper.CsvFileReader(string.Format(@"{0}\{1}\{2}", System.Web.HttpContext.Current.Server.MapPath("~/App_Data"), ConfigurationManager.AppSettings["players"], ConfigurationManager.AppSettings["playersinfo"])))
                {
                    var row = new List<string>();

                    while (reader.ReadRow(row))
                    {
                        if (!row.Contains("id"))
                        {
                            // Check user role
                            if (userid.Equals(row[0]) && username.Equals(row[1]) && row[3].Equals(ConfigurationManager.AppSettings["admin"]))
                                return View();
                        }
                    }
                }
            }
            return View("Error");
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
            var verifyUser = CheckUnAuthorize();
            if (verifyUser != null)
                return verifyUser;

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
            try
            {
                return JSONHelper.CreateJSONResult(true, GetPlayers());
            }
            catch (Exception ex)
            {
                return JSONHelper.CreateJSONResult(false, ex);
            }
        }

        #region Game Control

        [HttpPost]
        public JsonResult NextTurn()
        {
            var verifyUser = CheckUnAuthorize();
            if (verifyUser != null)
                return verifyUser;

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
                    // Update stocks price in market
                    UpdateMarketData(turn.Value);
                    // Pay dividend to player
                    PayDividend();
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
            var verifyUser = CheckUnAuthorize();
            if (verifyUser != null)
                return verifyUser;

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

        /// <summary>
        /// Get stock data (name, price, dividend) that trade in market.
        /// </summary>
        /// <returns>The stocks</returns>
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
        /// Get player data (cash and portfolio)
        /// </summary>
        /// <returns>The players</returns>
        private List<PlayerModel> GetPlayers()
        {
            var csvData = new List<PlayerModel>();
            var playerId = 0;

            try
            {
                // Get player info
                foreach (var files in Directory.GetFiles(PLAYER_FLODER))
                {
                    var info = new FileInfo(files);
                    var fileName = Path.GetFileName(info.FullName);
                    // Skip player account info
                    if (fileName.Equals("player.csv"))
                        continue;
                    // Player's portfolio path
                    var csvFile = string.Format("{0}\\{1}", PLAYER_FLODER, fileName);
                    // Read player's portfolio data
                    using (var reader = new CSVHelper.CsvFileReader(csvFile))
                    {
                        if (fileName.ToLower().Contains("stock"))
                        {
                            var stocks = new List<StockModel>();

                            while (reader.ReadRow(searchAll))
                            {
                                if (!searchAll[0].ToLower().Equals("id"))
                                {
                                    stocks.Add(new StockModel(int.Parse(searchAll[0]),
                                    searchAll[1].ToUpper(),
                                    decimal.Parse(searchAll[2]),
                                    decimal.Parse(searchAll[3]),
                                    int.Parse(searchAll[4])));
                                }
                            }
                            // Merge duplicate stocks which has different price
                            for (var i = 0; i < stocks.Count; i++)
                            {
                                var item = stocks[i];
                                // Find all stock in portfolio with the same id
                                var duplicateStocks = stocks.Where(p => p.Name.ToUpper() == item.Name.ToUpper());

                                if (stocks.Count() > 1)
                                {
                                    // Combine duplicate stock
                                    decimal newPrice = 0;
                                    var newVol = 0;
                                    foreach (var stock in duplicateStocks)
                                    {
                                        newPrice += stock.Price * stock.Vol;
                                        newVol += stock.Vol;
                                    }
                                    // Create new stock with avg cost
                                    var newStock = new StockModel(item.ID, item.Name, newPrice / newVol, 0, newVol);

                                    // Remove all duplicate stocks
                                    stocks.RemoveAll(p => p.Name.ToUpper() == item.Name.ToUpper());

                                    stocks.Add(newStock);
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
                                    playerId = player.ID;
                                }
                            }
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
            using (var reader = new CSVHelper.CsvFileReader(csvFile))
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
                    using (var reader = new CSVHelper.CsvFileReader(info.FullName))
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
            using (var writer = new CSVHelper.CsvFileWriter(csvFile, false))
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
        
        /// <summary>
        /// Pay dividend for each stocks
        /// </summary>
        private void PayDividend()
        {
            // Get stock dividend in market
            var stocks = GetMarketStocks();
            // Get player cash and portfolio
            var players = GetPlayers();
            // Pay dividend to each player that hold specific stock
            foreach(var stock in stocks)
            {
                foreach(var player in players)
                {
                    var portfolio = player.Stocks.Where(p => p.Name.ToLower().Equals(stock.Name.ToLower()));
                    var total = 0m;
                    // Update stock dividend in portfolio and calculate summation of dividend
                    foreach (var item in portfolio)
                    {
                        total += item.Vol * stock.Dividend;
                        item.Dividend = stock.Dividend;
                    }
                    //var total = portfolio.Sum(p => p.Vol) * stock.Dividend;
                    //
                    player.Cash += total;
                }
            }
            // Update player data
            foreach (var player in players)
            {
                var csvPath = string.Format(@"{0}\{1}.csv", PLAYER_FLODER, player.Name);
                using (var writer = new CSVHelper.CsvFileWriter(csvPath,false))
                {
                    // Header
                    writer.AddRow("id,name,cash,portfolio");
                    // New data
                    writer.AddRow(string.Format("{0},{1},{2},{3}",
                        player.ID,
                        player.Name,
                        player.Cash,
                        player.Portfolio));
                }
            }
        }

        #endregion

        #region Security

        private JsonResult CheckUnAuthorize()
        {
            // If access by not login redirect to login
            if (Session["USER_ID"] == null || Session["USER_NAME"] == null)
                return JSONHelper.CreateJSONResult(false, Constant.RedirectPath.HOME_URI);

            var userId = Session["USER_ID"].ToString();
            var row = new List<string>();
            var isRole = false;
            using (var reader = new CSVHelper.CsvFileReader(Constant.CsvFilesPath.PLAYER_PATH))
            {
                while (reader.ReadRow(row))
                {
                    if (!row.Contains("id"))
                    {
                        if (row[0].Equals(userId) && row[3].Equals(ConfigurationManager.AppSettings["admin"]))
                        {
                            isRole = true;
                            break;
                        }
                    }
                }
            }
            // Change the Result to point back to Home/Index
            if (!isRole)
            {
                return JSONHelper.CreateJSONResult(false, "");
            }

            return null;
        }

        #endregion
    }
}