using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ITMS.Models
{
    public class UserModel
    {
        string defaultConnection = ConfigurationManager.ConnectionStrings["DefaultConnectionString"].ToString();

        public int IDUser
        {
            get;set;
        }
        public string User_Name
        {
            get; set;
        }
        public string add_1
        {
            get; set;
        }
        public string add_2
        {
            get; set;
        }
        public string add_poscode
        {
            get; set;
        }
        public string add_city
        {
            get; set;
        }
        public string add_state
        {
            get; set;
        }
        public string UserEmail
        {
            get; set;
        }
        public string UserPassword
        {
            get; set;
        }
        public int User_Cat
        {
            get; set;
        }
        public string phone_no
        {
            get;set;
        }

        [Required(ErrorMessage ="Please enter your registered email")]
        public string UserName
        {
            get;set;
        }

        [Required(ErrorMessage ="Please enter your password")]
        public string Password
        {
            get;set;
        }

        public int HasPicture
        {
            get;set;
        }

        public int fromProfile
        {
            get;set;
        }

        public DataRow getUser(string id)
        {
            DataSet ds = new DataSet();
            using (SqlConnection con = new SqlConnection(defaultConnection))
            {
                using(SqlCommand cmd = new SqlCommand())
                {
                    con.Open();
                    cmd.Connection = con;
                    cmd.CommandText = "select * from tbl_admin where IDUser=@id";
                    cmd.CommandType = CommandType.Text;                  
                    cmd.Parameters.AddWithValue("@id", id);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                    con.Close();
                }

            }
            DataRow dr = null;
            if (ds.Tables.Count > 0)
                if (ds.Tables[0].Rows.Count > 0)
                    dr = ds.Tables[0].Rows[0];

            return dr;
        }
    }
}