using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using OpenNLP.Tools.PosTagger;
using SharpEntropy;
using Wnlib;
using WordsMatching;

namespace BingAnswers
{
    public class Ranker
    {
        public static readonly Dictionary<Answer.AnswerSource, double> sourceWeights = new Dictionary<Answer.AnswerSource, double>()
        {
            {Answer.AnswerSource.WolframeAlpha,100},
            {Answer.AnswerSource.TrueKnowledge, 100},
            {Answer.AnswerSource.WikipediaInfoBox, 30},
            {Answer.AnswerSource.Wikipedia, 20},
            {Answer.AnswerSource.YouTube, 6},
            {Answer.AnswerSource.BingImages, 6},
            {Answer.AnswerSource.Quora, 6},
            {Answer.AnswerSource.WikiHow, 6},
            {Answer.AnswerSource.WikiAnswers, 4},
            {Answer.AnswerSource.ChaCha, 4},
            {Answer.AnswerSource.eHow,6},
            {Answer.AnswerSource.MSDN,4},
            {Answer.AnswerSource.StackOverflow,4},
            {Answer.AnswerSource.YahooAnswers,4},
        };


        public static readonly List<string> irrelevantTags = new List<string>(new string[] { "DT", "SYM", "TO", "IN" });
        public static readonly List<string> WHTags = new List<string>(new string[] { "WDT", "WP", "WP$", "WRB" });
        // Looking only for proper nouns
        //public static readonly List<string> NNTags = new List<string>(new string[] { "NN", "NNS", "NNP", "NNPS" });
        public static readonly List<string> NNTags = new List<string>(new string[] { "NNP", "NNPS" });
        public static readonly List<string> VBTags = new List<string>(new string[] { "VB", "VBD", "VBG", "VBN", "VBP", "VBZ" });
        public static readonly List<string> JJTags = new List<string>(new string[] { "JJ", "JJR", "JJS" });
        public static readonly string spechialCharPattern = "[\\[\\]\\(\\)\\{\\}'\"\\?\\*\\+\\.\\|\\^,]";
        // query specific global variables
        public string query;
        public List<string> queryWords;
        public List<string> queryTags;
        public List<string> relevantQueryTerms;

        public static MaximumEntropyPosTagger tagger;
        public static readonly string dataPath = AppDomain.CurrentDomain.BaseDirectory + "Data";
        public static readonly string WordNetPath = dataPath + "\\WordNet\\";
        public static readonly string ModelLocation = dataPath + "\\EnglishPOS.nbin";
        public static readonly string dictionaryLocation = dataPath + "\\tagdict";
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0,DateTimeKind.Utc);

        public SentenceSimilarity SentenceSim = new SentenceSimilarity();

        public Ranker(string query)
        {            
            if (tagger == null)
                tagger = new MaximumEntropyPosTagger(new GisModel(new SharpEntropy.IO.BinaryGisModelReader(ModelLocation)), new DefaultPosContextGenerator(), new PosLookupList(dictionaryLocation));
            Regex r = new Regex("[\\[\\]\\(\\)\\{\\}'\"\\?\\*\\+\\.\\|\\^,]", RegexOptions.IgnoreCase);
            // retaining the case for tagging
            this.queryTags = tagger.Tag(query.Split(' ')).ToList<string>();
            this.query = query.ToLower();
            queryWords = r.Replace(this.query, "").Split(' ').ToList<string>();            
            this.relevantQueryTerms = this.queryWords.Where((s, i) => !Ranker.irrelevantTags.Contains(this.queryTags[i])).ToList<string>();
            // setting the 
            Wnlib.WNCommon.path = WordNetPath;         
        }

