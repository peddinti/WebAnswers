using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using NLog;

namespace BingAnswers.WebSources
{
    public class TrueKnowledge : Answer
    {
        public const string apiID = "api_binganswers";
        public const string apiPassword = "4vn8q75ajdvgrt5g";
        public string apiQuery = "https://api.trueknowledge.com/direct_answer?question={0}&api_account_id={1}&api_password={2}&structured_response=0";
        //NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public override List<Answer> GetAnswer(BasePage basePage)
        {
            base.ranker = basePage.Ranker;
            List<Answer> answers = new List<Answer>();            
            string webResponse = SendRequest(ranker.query);
            if (webResponse != null)
            {
                answers = ParseResponse(webResponse,ranker.query);
            }
            BasePage.SqlLogger.InsertAnswer(basePage.SessionID, ranker.query, AnswerSource.TrueKnowledge.ToString(), AnswerType.Text.ToString(), answers.Count);
            return answers;
        }

        private string SendRequest(string query)
        {
            apiQuery = String.Format(apiQuery,HttpUtility.UrlEncode(query),apiID,apiPassword);
            return Util.MakeWebRequest(apiQuery);
        }

        private List<Answer> ParseResponse(string webResponse,string question)
        {
            List<Answer> answers = new List<Answer>();
            Answer answer = new Answer();
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(webResponse);
            XmlNamespaceManager ns = new XmlNamespaceManager(responseXmlDoc.NameTable);
            ns.AddNamespace("tk", "http://www.trueknowledge.com/ns/kengine");
            XmlNode resultNode = responseXmlDoc.SelectSingleNode("//tk:text_result",ns);
            
            if (resultNode != null && resultNode.InnerText != "")
            {
                answer.question = question;
                answer.topAnswer = resultNode.InnerText;
                answer.score = 100;
                answer.isStarred = true;
                answer.url = responseXmlDoc.SelectSingleNode("//tk:tk_question_url", ns).InnerText;
                answer.source = AnswerSource.TrueKnowledge;
                answer.type = AnswerType.Text;
                ranker.GetFeatures(answer);
                answers.Add(answer);
            }
            //logger.Info("TrueKnowledge Result Count:{0}", answers.Count);
            return answers;
        }
    }
}