using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Mvc;

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

        public DataTable loadList(string item)
        {
            DataTable dt = null;
            if(item == "priority")
            {
                DataSet ds = new DataSet();
                string sql = "select prio_code item_code, prio_desc item_desc from cfg_work_prio where status='Active'";
                ds = GetDataSet(sql, null);
                if(ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                }
            }
            else if(item == "technician")
            {
                DataSet ds = new DataSet();
                string sql = "select '0' item_code, 'Please Choose' item_desc union all select IDUser item_code, UserName item_desc from tbl_admin where User_Cat=2";
                ds = GetDataSet(sql, null);
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                }
            }
            else if(item == "work_status")
            {
                DataSet ds = new DataSet();
                string sql = "select work_status_desc item_code, work_status_desc item_desc from cfg_work_status where status='Active'";
                ds = GetDataSet(sql, null);
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                }
            }
            else if (item == "user_cat")
            {
                DataSet ds = new DataSet();
                string sql = "select id item_code, Category_User item_desc from cfg_user_cat";
                ds = GetDataSet(sql, null);
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                }
            }
            return dt;
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