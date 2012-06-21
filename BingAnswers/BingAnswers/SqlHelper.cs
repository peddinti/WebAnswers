using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Data.SqlClient;

namespace BingAnswers
{
    public class SqlHelper
    {
        #region SqlTemplates        
        private const string SQL_Select_Count = "Select Count({0}) FROM {1} Where {0} = '{2}'";
        private string SQLTemplate_Insert = "INSERT INTO {0} VALUES({1})";
        private const string SQL_Table_User = "[User]";
        private const string SQL_Table_Session = "[Session]";
        private const string SQL_Table_Answer = "[Answer]";
        private const string SQL_Table_Action = "[Action]";
        private const string SQL_Table_Feedback = "[Feedback]";
        #endregion
        private static SqlConnection sqlConn;

        public SqlHelper()
        {
            if (sqlConn != null)
                return;
            SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder();
            //System.Configuration.ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);
            //connBuilder.DataSource = "rm18lr51uf.database.windows.net";
            //connBuilder.InitialCatalog = "BingWebAnswers";
            //connBuilder.Encrypt = true;
            //connBuilder.TrustServerCertificate = false;
            //connBuilder.UserID = "peddinti";
            //connBuilder.Password = "BingAnswers5*";
            //sqlConn = new SqlConnection(connBuilder.ToString());
            sqlConn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString);
            try
            {
                sqlConn.Open();
            }
            catch (Exception)
            {
            }
        }

        // Properly disposing the SqlConnection class.
        ~SqlHelper()
        {
            try
            {
                sqlConn.Close();
                sqlConn.Dispose();
            }
            catch (SqlException)
            {
            }
        }

        public bool isUniqueSessionID(string _SessionID)
        {
            using (SqlCommand command = new SqlCommand(string.Format(SQL_Select_Count, "SessionID", SQL_Table_User, _SessionID), sqlConn))
            {
                try
                {
                    if (sqlConn.State == System.Data.ConnectionState.Closed)
                        sqlConn.Open();
                    return int.Parse(command.ExecuteScalar().ToString()) == 0;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool isUniqueUUID(string _UUID)
        {
            using (SqlCommand command = new SqlCommand(string.Format(SQL_Select_Count, "UUID", SQL_Table_User, _UUID), sqlConn))
            {
                try
                {
                    if (sqlConn.State == System.Data.ConnectionState.Closed)
                        sqlConn.Open();
                    return int.Parse(command.ExecuteScalar().ToString()) == 0;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public void InsertUser(string _SessionID, string _UUID, string _IP)
        {
            string insertValues = String.Format("'{0}', '{1}', '{2}', CURRENT_TIMESTAMP", _SessionID, _UUID, _IP);
            using (SqlCommand command = new SqlCommand(string.Format(SQLTemplate_Insert, SQL_Table_User, insertValues), sqlConn))
            {
                try
                {
                    if (sqlConn.State == System.Data.ConnectionState.Closed)
                        sqlConn.Open();

                    Thread thread = new Thread(() => tryExecuteNonQuery(command));
                    thread.Start();
                    //command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                }
            }
        }

        public void InsertSession(string _SessionID, string _Query, int _ResultCount, string _Response)
        {
            string insertValues = String.Format("'{0}', '{1}', '{2}', '{3}', CURRENT_TIMESTAMP", _SessionID, _Query, _ResultCount, _Response);
            using (SqlCommand command = new SqlCommand(string.Format(SQLTemplate_Insert, SQL_Table_Session, insertValues), sqlConn))
            {
                try
                {
                    if (sqlConn.State == System.Data.ConnectionState.Closed)
                        sqlConn.Open();
                    Thread thread = new Thread(() => tryExecuteNonQuery(command));
                    thread.Start();

                    //command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                }
            }
        }

        public void InsertAnswer(string _SessionID, string _Query, string _Source, string _Type, int _AnswerCount)
        {
            string insertValues = String.Format("'{0}', '{1}', '{2}', '{3}', {4}, CURRENT_TIMESTAMP", _SessionID, _Query, _Source, _Type, _AnswerCount);
            using (SqlCommand command = new SqlCommand(string.Format(SQLTemplate_Insert, SQL_Table_Answer, insertValues), sqlConn))
            {
                try
                {
                    if (sqlConn.State == System.Data.ConnectionState.Closed)
                        sqlConn.Open();
                    Thread thread = new Thread(() => tryExecuteNonQuery(command));
                    thread.Start();

                    //command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                }
            }
        }

        public void InsertAction(string _SessionID, string _Action, string _Url, int _Position)
        {
            string insertValues = String.Format("'{0}', '{1}', '{2}', {3}, CURRENT_TIMESTAMP", _SessionID, _Action, HttpUtility.UrlEncode(_Url), _Position);
            using (SqlCommand command = new SqlCommand(string.Format(SQLTemplate_Insert, SQL_Table_Action, insertValues), sqlConn))
            {
                try
                {
                    if (sqlConn.State == System.Data.ConnectionState.Closed)
                        sqlConn.Open();
                    Thread thread = new Thread(() => tryExecuteNonQuery(command));
                    thread.Start();

                    //command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                }
            }
        }

        public void InsertRating(string _SessionID, string _Query, int _Rating, string _Comments)
        {
            string insertValues = String.Format("'{0}', '{1}', '{2}', '{3}', CURRENT_TIMESTAMP", _SessionID, _Query, _Rating, _Comments);
            using (SqlCommand command = new SqlCommand(string.Format(SQLTemplate_Insert, SQL_Table_Feedback, insertValues), sqlConn))
            {
                try
                {
                    if (sqlConn.State == System.Data.ConnectionState.Closed)
                        sqlConn.Open();
                    Thread thread = new Thread(() => tryExecuteNonQuery(command));
                    thread.Start();

                    //command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                }

            }

        }

        private void tryExecuteNonQuery(SqlCommand command)
        {
            try
            {
                command.ExecuteNonQuery();
            }
            catch(Exception)
            {
            }
        }
    }
}