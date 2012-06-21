using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.Threading;
using HtmlAgilityPack;
using NLog;
namespace BingAnswers.WebSources
{
    public class Quora : Answer
    {
        private const string siteName = "quora.com";
        //NLog.Logger logger = LogManager.GetCurrentClassLogger();
        public override List<Answer> GetAnswer(BasePage basePage)
        {
            base.ranker = basePage.Ranker;
            IEnumerable<WebResultsWebResult> bingResults = Util.GetBingWebResults(ranker.query, siteName);            
            if (bingResults == null)
            {
                //logger.Error("encountered in valid bing response for query:{0}", ranker.query);
                return new List<Answer>();
            }            
            List<Answer> answers = ParseSearchResult(bingResults, ranker.query);
            BasePage.SqlLogger.InsertAnswer(basePage.SessionID, ranker.query, AnswerSource.Quora.ToString(), AnswerType.Text.ToString(), answers.Count);
            return answers;
        }

        private List<Answer> ParseSearchResult(IEnumerable<WebResultsWebResult> bingResults, string query)
        {
            List<Answer> answers = new List<Answer>();
            List<Thread> workers = new List<Thread>();

            foreach (WebResultsWebResult result in bingResults)
            {
                Answer answer = new Answer();
                answer.url = result.Url;
                Thread worker = new Thread(() => GetQuoraAnswer(answer, answers));
                worker.Start();
                workers.Add(worker);
            }

            while (workers.Where(thread => thread.IsAlive).Count() > 0)
            {
                //List<Thread> remThreads = new List<Thread>();
                //foreach (Thread worker in workers)
                //    if (worker.IsAlive)
                //        remThreads.Add(worker);
                //workers = remThreads;
            }
            //logger.Info("Quora Result Count:{0}", answers.Count);
            return answers;
        }

        private void GetQuoraAnswer(Answer ans, List<Answer> answers)
        {
            Stream responseStream = Util.MakeWebRequestStream(ans.url);
            if (responseStream == null)
                return;
            HtmlDocument doc = new HtmlDocument();
            doc.Load(responseStream);
            try
            {
                HtmlNode questionNode = doc.DocumentNode.SelectNodes("//div[@class=\"question row\"]//h1")[0];
                HtmlNode answerNode = doc.DocumentNode.SelectNodes("//div[@class=\"row\"]//div[@class=\"row\"]//div[@class=\"answer_content\"]")[0];
                if (questionNode == null || answerNode == null)
                {
                    //logger.Error("encountered an invalid answer for url:{0}", ans.url);
                    return;
                }
                ans.question = questionNode.InnerText;
                ans.topAnswer = answerNode.InnerHtml;
                Int32.TryParse(doc.DocumentNode.SelectNodes("//div[@class=\"row\"]//div[@class=\"row\"]//strong[@class=\"voter_count\"]")[0].InnerText, out ans.answerVotes);
                ans.totalVotes = 0;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"row\"]//strong[@class=\"voter_count\"]"))
                {
                    int vote;
                    Int32.TryParse(node.InnerText, out vote);
                    ans.totalVotes += vote;
                }
                ans.source = AnswerSource.Quora;
                ans.type = AnswerType.Text;
                ranker.GetFeatures(ans);
                answers.Add(ans);
            }
            catch (Exception)
            {
                //logger.Error("Encountered an an exception parsing url:{0} Exception:{1}", ans.url, e.ToString());
            }
        }
    }
}