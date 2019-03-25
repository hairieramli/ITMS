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

        public void NotifyUser(string userID, string module, string desc)
        {
            using (SqlConnection Sqlcon = new SqlConnection(DefaultConnectionString))
            {
                List<SqlParameter> para = new List<SqlParameter>()
                {
                    new SqlParameter(){ParameterName="@userID", SqlDbType=SqlDbType.Int, Value=userID},
                    new SqlParameter(){ParameterName="@module", SqlDbType=SqlDbType.VarChar, Value=module},
                    new SqlParameter(){ParameterName="@desc", SqlDbType=SqlDbType.VarChar, Value=desc},
                    new SqlParameter(){ParameterName="@from_user", SqlDbType=SqlDbType.Int, Value=HttpContext.Current.Session["IDUser"].ToString()},
                    new SqlParameter(){ParameterName="@status", SqlDbType=SqlDbType.VarChar, Value="Unread"}
                };

                string command = "INSERT INTO tbl_notification(from_user, to_user, noti_desc, noti_module, status)VALUES(@from_user, @userID, @desc, @module, @status)";

                using (SqlCommand cmd = new SqlCommand())
                {
                    Sqlcon.Open();
                    cmd.Connection = Sqlcon;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = command;
                    if (para != null)
                        cmd.Parameters.AddRange(para.ToArray());
                    cmd.ExecuteNonQuery();

                }
            }
        }

        public string Exec(string command, List<SqlParameter> parameter)
        {
            DataSet ds = new DataSet();
            string ResultStr = "ERROR";
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
                        ResultStr = cmd.ExecuteNonQuery() == 1 ? "" : "Error";

                    }
                }
            }
            catch (Exception ex)
            {
                ResultStr = ex.Message;
            }
            finally
            {
                SQLConn.Close();

            }

            return ResultStr;
        }
    }
}