        public void GetFeatures(Answer answer)
        {
            try
            {
                if (answer == null)
                    return;
                answer.features = new Features();
                answer.features.titleDifference = Util.CosineDistance(answer.question.ToLower().Split(' '), this.query.ToLower().Split(' '));
                answer.features.titleLengthDifference = Math.Abs(answer.question.Length - this.query.Length);
                answer.features.queryInTitle = answer.question.Contains(this.query) ? 1 : 0;
                answer.features.queryInResponse = answer.TopAnswer.Contains(this.query) ? 1 : 0;
                int questionLength = answer.question.Split(' ').Length;
                int queryLength = this.query.Split(' ').Length;
                answer.features.questionLengthRatio = (double)Math.Min(questionLength, queryLength) / Math.Max(questionLength, queryLength);
                answer.features.responseLength = answer.topAnswer.Split(' ').Length;

                if (answer.questionFileTime != 0)
                    answer.features.creationTimeDifference = (DateTime.Now - DateTime.FromFileTimeUtc(answer.questionFileTime)).Days;
                else
                    answer.features.creationTimeDifference = 500;// a average number to obtain final score for this as 0.5
                if (answer.updatedFileTime != 0)
                    answer.features.lastResponseTimeDifference = (DateTime.Now - DateTime.FromFileTimeUtc(answer.updatedFileTime)).Days;
                else
                    answer.features.lastResponseTimeDifference = 500;// a average number to obtain final score for this as 0.5
                answer.features.responseVotes = answer.answerVotes;
                answer.features.totalVotes = answer.totalVotes;
                answer.features.voteRatio = (answer.answerVotes + 1) / (answer.totalVotes + 1);
                answer.features.sourceWeight = (sourceWeights.Keys.Contains(answer.source)) ? sourceWeights[answer.source] : 0;
                answer.features.containsSteps = (answer.topAnswer.Contains("<li>")) ? 1 : 0;
                Regex r = new Regex("<li>", RegexOptions.IgnoreCase);
                answer.features.noSteps = r.Matches(answer.TopAnswer).Count;

                r = new Regex(spechialCharPattern, RegexOptions.IgnoreCase);
                string[] questionwords = r.Replace(answer.question.ToLower(), "").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                List<string> questionTags = tagger.Tag(questionwords).ToList<string>();
                // Relevant Question words are words with stop words removed
                List<string> relevantQuestionWords = questionwords.Where((s, i) => !Ranker.irrelevantTags.Contains(questionTags[i])).ToList<string>();
                // feature to see how many words are relevant
                answer.features.relevantWordsRatio = (double)questionwords.Where(s => this.relevantQueryTerms.Contains(s)).Count() / questionwords.Count();
                // check if the question words contain the query words
                answer.features.relevantStringPresent = string.Join(" ", relevantQuestionWords).Contains(String.Join(" ", this.relevantQueryTerms)) ? 1 : 0;
                answer.features.relevantStringPresent = string.Join(" ", this.relevantQueryTerms).Contains(String.Join(" ", relevantQuestionWords)) ? 1 : 0;
                answer.features.relevantStringPresent = Math.Max(answer.features.relevantStringPresent, 1);

                answer.features.questionWordsMatchCount = questionwords.Where((s, i) => WHTags.Contains(questionTags[i])).Intersect(this.queryWords.Where((s, i) => WHTags.Contains(queryTags[i]))).Count();

                // Check if the Noun Words present in the query are present in the question
                List<string> NounQuestionWords = questionwords.Where((s, i) => Ranker.NNTags.Contains(questionTags[i])).ToList<string>();
                List<string> NounQueryWords = this.queryWords.Where((s, i) => WHTags.Contains(this.queryTags[i])).ToList<string>();
                answer.features.nounWordsPresent = string.Join(" ", NounQuestionWords).Contains(String.Join(" ", NounQueryWords)) ? 1 : 0;

                // Check if the verb Words present in the query are present in the question
                List<string> VerbQuestionWords = questionwords.Where((s, i) => Ranker.VBTags.Contains(questionTags[i])).ToList<string>();
                List<string> VerbQueryWords = this.queryWords.Where((s, i) => Ranker.VBTags.Contains(this.queryTags[i])).ToList<string>();
                // computing common verb counts; if either question or query doesnt have a verb then we give it a score of 1
                if (VerbQuestionWords.Count() == 0 || VerbQueryWords.Count() == 0)
                    answer.features.verbWordsPresent = 1;
                else
                    answer.features.verbWordsPresent = VerbQueryWords.Intersect(VerbQuestionWords).Count() / (VerbQueryWords.Count() + VerbQuestionWords.Count() + 0.0);

                answer.features.staticScore = answer.staticScore;

                answer.score = GetScore(answer.features);
            }
            catch (Exception)
            {
                answer.score = 0;
            }
        }

