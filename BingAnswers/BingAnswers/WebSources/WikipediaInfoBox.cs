using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Xml;
using WordsMatching;
using Wnlib;

namespace BingAnswers.WebSources
{
    public class WikipediaInfoBox : Answer
    {
        //Regex InfoBoxRegex = new Regex("{{Infobox ([^{}]*{?{?[^{}]+}?}?[^{}]*)+}}", RegexOptions.Compiled);
        Regex InfoBoxRegex = new Regex("{{Infobox (\\w+)\n((\\||\\<).*\\s*\\w+\\s*.*\n)+}}", RegexOptions.Compiled);
        Regex InfoKeyValueRegex = new Regex("\\|\\s*(\\w+)\\s*=\\s*(.+)\\s*", RegexOptions.Compiled);
        Regex wikiParseTemplate = new Regex("</span>([^<>]*)<span", RegexOptions.Compiled);        
        private const string siteName = "wikipedia.org"; 
        private const int resultsSize = 1;
        private const string wikiPediaParseUrlTemplate = "http://en.wikipedia.org/w/api.php?action=expandtemplates&text={0}&format=xml";
        private const string WikipediaSourceUrlTemplate = "http://en.wikipedia.org/w/index.php?title={0}&action=edit";
        
        List<Answer> Answers = new List<Answer>();
        //public Dictionary<string, float[][]> DebugDict = new Dictionary<string, float[][]>();
        private string nnQuery;

        public override List<Answer> GetAnswer(BasePage basePage)
        {
            //List<Answer> answers = new List<Answer>();
            base.ranker = basePage.Ranker;
            nnQuery = string.Join(" ", ranker.queryWords.Where((w, i) => Ranker.NNTags.Contains(ranker.queryTags.ElementAt(i))));
            IEnumerable<WebResultsWebResult> bingResults = Util.GetBingWebResults(nnQuery, siteName).Take(resultsSize);
            if (bingResults == null)
            {
                //logger.Error("Encountered invalid bing response for query:{0}", ranker.query);
                return new List<Answer>();
            }
            //answers = ParseSearchResult(bingResults, ranker.query);            
            ParseSearchResult(bingResults, ranker.query);            
            //return answers;
            return Answers;
        }


        //private List<Answer> ParseSearchResult(IEnumerable<WebResultsWebResult> bingResults, string query)
        private void ParseSearchResult(IEnumerable<WebResultsWebResult> bingResults, string query)
        {
            //List<Answer> answers = new List<Answer>();
            List<Thread> workers = new List<Thread>();
            
            foreach (WebResultsWebResult result in bingResults)
            {
                Answer answer = new Answer();
                answer.url = result.Url;
                Thread worker = new Thread(() => GetWikiInfoBoxAnswer(answer));                
                worker.Start();
                workers.Add(worker);
            }
            while (workers.Where(thread => thread.IsAlive).Count() > 0) ;
            //return answers;
            //.Take(1).ToList<Answer>();
        }

        //private void GetWikiInfoBoxAnswer(Answer ans, List<Answer> answers)
        private void GetWikiInfoBoxAnswer(Answer ans)
        {
            Dictionary<string, string> InfoBoxValues = new Dictionary<string, string>();            
            Dictionary<string, float> InfoKeySimValues = new Dictionary<string, float>();            
            SentenceSimilarity semsim = new SentenceSimilarity();
            try
            {
                string articleTitle = GetWikiEditUrl(ans.url);
                if (String.IsNullOrEmpty(articleTitle))
                    return;
                string wikiSourceWebResponse = Util.MakeWebRequest(articleTitle);
                wikiSourceWebResponse = HttpUtility.HtmlDecode(wikiSourceWebResponse);                
                string infoBoxText = InfoBoxRegex.Match(wikiSourceWebResponse).Groups[0].Captures[0].Value;
                // reading all the key value pairs in the infobox
                MatchCollection matches = InfoKeyValueRegex.Matches(infoBoxText);
                foreach (Match match in matches)
                {
                    InfoBoxValues[match.Groups[1].Value] = match.Groups[2].Value;
                }

                // Getting keys that match user Request 
                string[] infoBoxNames = InfoBoxValues["name"].ToLower().Split();                
                List<MyWordInfo> words = new List<MyWordInfo>();
                for(int i=0; i < ranker.queryWords.Count; i++)
                {
                    words.Add(new MyWordInfo(ranker.queryWords[i],PartOfSpeech.PennTag2Pos(ranker.queryTags[i])));
                }
                
                // adding extra words depending on the WH words present in the list
                if (ranker.queryWords.Contains("when"))
                    words.Add(new MyWordInfo("date", PartsOfSpeech.Unknown));                    
                if (ranker.queryWords.Contains("where"))
                    words.Add(new MyWordInfo("place", PartsOfSpeech.Unknown));                    
                if (ranker.queryWords.Contains("why"))
                    words.Add(new MyWordInfo("cause", PartsOfSpeech.Unknown));                    
                MyWordInfo[] simQueryWords = words.Where(word => !infoBoxNames.Contains(word.Word)).ToArray();                
                foreach (string key in InfoBoxValues.Keys)
                {
                    MyWordInfo[] keyWords = key.Split('_').Select(k => new MyWordInfo(k, PartsOfSpeech.Unknown)).ToArray();                    
                    InfoKeySimValues[key] = ranker.GetWordNetSimScore(keyWords,simQueryWords, key);
                    //InfoKeySimValues[key] = GetSimScore(semsim.GetSimilarityMatrix(key.Split('_'), simQuery.ToArray()));
                }
                // choosing more than one answer
                float max = InfoKeySimValues.Max(key => key.Value);
                List<string> validKeys = InfoKeySimValues.Where(key => key.Value == max).Select(key => key.Key).ToList();
                foreach (string key in validKeys)
                {
                    Answer infoanswer = new Answer();
                    infoanswer.url = ans.url;
                    infoanswer.topAnswer = ParseWikiText(InfoBoxValues[key]).Replace("[[", "").Replace("]]", "");
                    infoanswer.isStarred = true;
                    infoanswer.source = AnswerSource.Wikipedia;
                    infoanswer.type = AnswerType.Text;
                    infoanswer.question = InfoBoxValues["name"]  + " " + key.Replace("_", " ");;
                    ranker.GetFeatures(infoanswer);
                    //answers.Add(infoanswer);
                    Answers.Add(infoanswer);
                }
                
            }
            catch(Exception e)
            {}
        }

        private string GetWikiEditUrl(string url)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(Util.MakeWebRequestStream(url));
            string editUrl = doc.DocumentNode.SelectSingleNode("//li[@id=\"ca-viewsource\"]/span/a").Attributes["href"].Value;
            editUrl = "http://en.wikipedia.org" + HttpUtility.HtmlDecode(editUrl);
            return editUrl;
        }
        public string ParseWikiText(string wikiText)
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