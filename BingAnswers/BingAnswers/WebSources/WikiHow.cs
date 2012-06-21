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
    public class WikiHow : Answer
    {
        private const string siteName = "wikihow.com";
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
            BasePage.SqlLogger.InsertAnswer(basePage.SessionID, ranker.query, AnswerSource.WikiHow.ToString(), AnswerType.Text.ToString(), answers.Count);
            return answers;
        }

        private List<Answer> ParseSearchResult(IEnumerable<WebResultsWebResult> bingResults, string query)
        {
            List<Answer> answers = new List<Answer>();
            List<Thread> workers = new List<Thread>();
            XmlDocument xmlDoc = new XmlDocument();

            foreach (WebResultsWebResult result in bingResults)
            {
                Answer answer = new Answer();
                answer.url = result.Url;
                Thread worker = new Thread(() => GetWikihowAnswer(answer, answers));
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
            //logger.Info("WikiHow Result Count:{0}", answers.Count);
            return answers;
        }

        private void GetWikihowAnswer(Answer ans, List<Answer> answers)
        {
            Stream responseStream = Util.MakeWebRequestStream(ans.url);
            if (responseStream == null)
                return;
            HtmlDocument doc = new HtmlDocument();
            doc.Load(responseStream);
            try
            {
                string answer = "<ol>";
                if (doc.DocumentNode.SelectNodes("//div[@id=\"steps\"]") == null)
                {
                    //logger.Error("Encountered an invalid answer for url:{0}", ans.url);
                    return;
                }
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@id=\"steps\"]/ol/li/b"))
                {
                    answer += "<li>" + node.InnerHtml + "</li>";
                }
                answer += "</ol>";

                ans.topAnswer = answer;
                ans.question = doc.DocumentNode.SelectSingleNode("//h1[@class=\"firstHeading\"]/a").InnerText;
                ans.source = AnswerSource.WikiHow;
                ans.type = AnswerType.Text;
                ranker.GetFeatures(ans);
                answers.Add(ans);
            }
            catch (Exception)
            {
                //logger.Error("Encountered an an exception parsing url:{0} Exception:{1}", ans.url,e.ToString());
            }
        }
    }
}