using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Xml;

namespace BingAnswers.WebSources
{
    public class ChaCha_API : Answer
    {
        public override List<Answer> GetAnswer(BasePage basePage)
        {
            url = "http://query.chacha.com/answer/search.json?query=How+Old+is+Brad+Pitt&pageSize=10";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.120 Safari/535.2";
            request.Headers.Add("apikey", "jvwj5ttfhjqych42gm34xxyu");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            XmlDocument xdoc = new XmlDocument();
            using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
            {
                responseReader.ReadToEnd();                
                xdoc.Load(responseReader);
            }
            Answer answer = new Answer();
            List<Answer> answers = new List<Answer>();
            answers.Add(answer);
            return answers;
        }
    }
}