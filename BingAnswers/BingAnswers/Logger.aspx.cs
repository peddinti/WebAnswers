using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NLog;
namespace BingAnswers
{
    public partial class Logger : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //NLog.Logger logger = LogManager.GetLogger("InstrumentationLogger");
            if (string.IsNullOrEmpty(HttpContext.Current.Request.Params["scenario"]))
                return;
            try
            {
                switch (HttpContext.Current.Request.Params["scenario"])
                {
                    case "like":
                        //logger.Info("Like|{0}|{1}|{2}", HttpUtility.UrlDecode(HttpContext.Current.Request.Params["q"]), HttpUtility.UrlDecode(HttpContext.Current.Request.Params["link"]), HttpContext.Current.Request.Params["k"]);
                        break;
                    case "share":
                        //logger.Info("Share|{0}", HttpUtility.UrlDecode(HttpContext.Current.Request.Params["q"]));
                        break;
                    case "click":
                        //logger.Info("Like|{0}|{1}|{2}", HttpUtility.UrlDecode(HttpContext.Current.Request.Params["q"]), HttpUtility.UrlDecode(HttpContext.Current.Request.Params["link"]), HttpContext.Current.Request.Params["k"]);
                        break;
                }
            }
            catch (Exception)
            {
                //logger.Error("Error logging instrumentation url:{0}", HttpContext.Current.Request.QueryString);
            }
        }
    }
}