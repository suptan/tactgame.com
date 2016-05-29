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
    public class PlayerController : Controller
    {
        private readonly string PORTFOLIO_FILE_FORMAT = "stock.csv";
        private readonly string PLAYER_NAMES_PATH = string.Format("{0}\\{1}", System.Web.HttpContext.Current.Server.MapPath("~/App_Data"), ConfigurationManager.AppSettings["players"]);
        private readonly string MARKET_PATH = string.Format("{0}\\{1}\\{2}", System.Web.HttpContext.Current.Server.MapPath("~/App_Data"), ConfigurationManager.AppSettings["boards"], ConfigurationManager.AppSettings["market"]);
        private readonly decimal COMMISSION_RATE = decimal.Parse(ConfigurationManager.AppSettings["commissionPercent"]);

        // Controller is stateless
        private string playerPortfolioPath;
        private string playerPath;

        // GET: Player
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Search current stock in portfolio by player name
        /// </summary>
        /// <returns>The portfolio</returns>
        public JsonResult PortfolioSearch()
        {
            // TEMP
            //Session["USER_ID"] = "1";
            //Session["USER_NAME"] = "playerA";
            //playerPortfolioPath = string.Format("{0}\\{1}{2}", PLAYER_NAMES_PATH, Session["USER_NAME"], PORTFOLIO_FILE_FORMAT);
            //playerPath = string.Format("{0}\\{1}.csv", PLAYER_NAMES_PATH, Session["USER_NAME"]);

            PlayerModel playerData = null;
            var csvData = new List<StockModel>();

            try
            {
                // Portfolio data
                csvData = GetPortfolio();
                // Get player data
                playerData = GetPlayerData();
                // Player's stock
                playerData.Stocks = csvData;

            }
            catch (Exception ex)
            {
                return JSONHelper.CreateJSONResult(false, ex);
            }
            
            return JSONHelper.CreateJSONResult(true, playerData);
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
                return JSONHelper.CreateJSONResult(true, GetStockMarketByName(null));
            }
            catch (Exception ex)
            {
                return JSONHelper.CreateJSONResult(false, ex);
            }

        }

        /// <summary>
        /// Player action to buy stocks in market
        /// </summary>
        /// <param name="id">The stock id</param>
        /// <param name="name">The stock name</param>
        /// <param name="vol">The stock volumns</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult BuyStock(int id, string name, int vol)
        {
            // Check if user already logged in
            //if (Session["username"] == null)
            //    return;

            // Check Buy/Sell volumns if less than 0 return error
            if (vol < 1)
            {
                return JSONHelper.CreateJSONResult(false, "The volumns cannot less than 100.");
            }
            
            var row = new List<string>();
            decimal cost = 0;
            StockModel stock = null;

            try
            {
                // Read stocks data (price, dividend) at current turn
                stock = GetStockMarketByName(name).First();
                // Calculate stock cost
                cost = stock.Price * vol;
                // Apply commission cost
                cost += cost * COMMISSION_RATE;
                // Find player data
                var player = GetPlayerData();
                // Check player cash
                // If cash not enough return error
                if(player.Cash - cost < 0)
                {
                    return JSONHelper.CreateJSONResult(false, "Not enough cash");
                }

                // Read player portfolio
                var playerPortfolio = GetPortfolio();
                
                // Stock info
                var newStock = GetStockMarketByName(name).First();
                // Add new stock to player protfolio
                playerPortfolioPath = string.Format("{0}\\{1}{2}", PLAYER_NAMES_PATH, Session["USER_NAME"], PORTFOLIO_FILE_FORMAT);
                using (CSVHelper.CsvFileWriter writer = new CSVHelper.CsvFileWriter(playerPortfolioPath))
                {
                    writer.AddRow(string.Format("{0},{1},{2},{3},{4},{5}", id, name.ToLower(), newStock.Price, 0.00, vol, Session["USER_ID"]));
                }

                // Update player cash
                //var player = GetPlayerData(name);
                player.Cash -= cost;
                // Update Portfolio value
                var total = 0m;
                foreach(var item in playerPortfolio)
                {
                    total += item.Price * item.Vol;
                }
                player.Portfolio = player.Cash + total;
                // Write new player data
                SavePlayerData(player);
            }
            catch (Exception ex)
            {
                return JSONHelper.CreateJSONResult(false, ex);
            }

            return JSONHelper.CreateJSONResult(true, "Trade success");
        }

        /// <summary>
        /// Player action to sell stock in portfolio
        /// </summary>
        /// <param name="id">The stock id</param>
        /// <param name="name">The stock name</param>
        /// <param name="vol">The stock volumns</param>
        /// <returns></returns>
        public JsonResult SellStock(int id, string name, int vol)
        {
            // Check if user already logged in
            //if (Session["username"] == null)
            //    return;

            // Check Buy/Sell volumns if less than 0 return error
            if (vol < 1)
            {
                return JSONHelper.CreateJSONResult(false, "The volumns cannot less than 100.");
            }
            
            var amount = 0m;
            var row = new List<string>();

            var stockMarket = GetStockMarketByName(name).First();
            // Get player portfolio every record
            var portfolio = GetPortfolio(false);

            // Find all stocks in portfolio with the same name
            var stocks = portfolio.Where(p => p.Name.ToLower().Equals(name.ToLower())).OrderBy(p => p.Price).ToList();
            // If sell volumns are more than existing stock return error
            if (stocks.Sum(p => p.Vol) - vol < 0)
            {
                return JSONHelper.CreateJSONResult(false, "Not enough stock for sell");
            }
            // Sell stock 
            foreach (var item in stocks)
            {
                // If sell volumns are more than exists volumns, delete record
                if (item.Vol - vol < 0)
                {
                    // Cash gain from selling stock
                    amount += stockMarket.Price * item.Vol;
                    vol -= item.Vol;
                    item.Vol = 0;
                }
                // There are volumns left after sell
                else
                {
                    // Cash gain from selling stock
                    amount += stockMarket.Price * vol;
                    item.Vol -= vol;
                    vol = 0;
                    break;
                }
            }
            // Apply commission rate when sell
            amount -= amount * COMMISSION_RATE;

            // Update player data
            var player = GetPlayerData();
            player.Cash += amount;
            player.Portfolio = portfolio.Sum(p => p.Price * p.Vol);
            // Save new player data
            SavePlayerData(player);

            // Update portfolio
            playerPortfolioPath = string.Format("{0}\\{1}{2}", PLAYER_NAMES_PATH, Session["USER_NAME"], PORTFOLIO_FILE_FORMAT);
            using (CSVHelper.CsvFileWriter writer = new CSVHelper.CsvFileWriter(playerPortfolioPath, false))
            {
                // Write file header
                writer.AddRow("id,name,price,dividend,volumn,playerid");
                // Update stocks in portfolio which have volumns more than 0
                foreach(var item in portfolio)
                {
                    if (item.Vol > 0)
                    {
                        writer.AddRow(string.Format("{0},{1},{2},{3},{4},{5}",
                                        0,
                                        item.Name.ToLower(),
                                        item.Price,
                                        item.Dividend,
                                        item.Vol,
                                        Session["USER_ID"])); 
                    }
                }
            }

            return JSONHelper.CreateJSONResult(true, "Trade success");
        }

        #region Private Methods

        /// <summary>
        /// Search all stock in portfolio by specific player name
        /// </summary>
        /// <returns>List of stock</returns>
        private List<StockModel> GetPortfolio(bool isRemoveDuplicate = true)
        {
            var csvData = new List<StockModel>();
            var row = new List<string>();

            try
            {
                playerPortfolioPath = string.Format("{0}\\{1}{2}", PLAYER_NAMES_PATH, Session["USER_NAME"], PORTFOLIO_FILE_FORMAT);
                var info = new FileInfo(playerPortfolioPath);
                if (info != null)
                {
                    // Get stock list
                    using (CSVHelper.CsvFileReader reader = new CSVHelper.CsvFileReader(playerPortfolioPath))
                    {
                        while (reader.ReadRow(row))
                        {
                            if (!row[0].ToLower().Equals("id"))
                            {
                                csvData.Add(new StockModel(int.Parse(row[0]),
                                row[1].ToUpper(),
                                decimal.Parse(row[2]),
                                decimal.Parse(row[3]),
                                int.Parse(row[4])));
                            }
                        }
                    }

                    // Remove duplicate stock and sum vol with average price
                    if (isRemoveDuplicate)
                    {
                        for (var i = 0; i < csvData.Count; i++)
                        {
                            var item = csvData[i];
                            // Find all stock in portfolio with the same id
                            var stocks = csvData.Where(p => p.Name.ToUpper() == item.Name.ToUpper());

                            if (stocks.Count() > 1)
                            {
                                // Combine duplicate stock
                                decimal newPrice = 0;
                                var newVol = 0;
                                foreach (var stock in stocks)
                                {
                                    newPrice += stock.Price;
                                    newVol += stock.Vol;
                                }
                                // Create new stock with avg cost
                                var newStock = new StockModel(item.ID, item.Name, (newPrice / stocks.Count()), 0, newVol);

                                // Remove all duplicate stocks
                                csvData.RemoveAll(p => p.Name.ToUpper() == item.Name.ToUpper());

                                csvData.Add(newStock);
                            }
                        } 
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return csvData.OrderBy(p => p.Name).ToList();
        }

        /// <summary>
        /// Search for tradeable stock in market
        /// </summary>
        /// <param name="id">Specific stock name or null for search all</param>
        /// <returns>The list of stocks</returns>
        private List<StockModel> GetStockMarketByName(string name)
        {
            var csvData = new List<StockModel>();
            var row = new CSVHelper.CsvRow();

            try
            {
                // Read stocks data (price, dividend) at current turn
                using (CSVHelper.CsvFileReader reader = new CSVHelper.CsvFileReader(MARKET_PATH))
                {
                    while (reader.ReadRow(row))
                    {
                        if (!row.Contains("id"))
                        {
                            var stockName = row[1].ToUpper();
                            // Create stock model data from csv
                            if (string.IsNullOrEmpty(name))
                            {
                                csvData.Add(new StockModel(int.Parse(row[0]), stockName, decimal.Parse(row[2]), decimal.Parse(row[3])));
                            }
                            else if (stockName.ToLower().Equals(name.ToLower()))
                            {
                                csvData.Add(new StockModel(int.Parse(row[0]), stockName, decimal.Parse(row[2]), decimal.Parse(row[3])));
                                break;
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
        /// Search player data (name, cash, portfolio value) by player name
        /// </summary>
        /// <returns>The player data</returns>
        private PlayerModel GetPlayerData()
        {
            PlayerModel result = null;
            var row = new List<string>();

            playerPath = string.Format("{0}\\{1}.csv", PLAYER_NAMES_PATH, Session["USER_NAME"]);
            using (CSVHelper.CsvFileReader reader = new CSVHelper.CsvFileReader(playerPath))
            {
                while (reader.ReadRow(row))
                {
                    if (!row[0].ToLower().Equals("id"))
                    {
                        result = new PlayerModel(int.Parse(row[0]), row[1], decimal.Parse(row[2]), decimal.Parse(row[3]));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Save player data
        /// </summary>
        /// <param name="player">The player data</param>
        private void SavePlayerData(PlayerModel player)
        {
            playerPath = string.Format("{0}\\{1}.csv", PLAYER_NAMES_PATH, Session["USER_NAME"]);
            using (CSVHelper.CsvFileWriter writer = new CSVHelper.CsvFileWriter(playerPath, false))
            {
                // Add file header column
                writer.AddRow("id,name,cash,portfolio");
                // New player data
                writer.AddRow(string.Format("{0},{1},{2},{3}", 
                    Session["USER_ID"], 
                    Session["USER_NAME"], 
                    player.Cash, 
                    player.Portfolio));
            }
        }

        #endregion
    }
}