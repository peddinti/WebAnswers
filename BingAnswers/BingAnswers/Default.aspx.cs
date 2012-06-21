using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;

namespace BingAnswers
{    
    public partial class _Default : System.Web.UI.Page
    {
        public void Page_Load(object sender, EventArgs e)
        {
            // Redirect to Answers.aspx
            string hostName = Request.ServerVariables.GetValues("HTTP_HOST")[0];            
            if(!string.IsNullOrEmpty(SearchBox.Text))
                Response.Redirect("/Answers.aspx?q=" + HttpUtility.UrlEncode(SearchBox.Text));

        }        

    }
}
