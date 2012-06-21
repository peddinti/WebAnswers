using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;

namespace BingAnswers
{
    public partial class trainer : BasePage
    {
        public void Page_Load(object sender, EventArgs e)
        {
            string query = "";
            // the query comes always from the url parameter
            query = HttpContext.Current.Request.Params["q"];            

            if (!string.IsNullOrWhiteSpace(query))
            {
                this.GetAnswers(query);                

                // wait for all threads to finish
                List<Thread> remThreads = new List<Thread>();

                while (workers.Where(thread => thread.IsAlive).Count() > 0)
                {
                    //remThreads = new List<Thread>();
                    //foreach (Thread worker in workers)
                    //    if (!worker.IsAlive)
                    //    {
                    //        remThreads.Add(worker);                            
                    //    }
                    //foreach (Thread worker in remThreads)
                    //    workers.Remove(worker);
                }

                // sorting the results based on score
                FinalAnswers.Sort(delegate(Answer a1, Answer a2) { return a2.score.CompareTo(a1.score); });
                trainerRep.DataSource = FinalAnswers;                
                trainerRep.DataBind();
                this.DataBind();
            }

        }
    }
}