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
    public class YahooAnswers : Answer
    {
        public string yql_question_search_query = "select * from answers.search where query=\"{0}\" and type=\"resolved\"";
        public string yql_answer_information_query = "select * from answers.getquestion where question_id=\"{0}\"";
        public string yql_url = "http://query.yahooapis.com/v1/public/yql?q={0}&diagnostics=true";
        public const int maxAttempts = 5;
        public const int resultsSize = 3;
        //NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public override List<Answer> GetAnswer(BasePage basePage)
        {
            List<Thread> workers = new List<Thread>();
            List<Answer> answers = new List<Answer>();
            base.ranker = basePage.Ranker;
            Stream webResponse = sendQuestionSearchRequest(ranker.query);
            answers = ParseQuestionResponse(webResponse);
            // the below extra calls are only to get votes information which is not reliable and hence not worth making.
            //List<Answer> yahooAnswers = new List<Answer>();
            //int resultcount = 0;
            //foreach (Answer ans in answers)
            //{
            //    Thread worker = new Thread(() => ParseAnswerResponse(ans, sendAnswerSearchRequst(ans.metaData)));
            //    worker.Start();
            //    workers.Add(worker);
            //    yahooAnswers.Add(ans);
            //    //webResponse = sendAnswerSearchRequst(ans.metaData);
            //    //ParseAnswerResponse(ans, webResponse);
            //    if (resultcount > resultsSize)
            //        break;
            //    resultcount += 1;
            //}
            //while (workers.Count > 0)
            //{
            //    List<Thread>  remThreads = new List<Thread>();
            //    foreach (Thread worker in workers)
            //        if (worker.IsAlive)
            //            remThreads.Add(worker);
            //    workers = remThreads;
            //}
            //return yahooAnswers;
            if (answers == null)
            {
                //logger.Info("YahooAnswers Result Count: no results");
            }
            else
            {
                //logger.Info("YahooAnswers Result Count:{0}", answers.Count);
            }
            BasePage.SqlLogger.InsertAnswer(basePage.SessionID, ranker.query, AnswerSource.YahooAnswers.ToString(), AnswerType.Text.ToString(), answers.Count);
            return answers;
        }

        private Stream sendQuestionSearchRequest(string query)
        {
            return sendRequest(String.Format(yql_question_search_query, query));
        }

        private Stream sendAnswerSearchRequst(string question_id)
        {
            return sendRequest(String.Format(yql_answer_information_query, question_id));
        }

        private Stream sendRequest(string requestQuery)
        {
            //string encodedQuery = HttpUtility.UrlPathEncode(requestQuery);
            string encodedQuery = HttpUtility.UrlEncode(requestQuery);
            string requestUrl = String.Format(yql_url, encodedQuery);
            return Util.MakeWebRequestStream(requestUrl);
        }

        private List<Answer> ParseQuestionResponse(Stream responseStream)
        {
            List<Answer> answers = new List<Answer>();
            try
            {
                XmlReader reader = XmlReader.Create(responseStream);

                while (reader.ReadToFollowing("Question"))
                {
                    Answer answer = new Answer();

                    answer.isStarred = (reader.GetAttribute("type") == "Answered");

                    answer.metaData = reader.GetAttribute("id");

                    reader.ReadToFollowing("Subject");
                    answer.question = reader.ReadString();

                    reader.ReadToFollowing("Timestamp");
                    Int64.TryParse(reader.ReadString(), out answer.questionFileTime);
                    DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0,DateTimeKind.Utc);
                    answer.questionFileTime = Epoch.AddSeconds(answer.questionFileTime).ToFileTimeUtc();

                    reader.ReadToFollowing("Link");
                    answer.url = reader.ReadString();

                    reader.ReadToFollowing("ChosenAnswer");
                    answer.topAnswer = reader.ReadString();

                    reader.ReadToFollowing("ChosenAnswerAwardTimestamp");
                    Int64.TryParse(reader.ReadString(), out answer.updatedFileTime);
                    answer.updatedFileTime = Epoch.AddSeconds(answer.updatedFileTime).ToFileTimeUtc();
                    answer.source = AnswerSource.YahooAnswers;
                    answer.type = AnswerType.Text;
                    ranker.GetFeatures(answer);
                    answers.Add(answer);
                }
            }
            catch (Exception e)
            {                
                //logger.Error("encountered an exception {0}", e.ToString());
            }

            return answers;
        }

        private void ParseAnswerResponse(Answer answer, Stream responseStream)
        {
            if (responseStream != null)
            {
                XmlReader reader = XmlReader.Create(responseStream);
                reader.ReadToFollowing("Answers");

                while (reader.ReadToFollowing("Answer"))
                {
                    reader.ReadToFollowing("Content");
                    if (answer.topAnswer.Equals(reader.ReadString()))
                    {
                        // This score needs to be updated primarily due to the fact that that most answers have
                        // a score of 5.0/5.0. Perhaps we can use other metrics available such as number of answers.
                        reader.ReadToFollowing("Best");
                        int.TryParse(reader.ReadString(), out answer.answerVotes);
                        return;
                    }
                }
            }
        }
    }
}