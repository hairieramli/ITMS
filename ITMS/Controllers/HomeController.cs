using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using ITMS.Models;

namespace ITMS.Controllers
{
    public class HomeController : Controller
    {
        QueryCode app = new QueryCode();

        public ActionResult Index()
        {
            if(HttpContext.Session["IDUser"] != null)
                loadData();
            return View();
        }

        void loadData()
        {
            string sql = "select * from (select (select COUNT(*) from tbl_report a inner join tbl_ticket b on a.IDrep=b.IDrep where (a.IDUser=@id or b.IDtechnician=@id))total," +
"(select COUNT(*) from tbl_report a inner join tbl_ticket b on a.IDrep = b.IDrep where (a.IDUser = @id or b.IDtechnician =@id) and b.ticketStatus = 'Pending')pending," +
"(select COUNT(*) from tbl_report a inner join tbl_ticket b on a.IDrep = b.IDrep where (a.IDUser = @id or b.IDtechnician = @id) and b.ticketStatus = 'Work Done')workdone," +
"(select COUNT(*) from tbl_report a inner join tbl_ticket b on a.IDrep = b.IDrep where (a.IDUser = @id or b.IDtechnician = @id) and b.ticketStatus = 'In Process')inprocess)tbl";

            List<SqlParameter> para = new List<SqlParameter>()
            {
                new SqlParameter(){ParameterName="@id", SqlDbType= SqlDbType.Int, Value=HttpContext.Session["IDUser"].ToString()}
            };
            DataSet ds = app.GetDataSet(sql, para);
            if (ds.Tables.Count > 0)
                if (ds.Tables[0].Rows.Count > 0)
                {
                    ViewData["total"] = ds.Tables[0].Rows[0]["total"].ToString();
                    ViewData["inprocess"] = ds.Tables[0].Rows[0]["inprocess"].ToString();
                    ViewData["pending"] = ds.Tables[0].Rows[0]["pending"].ToString();
                    ViewData["workdone"] = ds.Tables[0].Rows[0]["workdone"].ToString();
                }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

    }
}