using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Routing;

namespace tactgame.com.Helpers
{
    public class FullControlMarket : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (string.IsNullOrEmpty(filterContext.HttpContext.Session["USER_ID"] as string))
            {
                // Change the Result to point back to Home/Index
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Home" }, { "action", "Index" } });
                
            }
            else
            {
                var userId = filterContext.HttpContext.Session["USER_ID"].ToString();

                if (string.IsNullOrEmpty(userId))
                {
                    // Change the Result to point back to Home/Index
                    filterContext.Result = RedirectHome();
                }
                else // We have selected a valid user
                {
                    //var gmIds = new List<int>();
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
                        filterContext.Result = RedirectHome();
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// Change the Result to point back to Home/Index
        /// </summary>
        /// <returns>Action to Home</returns>
        private RedirectToRouteResult RedirectHome()
        {
            return new RedirectToRouteResult(new RouteValueDictionary(new
            {
                controller = "Home",
                action = "Index"
            }));
        }
    }
}