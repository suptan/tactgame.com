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
            Session["USER_ID"] = null;
            Session["USER_NAME"] = null;
            return View();
        }

        public ActionResult About()
        {
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