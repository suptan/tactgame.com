﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace tactgame.com.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Session["USER_ID"] = 0;
            Session["USER_NAME"] = "KhunTact";
            return View();
        }

        public ActionResult About()
        {
            Session["USER_ID"] = 0;
            Session["USER_NAME"] = "KhunTact";
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}