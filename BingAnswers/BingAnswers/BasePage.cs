using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using System.Timers;
using NLog;
using BingAnswers.WebSources;

namespace BingAnswers
{
    public class BasePage : System.Web.UI.Page
    {
        //NLog.Logger logger = LogManager.GetLogger("BingAnswers.Answers");
        public List<Thread> workers = new List<Thread>();
        public static readonly object locker = new object();
        public List<Answer> FinalAnswers;
        public static SqlHelper SqlLogger;
        public string UUID;
        public string SessionID;
        protected static int Interval = int.MaxValue;
        public Ranker Ranker = new Ranker("");
        bool timeOut = false;

        public void GetAnswers(string query)
        {
            // Query alteration should be done before anything else
            query = QueryAlternation.Speller(query);
            FinalAnswers = new List<Answer>();
            workers = new List<Thread>();            
            // creating a ranker
            Ranker = new Ranker(query);            
            //Ranker.query = query;

            // Initializing 
            Initialize();

            //logger.Trace("Initializing Stack Overflow thread");
            StackOverflow so = new StackOverflow();
            Thread SOThread = new Thread(() => GetSource(so, this));
            SOThread.Name = "Stack Overflow Thread";
            workers.Add(SOThread);
            SOThread.Start();

            //logger.Trace("Initializing Yahoo Answers thread");
            YahooAnswers ya = new YahooAnswers();
            Thread yaThread = new Thread(() => GetSource(ya, this));
            yaThread.Name = "Yahoo Answers Thread";
            workers.Add(yaThread);
            yaThread.Start();

            //logger.Trace("Initializing MSDN thread");
            MSDNForum msdn = new MSDNForum();
            Thread msdnThread = new Thread(() => GetSource(msdn, this));
            msdnThread.Name = "MSDN thread";
            workers.Add(msdnThread);
            msdnThread.Start();

            //logger.Trace("Initializing Wolframe Alpha thread");
            WolframAlpha wa = new WolframAlpha();
            Thread waThread = new Thread(() => GetSource(wa, this));
            waThread.Name = "Wolframe Thread";
            workers.Add(waThread);
            waThread.Start();

            //logger.Trace("Initializing True Knowledge thread");
            TrueKnowledge tn = new TrueKnowledge();
            Thread tnThread = new Thread(() => GetSource(tn, this));
            tnThread.Name = "True Knowledge Answers Thread";
            workers.Add(tnThread);
            tnThread.Start();

            //logger.Trace("Initializing ehow thread");
            ehow eh = new ehow();
            Thread eThread = new Thread(() => GetSource(eh, this));
            eThread.Name = "Ehow thread";
            eThread.Start();
            workers.Add(eThread);

            //logger.Trace("Initializing WikiHow thread");
            WikiHow wh = new WikiHow();
            Thread wThread = new Thread(() => GetSource(wh, this));
            wThread.Name = "Wikihow thread";
            workers.Add(wThread);
            wThread.Start();

            //logger.Trace("Initializing Wiki Answers thread");
            WikiAnswers waa = new WikiAnswers();
            Thread waaThread = new Thread(() => GetSource(waa, this));
            waaThread.Name = "WikiAnswers Thread";
            workers.Add(waaThread);
            waaThread.Start();

            //logger.Trace("Initializing ChaCha thread");
            ChaCha ca = new ChaCha();
            Thread caThread = new Thread(() => GetSource(ca, this));
            caThread.Name = "ChaChaAnswers Thread";
            workers.Add(caThread);
            caThread.Start();

            //logger.Trace("Initializing Quora thread");
            Quora qa = new Quora();
            Thread qaThread = new Thread(() => GetSource(qa, this));
            qaThread.Name = "Quora Thread";
            workers.Add(qaThread);
            qaThread.Start();

            ////logger.Trace("Initializing Wikipedia Encyclopedia thread");
            //WikiEncyclopedia we = new WikiEncyclopedia();
            //Thread weThread = new Thread(() => GetSource(we, ranker));
            //weThread.Name = "Wiki Encyclopedia Thread";
            //workers.Add(weThread);
            //weThread.Start();

            //logger.Trace("Initializing Wikipedia thread");
            Wikipedia wiki = new Wikipedia();
            Thread wikiThread = new Thread(() => GetSource(wiki, this));
            wikiThread.Name = "Wikipedia Thread";
            workers.Add(wikiThread);
            wikiThread.Start();            

            //logger.Trace("Initializing YouTube thread");
            YouTube yt = new YouTube();
            Thread ytThread = new Thread(() => GetSource(yt, this));
            ytThread.Name = "YouTube Thread";
            workers.Add(ytThread);
            ytThread.Start();

            //logger.Trace("Initializing Bing Images Thread");
            BingImages bi = new BingImages();
            Thread btThread = new Thread(() => GetSource(bi, this));
            btThread.Name = "Bing Images Thread";
            workers.Add(btThread);
            btThread.Start();

            //logger.Trace("End of querying answers");

            // wait for all threads to finish
            List<Thread> remThreads = new List<Thread>();

            System.Timers.Timer time = new System.Timers.Timer();
            time.Interval = Interval;
            time.Elapsed += TimeOut;
            time.Enabled = true;
            // waiting for workers to end or time out
            while (workers.Where(thread => thread.IsAlive).Count() > 0 && !timeOut) ;
            if (timeOut)
            {
                workers.ForEach(worker => worker.Abort());                
            }

            PostInitialize();
        }

