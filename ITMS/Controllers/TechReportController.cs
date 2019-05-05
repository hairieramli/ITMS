using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ITMS.Models;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Data.Entity;
using System.Configuration;

namespace ITMS.Controllers
{
    public class TechReportController : Controller
    {

        QueryCode app = new QueryCode();
        string DefaultConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionString"].ConnectionString.ToString();
        // GET: TechReport
        public ActionResult Index()
        {
            loadDD();
            return View();
        }

        void loadDD()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            DataTable dt = app.loadList("report");
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem()
                {
                    Text = row["item_desc"].ToString(),
                    Value = row["item_code"].ToString()
                });
            }

            ViewData["reportList"] = new SelectList(list, "Value", "Text", "0");
        }
        public ContentResult List()
        {
            int draw = Int32.Parse(Request.QueryString["draw"] != null ? Request.QueryString["draw"].ToString() : "0");
            int length = Int32.Parse(Request.QueryString["length"] != null ? Request.QueryString["length"].ToString() : "0");
            int start = Int32.Parse(Request.QueryString["start"] != null ? Request.QueryString["start"].ToString() : "0");
            string search = Request.QueryString["search[value]"] != null ? Request.QueryString["search[value]"].ToString() : "";

            StringBuilder sb = new StringBuilder();
            sb.Clear();
            string whereClause = string.Empty;

            try
            {
                if (!String.IsNullOrEmpty(search))
                {
                    sb.Append(" and (rep_title LIKE '%");
                    sb.Append(search);
                    sb.Append("%' OR submittedDate LIKE '%");
                    sb.Append(search);
                    sb.Append("%')");
                    whereClause = sb.ToString();
                }
                sb.Clear();
                string orderByClause = string.Empty;
                //Check which column is to be sorted by in which direction
                // for (int i = 0; i < 11; i++)
                //{
                if (Request.Params["order[0][column]"] != null)
                {
                    sb.Append(Request.Params["order[0][column]"]);
                    sb.Append(" ");
                    sb.Append(Request.Params["order[0][dir]"]);
                }
                // }
                orderByClause = sb.ToString();
                //Replace the number corresponding the column position by the corresponding name of the column in the database


                if (!String.IsNullOrEmpty(orderByClause))
                {
                    //orderByClause = orderByClause.Replace("0", ", IDinv");
                    orderByClause = orderByClause.Replace("0", ", rep_title");
                    orderByClause = orderByClause.Replace("1", ", submittedDate");
                    //Eliminate the first comma of the variable "order"status_emp
                    orderByClause = orderByClause.Remove(0, 1);
                }
                else
                    orderByClause = "id asc";
                orderByClause = "ORDER BY " + orderByClause;

                string filterUser = "";
                if (HttpContext.Session["User_Cat"] != null && HttpContext.Session["IDUser"] != null)
                {
                    //if (HttpContext.Session["User_Cat"].ToString() == "3")
                    //    filterUser = " AND IDUser=" + HttpContext.Session["IDUser"].ToString();
                    if (HttpContext.Session["User_Cat"].ToString() == "2")
                        filterUser = " AND IDTech=" + HttpContext.Session["IDUser"].ToString();
                }



                DataTable dt = new DataTable();

                string sql = string.Format("select * from" +
"(select ROW_NUMBER() OVER({0}) as rownumber, * from" +
"(select(select COUNT(*) from tbl_tech_report a left join tbl_report b on a.IDrep = b.IDrep) as TotalRows, " +
"(select COUNT(*) from tbl_tech_report c left join tbl_report d on c.IDrep = d.IDrep WHERE 1 = 1{1}{4}) as TotalDisplayRows, " +
"id, rep_title, e.submittedDate, IDTech from tbl_tech_report e left join tbl_report f on e.IDrep = f.IDrep WHERE 1 = 1{1}{4})det)tbl WHERE rownumber between {2} AND {3}", orderByClause, whereClause, start + 1, length + 1, filterUser);

                dt = app.GetDataSet(sql, null).Tables[0];
                int totalRows = 0;
                int totalDisplay = 0;
                if (dt.Rows.Count > 0)
                {
                    totalRows = Int32.Parse(dt.Rows[0]["TotalRows"].ToString());
                    totalDisplay = Int32.Parse(dt.Rows[0]["TotalDisplayRows"].ToString());
                }

                sb.Clear();
                JArray json = JArray.Parse(DtToJson(dt));
                sb.Append("{\"draw\":\"" + draw.ToString() + "\",");
                sb.Append("\"recordsTotal\":\"" + totalRows.ToString() + "\",");
                sb.Append("\"recordsFiltered\":\"" + totalDisplay.ToString() + "\",");
                sb.Append("\"data\":" + json.ToString() + "}");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
            }

            return Content(sb.ToString(), "application/json");

        }

        private string DtToJson(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            string outputJson = "";
            try
            {
                if (dt.Rows.Count > 0)
                {
                    sb.Clear();
                    foreach (DataRow dr in dt.Rows)
                    {

                        sb.Append("{");
                        sb.Append("\"DT_ID\":\"r_" + dr["id"].ToString() + "\",");
                        sb.Append("\"0\":\"<a href='/TechReport/Details/" + dr["id"].ToString() + "'>" + dr["rep_title"].ToString() + "</a>\",");
                        sb.Append("\"1\":\"" + (dr["submittedDate"] != null ? String.Format("{0:dd MMM yyy}", dr["submittedDate"]) : "") + "\"");
                        sb.Append("},");
                    }
                    outputJson = sb.ToString();
                    outputJson = outputJson.Remove(outputJson.Length - 1, 1);

                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                System.Diagnostics.Debug.WriteLine("Error DB: " + ex.Message);
            }

            outputJson = "[" + outputJson + "]";
            return outputJson;
        }

        public ActionResult Details(string id)
        {
            //TempData.Clear();
            TechReportModel model = new TechReportModel();
            System.Diagnostics.Debug.WriteLine("IDTECH: " + id);
            loadData(id.Replace("r_", ""), model);
            TempData["edit_value"] = 1;

            return View("Index", model);
        }


        void loadData(string id, TechReportModel model)
        {

            id = id.Replace("r_", "");

            loadDD();
            try
            {
                string sql = "select * from tbl_tech_report where id=" + id;
                DataTable dt = app.GetDataSet(sql, null).Tables[0];

                if (dt.Rows.Count > 0)
                {

                    model.id = Int32.Parse(dt.Rows[0]["id"].ToString());
                    

                    model.IDrep = Int32.Parse(dt.Rows[0]["IDrep"].ToString());
                    model.rep_desc = dt.Rows[0]["rep_desc"].ToString();
                    model.submittedDate = DateTime.Parse(dt.Rows[0]["submittedDate"].ToString());

                    List<SelectListItem> list = new List<SelectListItem>();
                    DataTable sl = new DataTable();

                    list = new List<SelectListItem>();
                    sl.Clear();
                    sl = app.loadList("report");
                    foreach (DataRow row in sl.Rows)
                    {
                        list.Add(new SelectListItem()
                        {
                            Text = row["item_desc"].ToString(),
                            Value = row["item_code"].ToString()
                        });
                    }
                    ViewData["reportList"] = new SelectList(list, "Value", "Text", model.IDrep.ToString());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: " + ex.Message);
                TempData["edit_error"] = ex.Message;
            }
        }

        // GET: TechReport/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TechReport/Create
        [HttpPost]
        public ActionResult Create(FormCollection form)
        {
            TempData.Clear();
            loadDD();
            try
            {
                int return_value = 0;
                string iduser = "";
                string username = "";
                if(HttpContext.Session["IDUser"] != null && HttpContext.Session["UserName"] != null)
                {
                    iduser = HttpContext.Session["IDUser"].ToString();
                    username = HttpContext.Session["UserName"].ToString();
                    string sql = "insert into tbl_tech_report(IDrep, rep_desc, IDTech, UserName) VALUES(@IDrep, @rep_desc, @IDTech, @UserName)";
                    List<SqlParameter> para = new List<SqlParameter>()
                    {
                        new SqlParameter(){ParameterName="@IDrep", SqlDbType=SqlDbType.Int, Value=form["ddl_rep"].ToString()},
                        new SqlParameter(){ParameterName="@rep_desc", SqlDbType=SqlDbType.VarChar, Value=form["rep_desc"].ToString().Trim()},
                        new SqlParameter(){ParameterName="@IDTech", SqlDbType=SqlDbType.Int, Value=iduser},
                        new SqlParameter(){ParameterName="@UserName", SqlDbType=SqlDbType.VarChar, Value=form["rep_desc"].ToString().Trim()}
                    };
                    string result = app.Exec(sql, para);
                    if (result == "")
                        return_value = 1;
                    TempData["return_value"] = return_value;

                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["return_error"] = ex.Message;
                return View("Index");
            }

        }

        // GET: TechReport/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TechReport/Edit/5
        [HttpPost]
        public ActionResult Edit(TechReportModel model)
        {
            loadDD();
            try
            {
                System.Diagnostics.Debug.WriteLine("IDTECH2: " + model.id);
                int val = 0;
                string sql = "update tbl_tech_report set rep_desc=@rep_desc where id=@id";
                List<SqlParameter> para = new List<SqlParameter>()
                {
                    new SqlParameter(){ParameterName="@rep_desc", SqlDbType=SqlDbType.VarChar, Value=model.rep_desc},
                    new SqlParameter(){ParameterName="@id", SqlDbType=SqlDbType.Int, Value=model.id}
                };

                string result = app.Exec(sql, para);

                if (result == "")
                    val = 1;
                TempData["edit_return_value"] = val;
                loadData(model.id.ToString(), model);
                return View("Index", model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR TECHREPORT: " + ex.Message);
                loadData(model.id.ToString(), model);
                TempData["edit_return_error"] = ex.Message;
                return View("Index", model);
            }
        }

        // GET: TechReport/Delete/5
        public ActionResult Delete(int id)
        {
            TempData.Clear();
            loadDD();
            try
            {
                string sql = "delete from tbl_tech_report where id=@id";
                List<SqlParameter> para = new List<SqlParameter>()
                {
                    new SqlParameter(){ParameterName="@id", SqlDbType=SqlDbType.Int, Value=id}
                };
                string res = app.Exec(sql, para);
                if (res == "")
                {
                    TempData["del_return_value"] = 1;
                }
                else
                    TempData["del_return_value"] = 0;

                return RedirectToAction("Index", "TechReport");
            }
            catch (Exception ex)
            {
                TempData["del_error"] = ex.Message;
            }
            return View("Index");
        }

        // POST: TechReport/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
