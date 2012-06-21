using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.IO;
using System.Threading;
using HtmlAgilityPack;
using NLog;
using WordsMatching;
using Wnlib;
using System.Xml;

namespace BingAnswers.WebSources
{
    public class Wikipedia : Answer
    {
        Regex InfoBoxRegex = new Regex("{{Infobox (\\w+)\n((\\||\\<).*\\s*\\w+\\s*.*\n)+}}", RegexOptions.Compiled);
        Regex InfoKeyValueRegex = new Regex("\\|\\s*(\\w+)\\s*=\\s*(.+)\\s*", RegexOptions.Compiled);
        Regex wikiParseTemplate = new Regex("</span>([^<>]*)<span", RegexOptions.Compiled);

        public const string siteName = "wikipedia.org";
        private const string wikiPediaParseUrlTemplate = "http://en.wikipedia.org/w/api.php?action=expandtemplates&text={0}&format=xml";        
        public const int resultsSize = 1;
        public const double threshold = 0.35;
        private List<Answer> Answers = new List<Answer>();
        private List<Answer> InfoAnswers = new List<Answer>();
        //NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public override List<Answer> GetAnswer(BasePage basePage)
        {            
            base.ranker = basePage.Ranker;

            string nnQuery = string.Join(" ", ranker.queryWords.Where((w, i) => Ranker.NNTags.Contains(ranker.queryTags.ElementAt(i))));            
            IEnumerable<WebResultsWebResult> bingResults = Util.GetBingWebResults(nnQuery, siteName).Take(resultsSize);
            if (bingResults == null)
            {
                //logger.Error("Encountered invalid bing response for query:{0}", ranker.query);
                return Answers;
            }
            ParseSearchResult(bingResults, ranker.query);
            BasePage.SqlLogger.InsertAnswer(basePage.SessionID, ranker.query, AnswerSource.Wikipedia.ToString(), AnswerType.Text.ToString(), Answers.Count);
            BasePage.SqlLogger.InsertAnswer(basePage.SessionID, ranker.query, AnswerSource.WikipediaInfoBox.ToString(), AnswerType.Text.ToString(), InfoAnswers.Count);

            List<Answer> finalAnswers = new List<Answer>();
            // adding description wiki answer only if query is what / Who and there is no answer from info box
            if ((ranker.queryWords.Contains("what") || ranker.queryWords.Contains("who")) && InfoAnswers.Count == 0)
                finalAnswers.AddRange(Answers.Take(resultsSize));
            // adding info box answers
            finalAnswers.AddRange(InfoAnswers.Take(resultsSize));

            return finalAnswers;
            
        }

        private void ParseSearchResult(IEnumerable<WebResultsWebResult> bingResults, string query)
        {            
            List<Thread> workers = new List<Thread>();
            foreach (WebResultsWebResult result in bingResults)
            {                
                // Getting the main Wiki Description
                Thread worker = new Thread(() => GetWikiAnswer(result.Url));                
                worker.Start();
                workers.Add(worker);
            }
            while (workers.Where(thread => thread.IsAlive).Count() > 0) ;

            //logger.Info("Wikipedia Result Count:{0}", answers.Count);            
        }

        private void GetWikiAnswer(string url)
        {
            List<Thread> workers = new List<Thread>();
            Stream responseStream = Util.MakeWebRequestStream(url);
            if (responseStream == null)
                return;
            HtmlDocument doc = new HtmlDocument();
            doc.Load(responseStream);

            #region getting the main text content
            Thread mainContentThread = new Thread(() => GetMainTextContent(doc, url));
            mainContentThread.Start();
            workers.Add(mainContentThread);
            #endregion

            #region getting the infobox content            
            // Getting the edit url from the original doc.
            HtmlNode editUrlNode = doc.DocumentNode.SelectSingleNode("//li[@id=\"ca-viewsource\"]/span/a");
            if (editUrlNode == null || editUrlNode.Attributes["href"].Value == null)
                return;
            string editUrl = editUrlNode.Attributes["href"].Value;
            editUrl = "http://en.wikipedia.org" + HttpUtility.HtmlDecode(editUrl);

            // Getting the Info box  
            Answer infoAns = new Answer();
            infoAns.url = url;
            Thread infoContentThread = new Thread(() => GetWikiInfoBoxAnswer(infoAns, editUrl));
            infoContentThread.Start();
            workers.Add(infoContentThread);            
            #endregion
            
            //waiting for threads to finish
            while (workers.Where(thread => thread.IsAlive).Count() > 0) ;
        }

