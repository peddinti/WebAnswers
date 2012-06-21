using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using NLog;

namespace BingAnswers.WebSources
{
    public class BingImages : Answer
    {
        private const string siteFilter = "wikipedia.org";
        private const int resultCount = 1;
        //NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public override List<Answer> GetAnswer(BasePage basePage)
        {
            base.ranker = basePage.Ranker;
            string nnQuery = string.Join(" ", ranker.queryWords.Where((w, i) => Ranker.NNTags.Contains(ranker.queryTags.ElementAt(i))));
            // check if it is a what query;
            List<string> WrbQuery = ranker.queryWords.Where((w, i) => Ranker.WHTags.Contains(ranker.queryTags.ElementAt(i))).ToList<string>();
            if (!WrbQuery.Contains("what") && !WrbQuery.Contains("who"))
                return null;            
            SearchResponse bingResults = Util.SendBingSearchRequest(nnQuery, siteFilter, "image");
            if (bingResults == null||bingResults.Image == null || bingResults.Image.Results == null)
            {
                //logger.Error("encountered in valid bing response for query:{0}", nnQuery);
                return new List<Answer>();
            }
            List<Answer> finalAnswers = ParseSearchResult(bingResults.Image.Results.Take(5), nnQuery);
            BasePage.SqlLogger.InsertAnswer(basePage.SessionID, ranker.query, AnswerSource.BingImages.ToString(), AnswerType.Image.ToString(), finalAnswers.Count);
            return finalAnswers;
        }

        private List<Answer> ParseSearchResult(IEnumerable<ImageResultsImageResult> bingResults, string query)
        {
            List<Answer> answers = new List<Answer>();

            foreach (ImageResultsImageResult result in bingResults)
            {
                Answer answer = new Answer();
                answer.question = this.ranker.query;
                answer.topAnswer = result.MediaUrl;
                answer.url = result.MediaUrl;
                answer.isStarred = true;
                answer.source = AnswerSource.BingImages;
                answer.type = AnswerType.Image;
                answer.metaData = result.Height + "," + result.Width;
                // add images with high fidility by comaring that all the query words are present in the in the url and title 
                if (query.Split(' ').All(w => result.Title.ToLower().Contains(w)) &&
                    query.Split(' ').All(w => result.MediaUrl.ToLower().Contains(w)))
                {
                    ranker.GetFeatures(answer);
                    answers.Add(answer);
                }
            }
            return answers.Take(resultCount).ToList<Answer>();
        }
    }
}