        private static void GetSource(Answer SourceObject, BasePage basePage)
        {
            List<Answer> localAnswers = SourceObject.GetAnswer(basePage);
            if (localAnswers != null && localAnswers.Count > 0)
            {
                lock (locker)
                {
                    basePage.FinalAnswers.InsertRange(0, localAnswers);
                }
            }
        }

        private void TimeOut(object sender, ElapsedEventArgs e)
        {
            System.Timers.Timer timer = (System.Timers.Timer)sender;
            timer.Enabled = false;
            timeOut = true;
        }

        protected void Initialize()
        {
            if (SqlLogger == null)
                SqlLogger = new SqlHelper();
            
            if (Request.Cookies["UUID"] == null || string.IsNullOrEmpty(Request.Cookies["UUID"].Value.ToString()))
                SetUUID();
            else
                UUID = Request.Cookies["UUID"].Value.ToString();

            // Every time its a new Session
            SetSessionID();                        
        }

        protected void PostInitialize()
        {
            SqlLogger.InsertUser(SessionID, UUID, Request.UserHostAddress);
            SqlLogger.InsertSession(SessionID, Ranker.query, FinalAnswers.Count(), "");
        }

        private void SetUUID()
        {
            string uuid = System.Guid.NewGuid().ToString();
            int maxTry = 5;
            while(!SqlLogger.isUniqueUUID(uuid) && maxTry >= 0)
            {
                // try getting a new GUID if the old one conflicts
                uuid = System.Guid.NewGuid().ToString();
                maxTry -= 1;
            }
            Response.Cookies["UUID"].Value = uuid;
            Response.Cookies["UUID"].Expires = DateTime.MaxValue;
            UUID = uuid;
        }

        private void SetSessionID()
        {
            string sessionID = System.Guid.NewGuid().ToString();
            int maxTry = 5;
            while (!SqlLogger.isUniqueSessionID(sessionID) && maxTry >= 0)
            {
                sessionID = System.Guid.NewGuid().ToString();
                maxTry -= 1;
            }
            Session["SessionID"] = sessionID;
            SessionID = sessionID;
        }

        protected enum Action
        {
            Click,
            Like,
            Share
        }

        protected void SaveSession()
        {
            Session["BasePage"] = this;
            Session["Answers"] = this.FinalAnswers;
            
        }

        protected void LoadSession()
        {
            BasePage sessionBasePage = (BasePage)Session["BasePage"];
            this.FinalAnswers = (List<Answer>)Session["Answers"];
            this.SessionID = sessionBasePage.SessionID;
            this.UUID = sessionBasePage.UUID;
            this.Ranker = sessionBasePage.Ranker;            
        }
    }
}