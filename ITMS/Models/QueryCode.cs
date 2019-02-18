using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace ITMS.Models
{
    public class QueryCode
    {
        string DefaultConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionString"].ConnectionString.ToString();
        SqlConnection SQLConn = new SqlConnection();
        public QueryCode()
        {
            SQLConn.ConnectionString = DefaultConnectionString;
        }
        public DataSet GetDataSet(string command, List<SqlParameter> parameter)
        {
            DataSet ds = new DataSet();

            try
            {
                using (SqlConnection Sqlcon = new SqlConnection(DefaultConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        Sqlcon.Open();
                        cmd.Connection = Sqlcon;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = command;
                        if (parameter != null)
                            cmd.Parameters.AddRange(parameter.ToArray());
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(ds);

                    }
                }
            }
            catch (Exception ex)
            {
                
            }
            finally
            {
                SQLConn.Close();

            }

            return ds;
        }
    }
}