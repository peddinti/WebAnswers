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
    public class WikiAnswers : Answer
    {
        //NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private const string siteName = "wiki.answers.com/Q/";

        public override List<Answer> GetAnswer(BasePage basePage)
        {
            this.ranker = basePage.Ranker;
            IEnumerable<WebResultsWebResult> bingResults = Util.GetBingWebResults(ranker.query, siteName);            
            if (bingResults == null)
            {
                //logger.Error("encountered in valid bing response for query:{0}", ranker.query);
                return new List<Answer>();
            }
            List<Answer> answers = ParseSearchResult(bingResults, ranker.query);
            BasePage.SqlLogger.InsertAnswer(basePage.SessionID, ranker.query, AnswerSource.WikiAnswers.ToString(), AnswerType.Text.ToString(), answers.Count);
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
                Thread worker = new Thread(() => GetWikiAnswer(answer, answers));
                worker.Start();
                workers.Add(worker);
            }

            while (workers.Where(thread => thread.IsAlive).Count() > 0) ;
            //logger.Info("WikiAnswers Result Count:{0}", answers.Count);
            return answers;
        }

        private void GetWikiAnswer(Answer ans, List<Answer> answers)
        {
            Stream responseStream = Util.MakeWebRequestStream(ans.url);
            if (responseStream == null)
                return;
            HtmlDocument doc = new HtmlDocument();
            doc.Load(responseStream);
            try
            {
                HtmlNode answerNode = doc.DocumentNode.SelectSingleNode("//div[@id=\"editorText\"]");
                HtmlNode questionNode = doc.DocumentNode.SelectSingleNode("//div[@id=\"questionContainer\"]/h1");
                if (answerNode == null || questionNode == null)
                {
                    //logger.Error("Encountered an invalid answer for url:{0}", ans.url);
                    return;
                }
                ans.topAnswer = answerNode.InnerHtml;
                ans.question = questionNode.InnerText;
                ans.source = AnswerSource.WikiAnswers;
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