using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Mvc;
using tactgame.com.Models;

namespace tactgame.com.Helpers
{
	/// <summary>
	/// JSON helper.
	/// </summary>
	public static class JSONHelper
	{
		/// <summary>
		/// Tos the JSO.
		/// </summary>
		/// <returns>The JSO.</returns>
		/// <param name="obj">Object.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static string ToJSON<T> (T obj)
		{
			var js = new DataContractJsonSerializer (obj.GetType ());
			var stream = new MemoryStream ();
			js.WriteObject (stream, obj);
			stream.Position = 0;

			return new StreamReader (stream).ReadToEnd ();
		}

		public static T FromJSON<T> (string json)
		{
			var instance = Activator.CreateInstance<T> ();
			using (var stream = new MemoryStream (Encoding.Unicode.GetBytes (json))) {
				var js = new DataContractJsonSerializer (instance.GetType ());
				return (T)js.ReadObject (stream);
			}
		}
        
        /// <summary>
        /// Create JSON with object
        /// </summary>
        /// <param name="isSuccess">Is controller process success</param>
        /// <param name="obj">return data</param>
        /// <returns></returns>
        public static JsonResult CreateJSONResult(bool isSuccess, object obj)
        {
            return new JsonResult()
            {
                Data = new ResponseData(isSuccess, obj),
                ContentType = "application/JSON",
                JsonRequestBehavior = JsonRequestBehavior.DenyGet
            };
        }
    }
}

