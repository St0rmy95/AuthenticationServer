using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace AuthenticationServer.Database
{
    public class SQL
    {
        SqlConnection myConnection;
        public void SQLConnect(string ip, string user, string password)
        {
            myConnection = new SqlConnection("user id=" + user + ";" +
                                       "password=" + password + ";server=" + ip + ";" +
                                       "Trusted_Connection=no;" +
                                       "connection timeout=10");
            try
            {
                myConnection.Open();
            }
            catch
            {
                try
                {
                    myConnection.Open();
                }
                catch
                {
                    throw new System.ArgumentException("Could not connect to Database", "Login String");
                }
            }
        }

        public void SQLDisconnect()
        {
            myConnection.Close();
        }

        SqlCommand cmd;
        SqlDataReader reader;
        public void Execute(string query)
        {
            cmd = new SqlCommand();
            cmd.CommandText = query;
            cmd.Connection = myConnection;
            reader = cmd.ExecuteReader();
            reader.Close();
        }

        public bool HasRows(string query)
        {
            bool RowCount = false;
            SqlCommand queryCommand = new SqlCommand(query, myConnection);
            SqlDataReader queryCommandReader = queryCommand.ExecuteReader();
            while (queryCommandReader.Read())
            {
                RowCount = queryCommandReader.HasRows;
            }
            queryCommandReader.Close();
            return RowCount;
        }

    }
}
