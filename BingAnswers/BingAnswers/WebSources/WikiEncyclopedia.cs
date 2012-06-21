using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace BingAnswers.WebSources
{
    public class WikiEncyclopedia : Answer
    {
        public string queryTemplate = "http://encyclopedia.thefreedictionary.com/{0}";

        public override List<Answer> GetAnswer(BasePage basePage)
        {
            base.ranker = basePage.Ranker;
            // checking for query terms that contain noun forms and adjectives (as they talk about nouns)
            string nnQuery = string.Join(" ", ranker.queryWords.Where((w, i) => Ranker.NNTags.Contains(ranker.queryTags.ElementAt(i))));
            nnQuery = HttpUtility.UrlPathEncode(nnQuery);
            string queryUrl = String.Format(queryTemplate, nnQuery);
            List<Answer> answers = new List<Answer>();
            if (ranker.queryTags.Exists(tag => Ranker.WHTags.Contains(tag.ToString())))
            {
                // there is a valid postag in the query. hence triggering this answer
                Stream responseHtml = Util.MakeWebRequestStream(queryUrl);
                Answer answer = new Answer();
                ParseResponse(responseHtml, answer);
                if (answer != null && !string.IsNullOrEmpty(answer.topAnswer))
                {
                    answer.isStarred = true;
                    answer.type = AnswerType.Text;
                    answer.url = queryUrl;
                    answer.source = AnswerSource.Wikipedia;
                    ranker.GetFeatures(answer);
                    answers.Add(answer);
                }
            }
            BasePage.SqlLogger.InsertAnswer(basePage.SessionID, ranker.query, AnswerSource.Wikipedia.ToString(), AnswerType.Text.ToString(), answers.Count);
            return answers;
        }

        private void ParseResponse(Stream htmlStream, Answer answer)
        {
            if (htmlStream == null)
                return;
            HtmlDocument doc = new HtmlDocument();
            doc.Load(htmlStream);
            string responseHtml = "";
            try
            {
                
                HtmlNode divNode = doc.DocumentNode.SelectSingleNode("//div[@id=\"MainTxt\"]");
                answer.question = divNode.SelectSingleNode("span[@class=\"hw\"]").InnerText;
                foreach (HtmlNode node in divNode.ChildNodes.Skip(1))
                {
                    if (node.Name == "h2")
                        break;
                    if (node.Name == "table" || 
                        (node.Name=="span" && node.Attributes.Contains("class") && node.Attributes["class"].Value == "hw") ||
                        (node.Attributes.Contains("style") && node.Attributes["style"].Value == "display: none; visibility: hidden"))
                        continue;
                    responseHtml += node.OuterHtml;
                }
            }
            catch
            {
            }
            
            answer.topAnswer = Util.SanitizeHtmlContent(responseHtml);
            return;
        }

    }
}