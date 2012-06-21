using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Net;
using System.IO;
using System.Threading;
using NLog;

namespace BingAnswers.WebSources
{
    public class MSDNForum : Answer
    {
        public string queryUrlTemplate = "http://social.msdn.microsoft.com/search/en-US/feed?query={0}&refinement=112&outputAs=xmlsort=repliesdesc&filter=answered";
        public string answersUrlTemplate = "{0}?outputAs=xml";
        public const int maxAttempts = 5;
        public const int resultsSize = 3;
        //NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public override List<Answer> GetAnswer(BasePage basePage)
        {
            base.ranker = basePage.Ranker;
            List<Answer> answers = new List<Answer>();            
            string webResponse = SendRequest(ranker.query);
            answers = ParseResponse(webResponse);
            BasePage.SqlLogger.InsertAnswer(basePage.SessionID, ranker.query, AnswerSource.MSDN.ToString(), AnswerType.Text.ToString(), answers.Count);
            return answers;
        }

        private string SendRequest(string query)
        {
            string queryUrl = String.Format(queryUrlTemplate, HttpUtility.UrlEncode(query));
            return Util.MakeWebRequest(queryUrl);
           
        }

        private List<Answer> ParseResponse(string questionResponseXml)
        {
            List<Answer> parsedAnswers = new List<Answer>();
            List<Thread> workers = new List<Thread>();
            XmlDocument questionDoc = new XmlDocument();
            questionDoc.LoadXml(questionResponseXml);
            XmlNodeList questions = questionDoc.GetElementsByTagName("entry");
            XmlNamespaceManager ns = new XmlNamespaceManager(questionDoc.NameTable);
            int count = 0;
            foreach (XmlNode question in questions)
            {
                if (count > resultsSize)
                    break;
                string answerlink = question.LastChild.Attributes["href"].Value;
                if (answerlink.Contains("social.msdn.microsoft.com"))
                {
                    // only getting data for social.msdn as they have a xml rss response.
                    Thread worker = new Thread(() => FetchAnswer(answerlink, parsedAnswers));
                    worker.Start();
                    workers.Add(worker);
                    count += 1;
                }
            }

            while (workers.Where(thread => thread.IsAlive).Count() > 0)
            {
                //List<Thread> remThreads = new List<Thread>();
                //foreach (Thread worker in workers)
                //    if (worker.IsAlive)
                //        remThreads.Add(worker);
                //workers = remThreads;
            }
            //logger.Info("MSDN Result Count:{0}", parsedAnswers.Count);
            return parsedAnswers;
        }

        private void FetchAnswer(string url, List<Answer> answers)
        {            
            string answersUrl = String.Format(answersUrlTemplate, url);
            string answerresponse = Util.MakeWebRequest(answersUrl);
            Answer parsedAnswer = (answerresponse != null) ? ParseAnswer(answerresponse) : null;
            if (parsedAnswer != null)
                answers.Add(parsedAnswer);
        }

        private Answer ParseAnswer(string answerResponseXml)
        {
            Answer answer = new Answer();
            XmlDocument answerDoc = new XmlDocument();            
            try
            {
                answerDoc.LoadXml(answerResponseXml);
                // the first result is the question so getting the latest response.
                XmlNode answerNode = answerDoc.FirstChild.SelectSingleNode("messages/message[2]");
                answer.question = answerDoc.FirstChild.SelectSingleNode("thread/topic").InnerText;
                answer.isStarred = (answerNode.SelectSingleNode("answer") != null && answerNode.SelectSingleNode("answer").InnerText == "true");
                answer.topAnswer = answerNode.SelectSingleNode("body").InnerText;
                answer.url = answerNode.SelectSingleNode("url").InnerText;
                answer.source = AnswerSource.MSDN;
                answer.type = AnswerType.Text;
                Int64.TryParse(answerNode.SelectSingleNode("createdOn").InnerText, out answer.questionFileTime);
                DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                answer.questionFileTime = Epoch.AddSeconds(answer.questionFileTime).ToFileTimeUtc();
                int.TryParse(answerNode.Attributes["helpfulVotes"].Value, out answer.answerVotes);
                foreach(XmlNode node in answerDoc.FirstChild.SelectNodes("message"))
                {
                    int score = 0;
                    int.TryParse(node.Attributes["helpfulVotes"].Value, out score);
                    answer.totalVotes += score;
                }

            }
            catch (Exception e)
            {
                answer = null;
                //logger.Error("encountered an exception {0}",e.ToString());
            }
            ranker.GetFeatures(answer);            
            return answer;
        }
    }
}