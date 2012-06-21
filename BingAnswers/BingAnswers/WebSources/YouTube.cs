using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using NLog;

namespace BingAnswers.WebSources
{
    public class YouTube : Answer
    {
        private const string apiKey = "AI39si5noc4xIt3-tusuDLv0044KDD9YDViXgNG6KzO7hJ2OnS_8xN_8X8ZUFLKg_eJDpPLJj3bAW4GycAjDSUhl4quMBMps0A";
        private string apiQueryTemplate = "http://gdata.youtube.com/feeds/api/videos?key={0}&q={1}&max-results={2}";
        private const int resultsSize = 3;
        //NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public override List<Answer> GetAnswer(BasePage basePage)
        {
            base.ranker = basePage.Ranker;
            List<Answer> answers = new List<Answer>();            
            // check if it is a what query;
            List<string> WrbQuery = ranker.queryWords.Where((w, i) => Ranker.WHTags.Contains(ranker.queryTags.ElementAt(i))).ToList<string>();
            if (!WrbQuery.Contains("how"))
                return null;
            Stream webResponse = SendRequest(ranker.query);
            if (webResponse != null)
            {
                answers = ParseResponse(webResponse);
            }
            BasePage.SqlLogger.InsertAnswer(basePage.SessionID, ranker.query, AnswerSource.YouTube.ToString(), AnswerType.Video.ToString(), answers.Count);
            return answers;            
        }

        private Stream SendRequest(string query)
        {
            string apiQuery = string.Format(apiQueryTemplate, apiKey, HttpUtility.UrlPathEncode(query), resultsSize);
            return Util.MakeWebRequestStream(apiQuery);
        }

        private List<Answer> ParseResponse(Stream webResponse)
        {
            List<Answer> answers = new List<Answer>();
            XmlSerializer serializer = new XmlSerializer(typeof(feed));
            feed searchResponse = (feed)serializer.Deserialize(webResponse);
            foreach (feedEntry entry in searchResponse.entry)
            {
                Answer answer = new Answer();
                answer.question = entry.title;
                answer.questionFileTime = DateTime.Parse(entry.published).ToFileTimeUtc();
                answer.updatedFileTime = DateTime.Parse(entry.updated).ToFileTime();
                int.TryParse(entry.statistics.ElementAt(0).favoriteCount, out answer.answerVotes);
                int.TryParse(entry.statistics.ElementAt(0).viewCount, out answer.totalVotes);
                answer.isStarred = true;
                answer.metaData = entry.group.ElementAt(0).keywords;
                string videoID = entry.id.Split('/').LastOrDefault();
                answer.topAnswer = string.Format("http://www.youtube.com/embed/{0}", videoID);
                answer.url = entry.group.ElementAt(0).player.ElementAt(0).url;
                answer.source = AnswerSource.YouTube;
                answer.type = AnswerType.Video;
                ranker.GetFeatures(answer);
                answers.Add(answer);
            }
            return answers;
        }
    }
}