using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using tactgame.com.Helpers;

namespace tactgame.com.Controllers
{
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Login validation
        /// </summary>
        /// <param name="username">The input username</param>
        /// <param name="password">The input password</param>
        /// <returns></returns>
        //public ActionResult UserLogin(string username, string password)
        [HttpPost]
        public JsonResult UserLogin(string username, string password)
        {
            string userPattern = @"\w{6,10}";
            string passPattern = @"\w{6,20}";
            string loginError = "Username and Password incorrect";
            // Validate username and password pattern
            if (!Regex.Match(username, userPattern).Success || !Regex.Match(password, passPattern).Success)
                return JSONHelper.CreateJSONResult(false, loginError);

            var page = string.Empty;

            try
            {
                var path = string.Format("{0}\\{1}\\{2}", System.Web.HttpContext.Current.Server.MapPath("~/App_Data"), ConfigurationManager.AppSettings["players"], ConfigurationManager.AppSettings["playersinfo"]);
                var row = new List<string>();
                // Read user registered data
                using (var reader = new CSVHelper.CsvFileReader(path))
                {
                    while (reader.ReadRow(row))
                    {
                        if (!row.Contains("id"))
                        {
                            if (username.Equals(row[1]) && password.Equals(row[2]))
                            {
                                // Check user role
                                if (row[3].Equals(ConfigurationManager.AppSettings["admin"]))
                                    page = @"GM";
                                else
                                    page = @"Player";

                                // Set seesion profile
                                Session["USER_ID"] = int.Parse(row[0]);
                                Session["USER_NAME"] = row[1];

                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return JSONHelper.CreateJSONResult(false, ex.Message);
            }

            if(string.IsNullOrEmpty(page))
                return JSONHelper.CreateJSONResult(false, "Username and Password are not matched!!!");

            return JSONHelper.CreateJSONResult(true, page);
        }

        /// <summary>
        /// Log off
        /// </summary>
        /// <returns></returns>
        public ActionResult Logoff()
        {
            Session["USER_ID"] = null;
            Session["USER_NAME"] = null;
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// 
        /// </summary>
        public ActionResult Register()
        {
            return RedirectToAction("Signup", "Account");
        }
    }
}