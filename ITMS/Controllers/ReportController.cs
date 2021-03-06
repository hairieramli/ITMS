﻿using System;
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
    public class ReportController : Controller
    {

        QueryCode app = new QueryCode();
        string DefaultConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnectionString"].ConnectionString.ToString();

        // GET: Report
        public ActionResult Index()
        {

            loadDD();

            return View();
        }

        void loadDD()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            DataTable dt = app.loadList("priority");
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem()
                {
                    Text = row["item_desc"].ToString(),
                    Value = row["item_code"].ToString()
                });
            }

            ViewData["priorityList"] = new SelectList(list, "Value", "Text", "1");

            list = new List<SelectListItem>();
            dt.Clear();
            dt = app.loadList("technician");
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem()
                {
                    Text = row["item_desc"].ToString(),
                    Value = row["item_code"].ToString()
                });
            }
            ViewData["techList"] = new SelectList(list, "Value", "Text", "0");

            list = new List<SelectListItem>();
            dt.Clear();
            dt = app.loadList("work_status");
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem()
                {
                    Text = row["item_desc"].ToString(),
                    Value = row["item_code"].ToString()
                });
            }
            ViewData["work_status"] = new SelectList(list, "Value", "Text", "0");

            list = new List<SelectListItem>();
            list.Add(new SelectListItem(){Text = "No",Value = "No"});
            list.Add(new SelectListItem() { Text = "Yes", Value = "Yes" });
            ViewData["attendance_status"] = new SelectList(list, "Value", "Text", "0");
        }

        [HttpPost]
        public string Noti()
        {
            int id = 0;
            string result = "";
            try
            {
                
                string sql = "select a.*, (select TOP 1 UserName from tbl_admin where IDUser=a.from_user) as ffrom, (select TOP 1 UserName from tbl_admin where IDUser=a.to_user) as tto from tbl_notification a, tbl_admin b where a.status='Unread' and a.to_user=@curr and a.to_user=b.IDUser";
                List<SqlParameter> para = new List<SqlParameter>()
                {
                    new SqlParameter(){ParameterName="@curr", SqlDbType=SqlDbType.Int, Value=HttpContext.Session["IDUser"].ToString() }
                };
                DataSet ds = app.GetDataSet(sql, para);

                if(ds.Tables.Count > 0)
                    if(ds.Tables[0].Rows.Count > 0)
                    {
                        result =  ds.Tables[0].Rows[0]["ffrom"].ToString() +" " + ds.Tables[0].Rows[0]["noti_desc"].ToString();
                        id = Int32.Parse(ds.Tables[0].Rows[0]["IDnoti"].ToString());
                    }

                if (result != "" && id != 0)
                {
                    System.Diagnostics.Debug.WriteLine("IN: " + result);
                    using (SqlConnection Sqlcon = new SqlConnection(DefaultConnectionString))
                    {
                        string command = "UPDATE tbl_notification SET status='Read' where IDnoti=" + id;

                        using (SqlCommand cmd = new SqlCommand())
                        {
                            Sqlcon.Open();
                            cmd.Connection = Sqlcon;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = command;
                            cmd.ExecuteNonQuery();
                            Sqlcon.Close();
                        }
                    }
                }
                    
            }
            catch(Exception ex)
            {

            }
            return result;
        }

        public ContentResult GetAllNoti()
        {
            string outputJson = "";
            StringBuilder sb = new StringBuilder();
            int id = 0;
            try
            {

                sb.Clear();
                string sql = "select TOP 10 a.*, CASE WHEN a.noti_desc IS NOT NULL THEN (select TOP 1 UserName from tbl_admin where IDUser=a.from_user) + ' ' + a.noti_desc ELSE '' END as msg ,  (select TOP 1 UserName from tbl_admin where IDUser=a.from_user)from_user_name,(select TOP 1 UserName from tbl_admin where IDUser=a.to_user) as tto from tbl_notification a, tbl_admin b where a.to_user=@curr and a.to_user=b.IDUser order by a.createdDate DESC";
                List<SqlParameter> para = new List<SqlParameter>()
                {
                    new SqlParameter(){ParameterName="@curr", SqlDbType=SqlDbType.Int, Value=HttpContext.Session["IDUser"].ToString() }
                };
                DataSet ds = app.GetDataSet(sql, para);

                if (ds.Tables.Count > 0)
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        foreach(DataRow dr in ds.Tables[0].Rows)
                        {
                            sb.Append("{");
                            sb.Append("\"IDnoti\":\"r_" + dr["IDnoti"].ToString() + "\",");
                            sb.Append("\"Desc\":\""+ dr["noti_desc"].ToString() + "\",");
                            sb.Append("\"From_User\":\"" + dr["from_user_name"].ToString() + "\",");
                            sb.Append("\"createdDate\":\"" + (dr["createdDate"] != null ? String.Format("{0:dd MMM yyy hh:mm:tt}", dr["createdDate"]) : "") + "\"");
                            sb.Append("},");
                        }

                    }
                outputJson = sb.ToString();
                outputJson = outputJson.Remove(outputJson.Length - 1, 1);
                sb.Clear();
                sb.Append("{\"data\": ["+ outputJson +"]}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR NOTI: " + ex.Message);
            }
            return Content(sb.ToString(), "application/json");
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
                    sb.Append("%' OR rep_desc LIKE '%");
                    sb.Append(search);
                    sb.Append("%' OR status LIKE '%");
                    sb.Append(search);
                    sb.Append("%' OR submittedDate LIKE '%");
                    sb.Append(search);
                    sb.Append("%' OR UserName LIKE '%");
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
                    orderByClause = orderByClause.Replace("1", ", UserName");
                    orderByClause = orderByClause.Replace("2", ", submittedDate");
                    orderByClause = orderByClause.Replace("3", ", status");
                    //Eliminate the first comma of the variable "order"status_emp
                    orderByClause = orderByClause.Remove(0, 1);
                }
                else
                    orderByClause = "IDrep asc";
                orderByClause = "ORDER BY " + orderByClause;

                string filterUser = "";
                if(HttpContext.Session["User_Cat"] != null && HttpContext.Session["IDUser"] != null)
                {
                    if (HttpContext.Session["User_Cat"].ToString() == "3")
                        filterUser = " AND IDUser=" + HttpContext.Session["IDUser"].ToString();
                    else if(HttpContext.Session["User_Cat"].ToString() == "2")
                        filterUser = " AND IDtechnician=" + HttpContext.Session["IDUser"].ToString();
                }



                DataTable dt = new DataTable();

                string sql = string.Format("select * from" +
"(select ROW_NUMBER() OVER({0}) as rownumber, * from" +
"(select(select COUNT(*) from tbl_report a left join tbl_ticket b on a.IDrep = b.IDrep) as TotalRows, " +
"(select COUNT(*) from tbl_report c left join tbl_ticket d on c.IDrep = d.IDrep WHERE 1 = 1{1}{4}) as TotalDisplayRows, " +
"e.IDrep, rep_title, submittedDate, UserName, (CASE WHEN ticketStatus IS NULL THEN 'pending' ELSE ticketStatus END)status from tbl_report e left join tbl_ticket f on e.IDrep = f.IDrep WHERE 1 = 1{1}{4})det)tbl WHERE rownumber between {2} AND {3}", orderByClause, whereClause, start + 1, length + 1, filterUser);

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
                        string color = "";
                        if (dr["status"].ToString().ToLower() == "work done")
                            color = " color='#3EC81F'";
                        else if(dr["status"].ToString().ToLower() == "in process")
                            color = " color='#E9F015'";                        

                        sb.Append("{");
                        sb.Append("\"DT_ID\":\"r_" + dr["IDrep"].ToString() + "\",");
                        //sb.Append("\"0\":\"" + dr["IDinv"].ToString() + "\",");
                        sb.Append("\"0\":\"<a href='/Report/Details/r_" + dr["IDrep"].ToString() + "'>" + dr["rep_title"].ToString() + "</a>\",");
                        sb.Append("\"1\":\"" + dr["UserName"].ToString() + "\",");
                        sb.Append("\"2\":\"" + (dr["submittedDate"] != null ? String.Format("{0:dd MMM yyy}", dr["submittedDate"]):"") + "\",");
                        sb.Append("\"3\":\"<span"+ color +">" + dr["status"].ToString() + "</span>\"");
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

        // GET: Report/Details/5
        public ActionResult Details(string id)
        {
            //TempData.Clear();
            ReportModel model = new ReportModel();
            loadData(id.Replace("r_", ""), model);
            TempData["edit_value"] = 1;

            return View("Index", model);
        }


        void loadData(string id, ReportModel model)
        {
            
            id = id.Replace("r_", "");
            loadDD();
            try
            {
                string sql = "select a.*, (case when b.TicketDate is not null then b.TicketDate else '' end)TicketDate, (case when b.IDticket is not null then b.IDticket else '0' end)IDticket," +
                    "(case when b.task_type is not null then b.task_type else '' end)task_type," +
                    "(CASE WHEN b.ticketStatus IS NULL THEN 'pending' ELSE b.ticketStatus END)status, " +
                    "(case when b.priority is null then '1' else b.priority end)priority, " +
                    "(case when b.IDtechnician is not null then b.IDtechnician else '0' end)IDtechnician," +
                    "(case when b.IDtechnician is not null then (select UserName from tbl_admin where IDUser=b.IDtechnician) else '' end)technician," +
                    "(case when b.attendanceStatus is not null then b.attendanceStatus else 'No' End)attendanceStatus from tbl_report a " +
                    "left join tbl_ticket b ON a.IDrep=b.IDrep where a.IDrep=" + id;
                DataTable dt = app.GetDataSet(sql, null).Tables[0];

                if (dt.Rows.Count > 0)
                {

                    model.IDrep = Int32.Parse(dt.Rows[0]["IDrep"].ToString());
                    model.IDUser = Int32.Parse(dt.Rows[0]["IDUser"].ToString());
                    model.rep_title = dt.Rows[0]["rep_title"].ToString();
                    model.rep_desc = dt.Rows[0]["rep_desc"].ToString();
                    model.UserName = dt.Rows[0]["UserName"].ToString();
                    model.ticketStatus = dt.Rows[0]["status"].ToString();
                    model.priority = Int32.Parse(dt.Rows[0]["priority"].ToString());
                    model.IDtechnician = Int32.Parse(dt.Rows[0]["IDtechnician"].ToString());
                    model.technician = dt.Rows[0]["technician"].ToString();
                    model.task_type = dt.Rows[0]["task_type"].ToString();
                    model.IDTicket = Int32.Parse(dt.Rows[0]["IDticket"].ToString());
                    model.submittedDate = DateTime.Parse(dt.Rows[0]["submittedDate"].ToString());
                    model.TicketDate = DateTime.Parse(dt.Rows[0]["TicketDate"].ToString());
                    model.attendanceStatus = dt.Rows[0]["attendanceStatus"].ToString();
                    //TempData["submittedDate"] = 
                    List<SelectListItem> list = new List<SelectListItem>();
                    DataTable sl = new DataTable();

                    list = new List<SelectListItem>();
                    sl.Clear();
                    sl = app.loadList("work_status");
                    foreach (DataRow row in sl.Rows)
                    {
                        list.Add(new SelectListItem()
                        {
                            Text = row["item_desc"].ToString(),
                            Value = row["item_code"].ToString()
                        });
                    }
                    ViewData["work_status"] = new SelectList(list, "Value", "Text", model.ticketStatus.ToString());

                    list = new List<SelectListItem>();
                    dt = app.loadList("priority");
                    foreach (DataRow row in dt.Rows)
                    {
                        list.Add(new SelectListItem()
                        {
                            Text = row["item_desc"].ToString(),
                            Value = row["item_code"].ToString()
                        });
                    }

                    ViewData["priorityList"] = new SelectList(list, "Value", "Text", model.priority.ToString());

                    list = new List<SelectListItem>();
                    dt.Clear();
                    dt = app.loadList("technician");
                    foreach (DataRow row in dt.Rows)
                    {
                        list.Add(new SelectListItem()
                        {
                            Text = row["item_desc"].ToString(),
                            Value = row["item_code"].ToString()
                        });
                    }
                    ViewData["techList"] = new SelectList(list, "Value", "Text", model.IDtechnician.ToString());

                    list = new List<SelectListItem>();
                    list.Add(new SelectListItem() { Text = "No", Value = "No" });
                    list.Add(new SelectListItem() { Text = "Yes", Value = "Yes" });
                    ViewData["attendance_status"] = new SelectList(list, "Value", "Text", model.attendanceStatus);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: " + ex.Message);
                TempData["edit_error"] = ex.Message;
            }
        }

        // GET: Report/Create
        [HttpPost]
        public ActionResult Create(FormCollection form)
        {
            TempData.Clear();
            loadDD();
            try
            {
                if(HttpContext.Session["IDUser"] != null && HttpContext.Session["UserName"] != null)
                {
                    System.Diagnostics.Debug.WriteLine("IDUSER : ");

                    ITMSEntities2 et = new ITMSEntities2();
                    tbl_report tbl = new tbl_report();

                    DbSet test = et.Set<tbl_report>();
                    test.Add(new tbl_report { rep_title = form["rep_title"].ToString().Trim(), rep_desc = form["rep_desc"].ToString(), IDUser = Int32.Parse(HttpContext.Session["IDUser"].ToString()), UserName = HttpContext.Session["UserName"].ToString(),submittedDate=System.DateTime.Now});
                    int return_value = et.SaveChanges();
                    if(return_value > 0)
                        app.NotifyUser("1", "Report", "Has Created a New Report");
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

        // POST: Report/Create

        public ActionResult Create()
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Report/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Report/Edit/5
        [HttpPost]
        public ActionResult Edit(ReportModel model)
        {
            TempData.Clear();
            loadDD();
            try
            {
                // TODO: Add update logic here
                ITMSEntities2 et = new ITMSEntities2();
                int val = 0;

                tbl_report t = new tbl_report()
                {
                    IDrep = model.IDrep,
                    rep_title = model.rep_title.ToString().Trim(),
                    rep_desc = model.rep_desc.ToString().Trim(),
                    IDUser = model.IDUser,
                    UserName = model.UserName,
                    submittedDate = model.submittedDate
                };

                et.tbl_report.Attach(t);
                et.Entry(t).State = EntityState.Modified;
                val = et.SaveChanges();
                if (val > 0)
                    app.NotifyUser("1", "Report", "Has Made a Changes On a Report [" + model.rep_title.ToString().Trim() + "]");
                TempData["edit_return_value"] = val;
                loadData(model.IDrep.ToString(), model);
                return View("Index", model);
            }
            catch (Exception ex)
            {
                loadData(model.IDrep.ToString(), model);
                TempData["edit_return_error"] = ex.Message;
                return View("Index", model);
            }
        }

        // GET: Report/Delete/5
        public ActionResult Delete(int id)
        {
            TempData.Clear();
            loadDD();
            try
            {
                string sql = "delete from tbl_report where IDrep=@id";
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

                return RedirectToAction("Index", "Report");
            }
            catch (Exception ex)
            {
                TempData["del_error"] = ex.Message;
            }
            return View("Index");
        }

        public ActionResult NotifyUser(int id)
        {
            loadDD();
            TempData.Clear();
            try
            {
                string sql = "select * from tbl_report where IDrep = @id";
                List<SqlParameter> para = new List<SqlParameter>()
                {
                    new SqlParameter(){ParameterName="@id", SqlDbType=SqlDbType.Int, Value=id}
                };
                DataSet ds = app.GetDataSet(sql, para);
                if (ds.Tables.Count > 0)
                {
                    if(ds.Tables[0].Rows.Count > 0)
                    {
                        app.NotifyUser(ds.Tables[0].Rows[0]["IDUser"].ToString(), "Report", "Has Approve the Work Done by Technician On Your Report ["+ ds.Tables[0].Rows[0]["rep_title"].ToString() + "]");
                        TempData["not_return_value"] = 1;
                    }

                }
                else
                    TempData["not_return_value"] = 0;

                return RedirectToAction("Index", "Report");
            }
            catch (Exception ex)
            {
                TempData["noti_error"] = ex.Message;
            }
            return View("Index");
        }

        // POST: Report/Delete/5
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

        public void getTech()
        {

            Response.Cache.SetCacheability(HttpCacheability.ServerAndNoCache);
            Response.Clear();
            string callback = Request.Params["callback"] != null ? Request.Params["callback"] : "";
            string term = Request.Params["term"] != null ? Request.Params["term"].ToString().Replace("'", "''") : "";


            StringBuilder sb = new StringBuilder();

            string sql = "select top 10 IDUser, UserName from tbl_admin a where  UserName like '%" + term.Trim() + "%' and User_Cat=2";
            DataSet ds = app.GetDataSet(sql, null);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                sb.Append("{");

                sb.Append("\"id\": \"" + dr["IDUser"].ToString() + "\",");
                sb.Append("\"value\": \"" + dr["UserName"].ToString()  + "\"");
                sb.Append("},");
            }
            string j = callback + (sb.ToString() != "" ? "([" + sb.ToString().Remove(sb.ToString().Length - 1) + "])" : "");
            Response.ClearHeaders();
            Response.ClearContent();
            Response.AddHeader("Content-type", "text/json");

            Response.Write(j);
            Response.Flush();
            Response.End();
        }


        public void showTech()
        {

            Response.Cache.SetCacheability(HttpCacheability.ServerAndNoCache);
            Response.Clear();
            string callback = Request.Params["callback"] != null ? Request.Params["callback"] : "";
            string term = Request.Params["term"] != null ? Request.Params["term"].ToString().Replace("'", "''") : "";


            StringBuilder sb = new StringBuilder();

            string sql = "select '0' IDUser, 'Please Choose' UserName union all select IDUser, UserName from tbl_admin a where User_Cat=2";
            DataSet ds = app.GetDataSet(sql, null);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                sb.Append("{");

                sb.Append("\"id\": \"" + dr["IDUser"].ToString() + "\",");
                sb.Append("\"value\": \"" + dr["UserName"].ToString() + "\"");
                sb.Append("},");
            }
            string j = callback + (sb.ToString() != "" ? "([" + sb.ToString().Remove(sb.ToString().Length - 1) + "])" : "");
            Response.ClearHeaders();
            Response.ClearContent();
            Response.AddHeader("Content-type", "text/json");

            Response.Write(j);
            Response.Flush();
            Response.End();
        }
    }
}
