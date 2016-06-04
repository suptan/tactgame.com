using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using tactgame.com.Models;
using tactgame.com.Helpers;
using System.Configuration;
using System.Collections.Generic;
using System.IO;

namespace tactgame.com.Controllers
{
    public class AccountController : Controller
    {
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
                using(var writer = new CSVHelper.CsvFileWriter(PLAYERS_PATH))
                {
                    writer.AddRow(string.Format("{0},{1},{2},{3}", playerId, username, password, ConfigurationManager.AppSettings["investor"]));
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