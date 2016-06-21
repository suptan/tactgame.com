using System;
using System.Linq;
using System.Web.Mvc;
using tactgame.com.Helpers;
using System.Configuration;
using System.Collections.Generic;
using System.IO;

namespace tactgame.com.Controllers
{
    public class AccountController : Controller
    {
        private readonly string PLAYER_FOLDER = string.Format(@"{0}\{1}", System.Web.HttpContext.Current.Server.MapPath("~/App_Data"), ConfigurationManager.AppSettings["players"]);
        private readonly string PLAYERS_PATH = string.Format(@"{0}\{1}\{2}", System.Web.HttpContext.Current.Server.MapPath("~/App_Data"), ConfigurationManager.AppSettings["players"], ConfigurationManager.AppSettings["playersinfo"]);

        public ActionResult SignUp()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UserSignUp(string username, string password)
        {
            try
            {
                // Validate existing user
                var players = Directory.GetFiles(PLAYER_FOLDER);
                // Array contain must be exactly match
                //var playerExists = players.Contains(username);

                foreach (var name in players)
                {
                    if (name.Contains(username))
                        return JSONHelper.CreateJSONResult(false, "Username already exists."); 
                }
                // Lastest player id
                var playerId = -1;
                // Check lastest player id
                using (var reader = new CSVHelper.CsvFileReader(PLAYERS_PATH))
                {
                    var row = new List<string>();
                    while (reader.ReadRow(row))
                    {
                        if (!row.Contains("id"))
                        {
                            playerId = int.Parse(row[0]);
                        }
                    }
                }
                // New player id
                playerId++;
                using (var writer = new CSVHelper.CsvFileWriter(PLAYERS_PATH))
                {
                    writer.AddRow(string.Format("{0},{1},{2},{3}", playerId, username, password, ConfigurationManager.AppSettings["investor"]));
                }
                // Create player account
                var playerPath = string.Format(@"{0}\{1}.csv", PLAYER_FOLDER, username);
                System.IO.File.Create(playerPath).Dispose();

                using (var writer = new CSVHelper.CsvFileWriter(playerPath, false))
                {
                    // Header columns
                    writer.AddRow("id,name,cash,dividend,portfolio");
                    // Player data
                    writer.AddRow(string.Format("{0},{1},{2},{3},{4}", playerId, username, ConfigurationManager.AppSettings["startCash"], 0.00, 0.00));
                }
                // Create player portfolio
                var playerPortfolioPath = string.Format(@"{0}\{1}stock.csv", PLAYER_FOLDER, username);
                System.IO.File.Create(playerPortfolioPath).Dispose();

                using (var writer = new CSVHelper.CsvFileWriter(playerPortfolioPath, false))
                {
                    // Header columns
                    writer.AddRow("id,name,price,dividend,volumn,playerid");
                }
            }
            catch (Exception e)
            {
                return JSONHelper.CreateJSONResult(false, e.Message);
            }

            return JSONHelper.CreateJSONResult(true, "Register success, please login.");
        }
    }
}