        private void GetMainTextContent(HtmlDocument doc, string url)
        {
            Answer ans = new Answer();
            ans.url = url;
            try
            {
                HtmlNode node = doc.DocumentNode.SelectSingleNode("//div[@id=\"mw-content-text\"]");
                if (node == null)
                {
                    //logger.Error("encountered an invalid answer for url:{0}", ans.url);
                    return;
                }
                ans.topAnswer = node.SelectSingleNode("//p[string-length()>200]").InnerText.Replace("\n", "").Replace("\t", "");
                ans.question = doc.DocumentNode.SelectSingleNode("//h1[@id=\"firstHeading\"]").InnerText.Replace("\n", "").Replace("\t", "");
                ans.source = AnswerSource.Wikipedia;
                ans.type = AnswerType.Text;
                ranker.GetFeatures(ans);
                Answers.Add(ans);
            }
            catch (Exception)
            {
                //logger.Error("Encountered an exception parsing url:{0} Exception:{1}", ans.url, e.ToString());
            }

        }        

        private void GetWikiInfoBoxAnswer(Answer ans, string url)
        {
            Dictionary<string, string> InfoBoxValues = new Dictionary<string, string>();
            Dictionary<string, float> InfoKeySimValues = new Dictionary<string, float>();
            
            try
            {
                string wikiSourceWebResponse = Util.MakeWebRequest(url);
                wikiSourceWebResponse = HttpUtility.HtmlDecode(wikiSourceWebResponse);
                Match infoMatch = InfoBoxRegex.Match(wikiSourceWebResponse);
                if (!infoMatch.Success)
                    return;
                string infoBoxText = infoMatch.Groups[0].Captures[0].Value;
                
                // reading all the key value pairs in the infobox
                MatchCollection matches = InfoKeyValueRegex.Matches(infoBoxText);                
                foreach (Match match in matches)
                {
                    InfoBoxValues[match.Groups[1].Value] = match.Groups[2].Value;
                }

                // Getting keys that match user Request 
                string[] infoBoxName = InfoBoxValues["name"].ToLower().Split();
                List<MyWordInfo> queryWords = new List<MyWordInfo>();
                queryWords = ranker.queryWords.Select((word, i) => new MyWordInfo(word, PartOfSpeech.PennTag2Pos(ranker.queryTags[i]))).ToList();                
                // adding extra words depending on the WH words present in the list
                if (ranker.queryWords.Contains("when"))
                    queryWords.Add(new MyWordInfo("date", PartsOfSpeech.Unknown));
                if (ranker.queryWords.Contains("where"))
                    queryWords.Add(new MyWordInfo("place", PartsOfSpeech.Unknown));
                if (ranker.queryWords.Contains("why"))
                    queryWords.Add(new MyWordInfo("cause", PartsOfSpeech.Unknown));
                
                MyWordInfo[] simQueryWords = queryWords.Where(word => !infoBoxName.Contains(word.Word)).ToArray();
                foreach (string key in InfoBoxValues.Keys)
                {
                    MyWordInfo[] keyWords = key.Split('_').Select(k => new MyWordInfo(k, PartsOfSpeech.Unknown)).ToArray();
                    InfoKeySimValues[key] = ranker.GetWordNetSimScore(keyWords, simQueryWords, key);                    
                }
                // choosing more than one answer
                float max = InfoKeySimValues.Max(key => key.Value);
                List<string> validKeys = InfoKeySimValues.Where(key => key.Value == max && key.Value > threshold).Select(key => key.Key).ToList();
                foreach (string key in validKeys)
                {
                    Answer infoanswer = new Answer();
                    infoanswer.url = ans.url;
                    infoanswer.topAnswer = ParseWikiTextSyntax(InfoBoxValues[key]).Replace("[[", "").Replace("]]", "");
                    infoanswer.isStarred = true;
                    infoanswer.source = AnswerSource.WikipediaInfoBox;
                    infoanswer.type = AnswerType.Text;
                    infoanswer.question = InfoBoxValues["name"] + " " + key.Replace("_", " "); ;
                    infoanswer.staticScore = max;
                    ranker.GetFeatures(infoanswer);
                    //answers.Add(infoanswer);
                    InfoAnswers.Add(infoanswer);
                }

            }
            catch (Exception)
            { }
        }

        private string GetWikiEditUrl(string url)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(Util.MakeWebRequestStream(url));
            string editUrl = doc.DocumentNode.SelectSingleNode("//li[@id=\"ca-viewsource\"]/span/a").Attributes["href"].Value;
            editUrl = "http://en.wikipedia.org" + HttpUtility.HtmlDecode(editUrl);
            return editUrl;
        }

        public string ParseWikiTextSyntax(string wikiText)
        {
            string wikiPediaParseUrl = string.Format(wikiPediaParseUrlTemplate, HttpUtility.UrlEncode(wikiText));
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(Util.MakeWebRequest(wikiPediaParseUrl));
            string parsedContent = xdoc.GetElementsByTagName("expandtemplates")[0].InnerText;
            parsedContent = HttpUtility.HtmlDecode(parsedContent);
            return parsedContent;
        }        
    }
}