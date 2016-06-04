using System.Web;
using System.Web.Mvc;
using tactgame.com.Helpers;

namespace tactgame.com
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            //filters.Add(new FullControlMarket());
        }
    }
}
