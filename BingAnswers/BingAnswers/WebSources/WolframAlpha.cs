using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using NLog;

namespace BingAnswers.WebSources
{
    public class WolframAlpha : Answer
    {
        public const string apiKey = "V98YX2-5KTH689X3K";
        public string apiQuery = "http://api.wolframalpha.com/v2/query?input={1}&appid={0}";
        //NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public override List<Answer> GetAnswer(BasePage basePage)
        {
            base.ranker = basePage.Ranker;
            List<Answer> answers = new List<Answer>();            
            string webResponse = SendRequest(ranker.query);
            if (webResponse != null)
            {
                answers = ParseResponse(webResponse);
            }
            BasePage.SqlLogger.InsertAnswer(basePage.SessionID, ranker.query, AnswerSource.WolframeAlpha.ToString(), AnswerType.Text.ToString(), answers.Count);
            return answers;
        }

        private string SendRequest(string query)
        {
            apiQuery = String.Format(apiQuery, apiKey,HttpUtility.UrlEncode(query));
            return Util.MakeWebRequest(apiQuery);
        }

        private List<Answer> ParseResponse(string webResponse)
        {
            List<Answer> answers = new List<Answer>();
            Answer answer = new Answer();
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.LoadXml(webResponse);
            XmlNode inputNode = responseXmlDoc.SelectSingleNode("//pod[@id='Input']");
            XmlNode resultNode = responseXmlDoc.SelectSingleNode("//pod[@id='Result']");
            if (resultNode != null)
            {                
                answer.question = inputNode.SelectSingleNode("subpod/plaintext").InnerText;
                answer.topAnswer = resultNode.SelectSingleNode("subpod/plaintext").InnerText;
                answer.score = 100;
                answer.isStarred = true;
                answer.source = AnswerSource.WolframeAlpha;
                answer.type = AnswerType.Text;
                answer.url = "http://wolframalpha.com";
                ranker.GetFeatures(answer);
                answers.Add(answer);
            }
            //logger.Info("WolframeAlpha Result Count:{0}", answers.Count);
            return answers;
        }
    }
}