        private static double GetScore(Features features)
        {
            double score = 0;
            score += 1 * features.sourceWeight;
            score += 25 * features.titleDifference;
            score += 20 * features.relevantWordsRatio;
            score += 20 * features.queryInTitle;
            score += 20 * features.queryInResponse;
            score += 20 * features.relevantStringPresent;
            score += 15 * features.verbWordsPresent;
            score += 10 * features.questionWordsMatchCount;            
            score += 5 *  features.containsSteps;
            score += 5 * features.nounWordsPresent;
            score += 5 * features.questionLengthRatio;
            score += 5 * features.staticScore;
            
            score += 3 * Math.Max(1 - (features.titleLengthDifference/100),0);
            score += 0.5 * Math.Max(1 - (features.creationTimeDifference/1000), 0);
            score += 0.5 * Math.Min(1 - (features.lastResponseTimeDifference/1000), 0);            
            score += 3 * Math.Max(1 - (features.responseLength / 20), 0);
            score += 2 * Math.Max(1 - (features.voteRatio / 20), 0);
            score += 2 * Math.Max(1 - (features.responseVotes / 500), 0);
            score += 5 * Math.Max(1 - (features.noSteps / 10), 0);            
            
            return score;
        }

        public float GetSimScore(float[][] simMatrix)
        {
            float score = simMatrix.Select(s => s.Select(ls => ls * (ls - s.Average())).Max()).Average();
            return score;
        }

        public float GetWordNetSimScore(MyWordInfo[] words1, MyWordInfo[] words2, string key)
        {
            float[][] simMatrix = SentenceSim.GetSimilarityMatrix(words1, words2);
            return GetSimScore(simMatrix);
        } 

    }

    public class Features
    {
        public double titleDifference;
        public double titleLengthDifference;
        public int queryInTitle;
        public int queryInResponse;
        public double creationTimeDifference;
        public double lastResponseTimeDifference;
        public double questionLengthRatio;
        public int responseLength;
        public int responseVotes;
        public double voteRatio;
        public int totalVotes;
        public double sourceWeight;
        public int containsSteps;
        public int noSteps;
        public double relevantWordsRatio;
        public int relevantStringPresent;
        public int questionWordsMatchCount;
        public int nounWordsPresent;
        public double verbWordsPresent;
        public double staticScore;
        //public List<double> FeatureVector()
        //{
        //    List<double> fv = new List<double>();
        //    fv.Add(titleDifference);
        //    fv.Add(titleLengthDifference);
        //    fv.Add(queryInTitle);
        //    fv.Add(queryInResponse);
        //    fv.Add(creationTimeDifference);
        //    fv.Add(lastResponseTimeDifference);
        //    fv.Add(questionLength);
        //    fv.Add(responseLength);
        //    fv.Add(responseVotes);
        //    fv.Add(voteRatio);
        //    fv.Add(totalVotes);
        //    fv.Add(sourceWeight);
        //    fv.Add(containsSteps);
        //    fv.Add(noSteps);
        //}
        public string featureString
        {
            get
            {
                return titleDifference + "$$$$$" + titleLengthDifference + "$$$$$" + queryInTitle + "$$$$$" + queryInResponse + "$$$$$" + creationTimeDifference + "$$$$$" + lastResponseTimeDifference + "$$$$$" + questionLengthRatio + "$$$$$" + responseLength + "$$$$$" + responseVotes + "$$$$$" + totalVotes + "$$$$$" + sourceWeight + "$$$$$" + containsSteps + "$$$$$" + noSteps + "$$$$$" + relevantWordsRatio + "$$$$$" + relevantStringPresent + "$$$$$" + questionWordsMatchCount + "$$$$$" + nounWordsPresent + "$$$$$" + verbWordsPresent + "$$$$$" + staticScore;
            }
        }
    }    
}