using System.Configuration;
using System.Web;

namespace tactgame.com.Helpers
{
    public static class Constant
    {
        public static class CsvFilesPath
        {
            public static readonly string PLAYER_PATH = string.Format(@"{0}\{1}\{2}", HttpContext.Current.Server.MapPath("~/App_Data"), ConfigurationManager.AppSettings["players"], ConfigurationManager.AppSettings["playersinfo"]);
        }

        public static class RedirectPath
        {
            public static readonly string HOME_URI = "/";
        }
    }
}