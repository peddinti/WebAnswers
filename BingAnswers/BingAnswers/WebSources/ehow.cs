using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using System.Xml;
using System.Threading;
using System.Xml.Serialization;
using HtmlAgilityPack;
using NLog;

namespace BingAnswers.WebSources
{
    public class ehow : Answer
    {
        public const string siteName = "ehow.com";
        public const int resultsSize = 3;
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
            BasePage.SqlLogger.InsertAnswer(basePage.SessionID, ranker.query, AnswerSource.eHow.ToString(), AnswerType.Text.ToString(), answers.Count);
            return answers;
        }

        private List<Answer> ParseSearchResult(IEnumerable<WebResultsWebResult> bingResults, string query)
        {
            List<Answer> answers = new List<Answer>();
            List<Thread> workers = new List<Thread>();

            foreach (WebResultsWebResult result in bingResults)
            {
                Answer answer = new Answer();
                if (result.Url.Contains("how_"))
                {
                    answer.url = result.Url;
                    Thread worker = new Thread(() => GetEhowAnswer(answer,answers));
                    worker.Start();
                    workers.Add(worker);                    
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
            //logger.Info("Ehow Result Count:{0}", answers.Count);
            return answers;
        }

        private void GetEhowAnswer(Answer ans, List<Answer> answers)
        {
            Stream responseStream = Util.MakeWebRequestStream(ans.url);
            if (responseStream == null)
                return;
            HtmlDocument doc = new HtmlDocument();
            doc.Load(responseStream);
            try
            {
                HtmlNode node = doc.DocumentNode.SelectSingleNode("//ol[@id=\"intelliTxt\"]");
                if (node == null)
                {
                    //logger.Error("encountered an invalid answer for url:{0}", ans.url);
                    return;
                }
                string answer = "<ol>";
                foreach (HtmlNode subnode in node.SelectNodes("//div[@itemprop=\"step\"]"))
                {
                    answer += "<li>" + subnode.InnerText + "</li>";
                }
                answer += "</ol>";
                ans.topAnswer = answer;
                ans.question = doc.DocumentNode.SelectSingleNode("//h1[@class=\"articleTitle Heading1\"]").InnerText;
                ans.source = AnswerSource.eHow;
                ans.type = AnswerType.Text;
                ranker.GetFeatures(ans);
                answers.Add(ans);
            }
            catch (Exception e)
            {
                //logger.Error("Encountered an an exception parsing url:{0} Exception:{1}", ans.url, e.ToString());
            }
            // To get individual steps.
            //HtmlNodeCollection collection = root_node.SelectNodes("//ol[@id=\"intelliTxt\"]/li/ul/li//p");
        }
    }
}