using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using System.Timers;
using System.Text.RegularExpressions;
using NLog;

namespace BingAnswers
{
    public partial class Answers : BasePage
    {
        //public List<Thread> workers = new List<Thread>();
        //public static readonly object locker = new object();
        //public List<Answer> FinalAnswers;
        //bool timeOut = false;

        //NLog.Logger logger = LogManager.GetLogger("BingAnswers.Answers");        

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack && !string.IsNullOrEmpty(Page.Request.Params["__EVENTTARGET"]) &&
                (this.FindControl(Page.Request.Params["__EVENTTARGET"]).ClientID.Contains("Click") || this.FindControl(Page.Request.Params["__EVENTTARGET"]).ClientID.Contains("Feedback")))
            {
                LoadSession();
                return;
            }

            string query = "";            
            if (!string.IsNullOrWhiteSpace(Answer_SearchBox.Text))
            {
                query = Answer_SearchBox.Text;
                string hostName = Request.ServerVariables.GetValues("HTTP_HOST")[0];
                //logger.Info("Redirecting to Answers.aspx with query:{0}", query);
                Response.Redirect("http://" + hostName + "/Answers.aspx?q=" + HttpUtility.UrlEncode(query));                
            }
            else if (!string.IsNullOrEmpty(HttpContext.Current.Request.Params["q"]))
                query = HttpContext.Current.Request.Params["q"];            
            if (!string.IsNullOrWhiteSpace(query))
            {
                //logger.Info("Query:{0}", query);
                Answer_SearchBox.Text = query;
                // set interval to 5000
                BasePage.Interval = 7000;
                this.GetAnswers(query);                                

                //logger.Trace("Ranking results");
                //logger.Info("Final Num Results:{0}", FinalAnswers.Count);
                
                // sorting the results based on score
               FinalAnswers.Sort(delegate(Answer a1, Answer a2) { return a2.score.CompareTo(a1.score); });
                //logger.Trace("End of Ranking");
                answerRep.DataSource = FinalAnswers;
                FinalAnswers = FinalAnswers;
                answerRep.DataBind();
                this.DataBind();
                SaveSession();
            }            
        } 

        public string GetAnswerTemplate(object answer, int index)
        {
            try
            {
                Answer finalAnswer = (Answer)answer;
                switch (finalAnswer.Type)
                {
                    case Answer.AnswerType.Text:
                        return finalAnswer.topAnswer;
                    case Answer.AnswerType.Video:
                        return "<iframe width=\"600\" height=\"300\" frameborder=\"0\" src=\"" + finalAnswer.topAnswer + "\" allowfullscreen ></iframe>";
                    case Answer.AnswerType.Image:
                        string[] metaDataCol = finalAnswer.metaData.Split(',');
                        int height; int.TryParse(metaDataCol[0],out height);
                        int width; int.TryParse(metaDataCol[0],out width);
                        if (height > 400)
                            height = 400;
                        if (width > 400)
                            width = 400;
                        return string.Format("<img width=\"{0}\" height=\"{1}\" src=\"{2}\" />",width,height,finalAnswer.topAnswer);
                    default:
                        return "";
                }
            }
            catch
            {
                return "";
            }
        }        

        protected void Entity_Click(object sender, System.EventArgs e)
        {
            int index = int.Parse(((LinkButton)sender).CommandArgument);
            Answer answer = FinalAnswers.ElementAt(index);
            SqlLogger.InsertAction(SessionID, Action.Click.ToString(), answer.Url, index);
            Response.Redirect(answer.url);
        }

        protected void Entity_Like(object sender, System.EventArgs e)
        {
            int index = int.Parse(((LinkButton)sender).CommandArgument);
            Answer answer = FinalAnswers.ElementAt(index);
            SqlLogger.InsertAction(SessionID, Action.Like.ToString(), answer.Url, index);            
        }

        protected void RatingSubmit_Click(object sender, System.EventArgs e)
        {
            int rating; int.TryParse(((RadioButtonList)this.FindControl("ctl00$FeedbackOverlay$FeedbackRating")).SelectedValue, out rating);
            string comments = ((TextBox)this.FindControl("ctl00$FeedbackOverlay$FeedbackComments")).Text;
            if(!String.IsNullOrEmpty(SessionID))
                SqlLogger.InsertRating(SessionID, Ranker.query, rating, comments);
        }
    }
}
