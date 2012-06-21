using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BingAnswers
{
    public class Answer
    {        
        public string question;
        public string topAnswer;
        public bool isStarred=false;
        public string url;
        public object image;
        public double score=0;
        public double staticScore = 0;
        public string metaData;
        public AnswerSource source;
        public AnswerType type;
        public int answerVotes=0;
        public int totalVotes=0;
        public Int64 questionFileTime=0;
        public Int64 updatedFileTime=0;
        public Features features;
        public enum AnswerSource { WolframeAlpha, YahooAnswers, MSDN, eHow, StackOverflow,TrueKnowledge, WikiHow, WikiAnswers, ChaCha, Quora, YouTube, BingImages, Wikipedia, WikipediaInfoBox };
        public enum AnswerType { Text, Video, Image }

        protected Ranker ranker;

        public virtual List<Answer> GetAnswer(BasePage basePage)
        {
            return new List<Answer>();
        }

        public string Question
        {
            get
            {
                return this.question;
            }
        }

        public string TopAnswer
        {
            get
            {
                return this.topAnswer;
            }
        }

        public bool IsStarred
        {
            get
            {
                return this.isStarred;
            }
        }

        public string Url
        {
            get
            {
                return this.url;
            }
        }

        public double Score
        {
            get
            {
                return this.score;
            }

        }

        public object Image
        {
            get
            {
                return this.image;
            }
        }

        public AnswerSource Source
        {
            get
            {
                return this.source;
            }
        }

        public Features Features
        {
            get
            {
                return this.features;
            }
        }

        public AnswerType Type
        {
            get
            {
                return this.type;
            }
        }
    }

    
}