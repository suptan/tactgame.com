using System;
using System.Web.Mvc;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace tactgame.com.Helpers
{
	public class ObjectResult<T> : ActionResult
	{
		/// <summary>
		/// The UTF8.
		/// </summary>
		private static UTF8Encoding UTF8 = new UTF8Encoding (false);

		/// <summary>
		/// Gets or sets the data.
		/// </summary>
		/// <value>The data.</value>
		public T Data { get; set; }

		/// <summary>
		/// The included types.
		/// </summary>
		public Type[] IncludedTypes = new[] { typeof(object) };

		/// <summary>
		/// Initializes a new instance of the ObjectResult class.
		/// </summary>
		public ObjectResult ()
		{
		}

		/// <summary>
		/// Initializes a new instance of the ObjectResult class.
		/// </summary>
		/// <param name="data">Data.</param>
		public ObjectResult (T data)
		{
			this.Data = data;
		}

		/// <summary>
		/// Initializes a new instance of the ObjectResult class.
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="extraTypes">Extra types.</param>
		public ObjectResult (T data, Type[] extraTypes)
		{
			this.Data = data;
			this.IncludedTypes = extraTypes;
		}

		public override void ExecuteResult (ControllerContext context)
		{
			// If ContentType is not expected to be application/json, then return XML
			if (context.HttpContext.Request.Headers ["Content-Type"].Contains ("application/json")) {
				new JsonResult{ Data = this.Data }.ExecuteResult (context);
			} else {
				using (MemoryStream stram = new MemoryStream (500)) {
					using (var xmlWriter = XmlTextWriter.Create (stram, new XmlWriterSettings () {
						OmitXmlDeclaration = true,
						Encoding = UTF8,
						Indent = true
					})) {
						new XmlSerializer (typeof(T), IncludedTypes).Serialize (xmlWriter, this.Data);
					}
					// NOTE:
					new ContentResult {
						ContentType = "text/xml",
						Content = UTF8.GetString (stram.ToArray ()),
						ContentEncoding = UTF8
					}.ExecuteResult (context);
				}
			}
		}
	}
}

