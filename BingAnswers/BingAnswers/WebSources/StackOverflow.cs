using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Xml;
using System.Threading;
using NLog;

namespace BingAnswers.WebSources
{    
    public  class StackOverflow : Answer
    {
        public const string apiKey = "Ss7uZc4OT0qE1qu1gmf30Q";
        public string apiqueryTemplate = "http://api.stackoverflow.com/1.1/search?key={0}&intitle={1}&sort=votes&pagesize={2}";
        public string answerQueryTemplate = "http://api.stackoverflow.com/1.1/answers/{1}?key={0}&body=true&comments=true&sort=votes";
        public string answerUrlTemplate = "http://stackoverflow.com/questions/{0}/answers";
        public const int maxAttempts = 5;
        public const int resultsSize = 3;
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
            BasePage.SqlLogger.InsertAnswer(basePage.SessionID, ranker.query, AnswerSource.StackOverflow.ToString(), AnswerType.Text.ToString(), answers.Count);
            return answers;
        }

        private string SendRequest(string query)
        {
            string apiquery = string.Format(apiqueryTemplate, apiKey, HttpUtility.UrlPathEncode(query), resultsSize);
            return Util.MakeWebRequest(apiquery);            
        }

        private List<Answer> ParseResponse(string stackResponse)
        {
            List<Answer> parsedAnswers = new List<Answer>();
            List<Thread> workers = new List<Thread>();
            #region converted the json to xml
            stackResponse = Util.SanitizeJson(stackResponse);
            XmlDocument responseXmlDoc = new XmlDocument();
            responseXmlDoc.Load(Util.JsonToXml(stackResponse));
            #endregion
            XmlNodeList items = responseXmlDoc.FirstChild.SelectNodes("questions/item");
            int resultcount = 0;
            foreach (XmlNode item in items)
            {
                if (item.SelectSingleNode("accepted_answer_id") != null)
                {
                    // if there is no accepted answer then we cant decide which answer to show so we throw it out.
                    string answerID = item.SelectSingleNode("accepted_answer_id").InnerText;
                    Thread worker = new Thread(() => FetchAnswer(answerID, parsedAnswers));
                    worker.Start();
                    workers.Add(worker);
                    if (resultcount > resultsSize)
                        break;
                    resultcount += 1;
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
            //logger.Info("StackOverflow Result Count:{0}", parsedAnswers.Count);
            return parsedAnswers;
        }

        private void FetchAnswer(string id, List<Answer> answers)
        {
            Answer answer = new Answer();
            string answerQuery = string.Format(answerQueryTemplate, apiKey,id);
            string answerResponse = Util.MakeWebRequest(answerQuery);
            Answer answerFetched = (answerResponse != null) ? ParseAnswer(answerResponse) : null;
            if (answerFetched != null)
                answers.Add(answerFetched);
        }

        private Answer ParseAnswer(string answerJsonResponse)
        {
            Answer answer = new Answer();
            #region converting the json to xml
            string sanitizedJson = Util.SanitizeJson(answerJsonResponse);
            XmlDocument answerXmlDoc = new XmlDocument();
            answerXmlDoc.Load(Util.JsonToXml(sanitizedJson));
            #endregion
            XmlNode answerNode = answerXmlDoc.FirstChild.SelectSingleNode("answers/item");
            #region creating the answer object
            answer.question = answerNode.SelectSingleNode("title").InnerText;            
            answer.isStarred = (answerNode.SelectSingleNode("accepted").InnerText == "true") ? true : false;
            answer.topAnswer = answerNode.SelectSingleNode("body").InnerText;
            string questionid = answerNode.SelectSingleNode("question_id").InnerText;
            answer.url = String.Format(answerUrlTemplate, questionid);
            Int64.TryParse(answerNode.SelectSingleNode("creation_date").InnerText, out answer.questionFileTime);
            DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            answer.questionFileTime = Epoch.AddSeconds(answer.questionFileTime).ToFileTimeUtc();
            Int64.TryParse(answerNode.SelectSingleNode("last_activity_date").InnerText, out answer.updatedFileTime);
            answer.updatedFileTime = Epoch.AddSeconds(answer.updatedFileTime).ToFileTimeUtc();
            int.TryParse(answerNode.SelectSingleNode("score").InnerText, out answer.answerVotes);

            // getting total votes
            foreach (XmlNode node in answerXmlDoc.FirstChild.SelectNodes("up_vote_count"))
            {
                int score = 0;
                int.TryParse(node.InnerText,out score);
                answer.totalVotes += score;
            }
            foreach (XmlNode node in answerXmlDoc.FirstChild.SelectNodes("down_vote_count"))
            {
                int score = 0;
                int.TryParse(node.InnerText, out score);
                answer.totalVotes += score;
            }

            // writing the whole node as meta data
            answer.metaData = answerNode.ToString();
            answer.source = AnswerSource.StackOverflow;
            answer.type = AnswerType.Text;
            #endregion
            // extracting the features
            ranker.GetFeatures(answer);
            return answer;
        }
    }
}