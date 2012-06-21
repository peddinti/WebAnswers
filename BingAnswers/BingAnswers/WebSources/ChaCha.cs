using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using HtmlAgilityPack;
using NLog;
using System.Threading;

namespace BingAnswers.WebSources
{
    public class ChaCha : Answer
    {
        public string queryUrlTemplate = "http://chacha.com/askChaCha/{0}";
        //NLog.Logger logger = LogManager.GetCurrentClassLogger();
        public const string inValidAnswer = "ChaCha";
        public override List<Answer> GetAnswer(BasePage basePage)
        {
            base.ranker = basePage.Ranker;
            IEnumerable<WebResultsWebResult> bingResults = Util.GetBingWebResults(ranker.query, "chacha.com");            
            if (bingResults == null)
            {
                //logger.Error("encountered in valid bing response for query:{0}", ranker.query);
                return new List<Answer>();
            }
            List<Answer> answers = ParseSearchResponse(bingResults, ranker.query);
            BasePage.SqlLogger.InsertAnswer(basePage.SessionID, ranker.query, AnswerSource.ChaCha.ToString(), AnswerType.Text.ToString(), answers.Count);
            return answers;
        }       

        private List<Answer> ParseSearchResponse(IEnumerable<WebResultsWebResult> bingResults, string query)
        {
            List<Answer> answers = new List<Answer>();
            List<Thread> workers = new List<Thread>();

            foreach (WebResultsWebResult result in bingResults)
            {
                Answer answer = new Answer();
                answer.url = result.Url;
                Thread worker = new Thread(() => GetChaChaAnswer(answer, answers));
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
            //logger.Info("ChaCha Result Count:{0}", answers.Count);
            return answers;
        }

        private void GetChaChaAnswer(Answer ans, List<Answer> answers)
        {
            Stream responseStream = Util.MakeWebRequestStream(ans.url);
            if (responseStream == null)
                return;
            HtmlDocument doc = new HtmlDocument();            
            try
            {
                doc.Load(responseStream);
                HtmlNode questionNode = doc.DocumentNode.SelectSingleNode("//div[@id=\"question-answer\"]/h1");
                HtmlNode answerNode = doc.DocumentNode.SelectSingleNode("//div[@id=\"question-answer\"]/h2");
                if (questionNode == null || answerNode == null)
                {
                    //logger.Error("Encountered an invalid answer for url:{0}", ans.url);
                    return;
                }
                ans.topAnswer = answerNode.InnerText;
                ans.question = questionNode.InnerText;
                ans.source = AnswerSource.ChaCha;
                ans.type = AnswerType.Text;
                ranker.GetFeatures(ans);
                answers.Add(ans);
            }
            catch (Exception)
            {
                //logger.Error("Encountered an an exception for url:{0} Exception:{1}", ans.url, e.ToString());
            }
        }
    }
}