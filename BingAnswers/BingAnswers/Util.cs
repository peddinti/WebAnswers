using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Net;
using System.Web;
using NLog;

namespace BingAnswers
{
    public static class Util
    {
        private const string api_key = "F861FD792AA88860B6BAE5C4CD21CC19742E11FA";
        public static string bing_url = "http://api.bing.net/xml.aspx?Appid=" + api_key + "&query={0}&sources={1}";
        public const int resultsSize = 3;

        public static int maxQueryAttempts = 5;
        // Regex for catching unquoted keys in JSON
        private static Regex unquotedJsonRegex = new Regex(@"(?<=\s*)[^{}\[\]:,\s]+?(?=\s*:)", RegexOptions.Compiled);

        /// <summary>
        /// Convert a JSON string to an XmlReader.  The XmlReader maps JSON
        /// more-or-less as you would expect.  Of note, items in JSON lists
        /// (which, of course, don't have a name) become &lt;item&gt;
        /// elements in the XML.
        /// </summary>
        /// <param name="jsonString">The JSON to XML-ify.</param>
        /// <returns>An XmlReader on the XML-ified JSON.</returns>
        public static XmlReader JsonToXml(string jsonString)
        {
            Stream jsonStream = new MemoryStream(jsonString.Length);
            StreamWriter jsonStreamWriter = new StreamWriter(jsonStream);
            jsonStreamWriter.Write(jsonString);
            jsonStreamWriter.Flush();
            jsonStream.Position = 0;

            try
            {
                XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(jsonStream, XmlDictionaryReaderQuotas.Max);
                return reader;
            }
            catch
            {
                return XmlReader.Create(new StringReader(@"<Error><Message>The supplied JSON was not properly formed.</Message>" +
                    "<originalJSONText><![CDATA[" + jsonString + "]]></originalJSONText></Error>"));
            }
        }

        /// <summary>
        /// Certain teams at Bing (Wrapstar and the instrumentation folks,
        /// among others) are distressingly fond of omitting quotes from
        /// JSON keys.  For example, instead of { "Foo":"Bar" }, they
        /// might provide { Foo:"Bar" }.  This is invalid JSON, and will
        /// break the .NET libraries used in JsonToXml, among other things.
        /// 
        /// This method fixes such JSON.  It requires some computation, so
        /// it should only be used if necessary.
        /// </summary>
        /// <param name="jsonString">The JSON string to fix.</param>
        /// <returns>The fixed JSON string.</returns>
        public static string FixUnquotedJson(string jsonString)
        {
            // A StringBuilder to hold the fixed JSON, with a little extra room
            // for the new quotes.
            StringBuilder fixedJsonBuilder = new StringBuilder((int)(jsonString.Length * 1.1));

            // A StringBuilder to hold strings outside quotes for processing.
            // These strings will be repaired, if necessary.  For example, with
            // the JSON { "Foo":"Bar", Baz:"Qux" }, this StringBuilder would hold
            // '{ ', ':', ', Baz:', and ' }' in turn.  ', Baz:' would be repaired.
            StringBuilder unquotedChunkBuilder = new StringBuilder();

            // Are we currently inside a quoted string?
            bool insideQuotes = false;

            // Are we in escape mode?  ('\')
            bool escaping = false;

            foreach (char c in jsonString)
            {
                if (insideQuotes)
                {
                    fixedJsonBuilder.Append(c);
                    if (c == '\\')
                    {
                        // Will set "escaping" to true, unless we were already escaping.
                        // If we were, that means we had "\\" -- escaping a backslash --
                        // so turn off escaping.
                        escaping = !escaping;
                    }
                    else if (c == '"' && !escaping)
                    {
                        insideQuotes = false;
                    }
                    escaping = false;
                }
                else // not inside quotes
                {
                    if (c == '"')
                    {
                        insideQuotes = true;
                        string fixedChunk = FixUnquotedJsonChunk(unquotedChunkBuilder.ToString());
                        unquotedChunkBuilder.Length = 0;
                        fixedJsonBuilder.Append(fixedChunk);
                        fixedJsonBuilder.Append(c);
                    }
                    else // still not inside quotes
                    {
                        unquotedChunkBuilder.Append(c);
                    }
                }
            }

            // Get the last bit of text
            fixedJsonBuilder.Append(unquotedChunkBuilder.ToString());

            return fixedJsonBuilder.ToString();
        }

        private static string FixUnquotedJsonChunk(string jsonChunk)
        {
            return unquotedJsonRegex.Replace(jsonChunk, match => "\"" + match.Value + "\"");
        }

        public static string SanitizeJson(string jsonChunk)
        {
            return jsonChunk.Replace('\r',' ').Replace('\n',' ');
        }

