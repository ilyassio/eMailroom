using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;


namespace eMailroom.Models
{
    public static class Database
    {
        private static readonly string StringConnection = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + HttpContext.Current.Server.MapPath("~/App_Data/eMailroomDatabase.mdf") + ";Integrated Security=True";
        public static SqlConnection Cnx;
        public static SqlCommand Cmd;
        public static SqlDataReader reader;


        private static bool Open()
        {
            Cnx = new SqlConnection(StringConnection);
            Cmd = new SqlCommand();
            try
            {
                Cnx.Open();
                Cmd.Connection = Cnx;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Close()
        {
            Cnx.Close();
        }

        public static SqlDataReader ExecuteDqlQuery(string query)
        {
            if (Open())
            {
                try
                {
                    Cmd.CommandText = query;

                    reader = Cmd.ExecuteReader();
                    return reader;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
                return null;
        }

        public static SqlDataReader ExecuteDqlQuery(string query, string[] param, object[] paramValue)
        {
            if (Open() && param.Length == paramValue.Length)
            {
                try
                {
                    Cmd.CommandText = query;

                    for (int i = 0; i < param.Length ; i++)
                    {
                        Cmd.Parameters.Add(new SqlParameter(param[i], paramValue[i]));
                    }

                    reader = Cmd.ExecuteReader();
                    return reader;
                }
                catch
                {
                    return null;
                }

            }
            else
                return null;

        }

        public static int ExecuteDmlQuery(string query)
        {
            int nbrOfRowsChanged = -1;

            if (Open())
            {
                try
                {
                    Cmd.CommandText = query;
                    nbrOfRowsChanged = Cmd.ExecuteNonQuery();
                }
                catch
                {

                }
                finally
                {
                    Database.Close();
                }
            }
            
            return nbrOfRowsChanged;
        }

        public static int ExecuteDmlQuery(string query, string[] param, object[] paramValue)
        {
            int nbrOfRowsChanged = -1;

            if (Open() && param.Length == paramValue.Length)
            {
                try
                {
                    Cmd.CommandText = query;

                    for (int i = 0; i < param.Length; i++)
                        Cmd.Parameters.Add(new SqlParameter(param[i], (object)paramValue[i]));

                    nbrOfRowsChanged = Cmd.ExecuteNonQuery();
                }
                catch
                {
                    
                }
                finally
                {
                    Database.Close();
                }
            }

            return nbrOfRowsChanged;
        }

    }
}