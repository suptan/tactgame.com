using System;
using System.Runtime.Serialization.Json;
using System.Web.Mvc;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace tactgame.com.Helpers
{
	public class ObjectFilter : ActionFilterAttribute
	{
		#region Properties

		/// <summary>
		/// Gets or sets the parameter.
		/// </summary>
		/// <value>The parameter.</value>
		public string Param { get; set; }

		/// <summary>
		/// Gets or sets the type of the root.
		/// </summary>
		/// <value>The type of the root.</value>
		public Type RootType { get; set; }

		#endregion

		#region IActionFilter Members

		/// <summary>
		/// Raises the action executing event.
		/// </summary>
		/// <param name="filterContext">Filter context.</param>
		public override void OnActionExecuting (ActionExecutingContext filterContext)
		{
			if ((filterContext.HttpContext.Request.ContentType ?? string.Empty).Contains ("application/json")) {
				object o = new DataContractJsonSerializer (RootType).ReadObject (filterContext.HttpContext.Request.InputStream);
				filterContext.ActionParameters [Param] = 0;
			} else {
				var xmlRoot = XElement.Load (new StreamReader (filterContext.HttpContext.Request.InputStream, filterContext.HttpContext.Request.ContentEncoding));
				object o = new XmlSerializer (RootType).Deserialize (xmlRoot.CreateReader ());
			}
		}

		#endregion
	}
}