        public static string MakeWebRequest(string url)
        {
            int attempts = 0;
            while (attempts <= maxQueryAttempts)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
                    request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.120 Safari/535.2";
                    request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                    request.Headers.Add("Accept-Language", "en-US,en;q=0.8");
                    request.AutomaticDecompression = DecompressionMethods.GZip;
                    request.AllowAutoRedirect = false;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
                    {
                        return responseReader.ReadToEnd();
                    }
                }
                catch (WebException)
                {
                    attempts += 1;
                }
            }
            return null;
        }

        public static Stream MakeWebRequestStream(string url)
        {
            int attempts = 0;
            while (attempts <= maxQueryAttempts)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
                    request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.120 Safari/535.2";
                    request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                    request.Headers.Add("Accept-Language", "en-US,en;q=0.8");
                    request.AutomaticDecompression = DecompressionMethods.GZip;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    return response.GetResponseStream();
                }
                catch (WebException)
                {
                    attempts += 1;
                }
            }
            return null;
        }

        public static IEnumerable<WebResultsWebResult> GetBingWebResults(string query, string site)
        {
            SearchResponse response = SendBingSearchRequest(query, site, "web");
            if (response.Web == null || response.Web.Results == null || response.Web.Results.Count() == 0)
                return null;
            return response.Web.Results.Take(resultsSize);
        }

        public static SearchResponse SendBingSearchRequest(string query, string site, string source)
        {
            if (!string.IsNullOrEmpty(site))
                query += " site:" + site;
            Stream bingResponseStream = Util.MakeWebRequestStream(String.Format(bing_url, HttpUtility.UrlEncode(query), source));
            XmlSerializer serializer = new XmlSerializer(typeof(SearchResponse));            
            SearchResponse searchResponse = (SearchResponse)serializer.Deserialize(bingResponseStream);
            return searchResponse;
        }

        public static int LevenshteinDistance(string[] s1, string[] s2)
        {
            // if one of the arrays is empty
            if (s1.Length == 0 || s2.Length == 0)
                return Math.Max(s1.Length, s2.Length);
                        
            int[,] distanceMatrix = new int[s1.Length + 1, s2.Length + 1];
            // initializing
            for (int i = 0; i <= distanceMatrix.GetUpperBound(0); i += 1)
                distanceMatrix[i, 0] = i;
            for (int i = 0; i <= distanceMatrix.GetUpperBound(1); i += 1)
                distanceMatrix[0, i] = i;

            // calculating the distance
            for(int i = 1;i<=distanceMatrix.GetUpperBound(0); i+= 1)
                for (int j = 1; j <= distanceMatrix.GetUpperBound(1); j += 1)
                {
                    int localCost = Convert.ToInt16(!(s1[i - 1] == s2[j - 1]));
                    int hcost = distanceMatrix[i - 1, j] + 1;
                    int vcost = distanceMatrix[i, j - 1] + 1;
                    int dcost = distanceMatrix[i - 1, j - 1] + localCost;
                    distanceMatrix[i, j] = Math.Min(Math.Min(hcost, vcost), dcost);

                }

            return distanceMatrix[distanceMatrix.GetUpperBound(0), distanceMatrix.GetUpperBound(1)];
        }

        public static double CosineDistance(string[] s1, string[] s2)
        {
            //getting feature vector
            List<string> feature = new List<string>();
            feature.AddRange(s1);
            foreach (string word in s2)
                if (!feature.Contains(word))
                    feature.Add(word);
            // Getting Histogram
            Dictionary<string,int> s1Hist = new Dictionary<string,int>();
            foreach (string word in s1)
                if(s1Hist.Keys.Contains(word))
                    s1Hist[word] += 1;
            else
                s1Hist[word] = 1;
            
            Dictionary<string,int> s2Hist = new Dictionary<string,int>();
            foreach (string word in s2)
                if(s2Hist.Keys.Contains(word))
                    s2Hist[word] += 1;
            else
                s2Hist[word] = 1;
            // Creating feature vector            
            double numSum = 0;
            double s1Sum = 0;
            double s2Sum = 0;
            foreach (string word in feature)
            {                
                if(s1Hist.Keys.Contains(word) && s2Hist.Keys.Contains(word))
                {
                    numSum += s1Hist[word]*s2Hist[word];
                    s1Sum += Math.Pow(s1Hist[word],2);
                    s2Sum += Math.Pow(s2Hist[word],2);
                }
                else if(s1Hist.Keys.Contains(word))
                    s1Sum += Math.Pow(s1Hist[word], 2);
                else if (s2Hist.Keys.Contains(word))
                    s2Sum += Math.Pow(s2Hist[word], 2);
            }
            return numSum / (Math.Sqrt(s1Sum) * Math.Sqrt(s2Sum));
                
                

        }

        public static string SanitizeHtmlContent(string content)
        {
            // removing <a> links
            content = Regex.Replace(content, "<a(.+?)</a>", "");
            // removing references
            content = Regex.Replace(content, "<sup(.+?)</sup>", "");

            return content;
        }
    }
}
