using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Algorithmic_trader
{
    class D_B
    {
        public List<double> p;
        public List<string> d = new List<string>();
        SqlConnection con = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataReader datareader;

        
        public void db_read(string share, string D1, string D2)
        {
            p = new List<double>();
            cmd.Parameters.Clear();
            
          //    SqlConnection con = new SqlConnection("Data Source=.;Initial Catalog=Sherin;Integrated Security=True");
           SqlConnection con = new SqlConnection("Data Source = SQL5063.site4now.net; Initial Catalog = db_a8b890_sherindb; User Id = db_a8b890_sherindb_admin; Password = Crypto_Market2023");
            con.Open();
            cmd.Connection = con;
            
            cmd.CommandText = "select [Price],[Date] from " + share + " where date between @P1 and @P2";  
           

      
            cmd.Parameters.Add("@P1", System.Data.SqlDbType.Date); cmd.Parameters["@P1"].Value = D1;
            cmd.Parameters.Add("@P2", System.Data.SqlDbType.Date); cmd.Parameters["@P2"].Value = D2;

            datareader = cmd.ExecuteReader();
            datareader.Read();
            do
            {
                p.Add(double.Parse(datareader["Price"].ToString()));
                d.Add(datareader["Date"].ToString());
            } while (datareader.Read());
            con.Close();
        }



    }
}
