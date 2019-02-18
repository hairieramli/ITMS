using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ITMS.Models;
using System.Web.Script.Serialization;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ITMS.Controllers
{
    public class InventoryController : Controller
    {
        // GET: Inventory
        QueryCode app = new QueryCode();

        public ActionResult Index()
        {
            if (Session["IDUser"] == null)
                return RedirectToAction("Index", "Account");
            return View();
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
                    sb.Append(" and (invName LIKE '%");
                    sb.Append(search);
                    sb.Append("%' OR invQty LIKE '%");
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
                    orderByClause = orderByClause.Replace("0", ", IDinv");
                    orderByClause = orderByClause.Replace("1", ", invName");
                    orderByClause = orderByClause.Replace("2", ", invQty");
                    //Eliminate the first comma of the variable "order"status_emp
                    orderByClause = orderByClause.Remove(0, 1);
                }
                else
                    orderByClause = "IDinv asc";
                orderByClause = "ORDER BY " + orderByClause;

                DataTable dt = new DataTable();

                string sql = string.Format("select * from " +
                    "(select ROW_NUMBER() OVER({0}) as rownumber, * from " +
                    "(select(select COUNT(*) from tbl_inventory) as TotalRows, " +
                    "(select COUNT(*) from tbl_inventory WHERE 1=1{1}) as TotalDisplayRows, " +
                    "* from tbl_inventory WHERE 1=1{1})details)a WHERE rownumber between {2} AND {3}", orderByClause, whereClause, start + 1, length + 1);

                dt = app.GetDataSet(sql, null).Tables[0];
                int totalRows = 0;
                int totalDisplay = 0;
                if (dt.Rows.Count > 0)
                {
                    totalRows = Int32.Parse(dt.Rows[0]["TotalRows"].ToString());
                    totalDisplay = Int32.Parse(dt.Rows[0]["TotalDisplayRows"].ToString());
                }

                sb.Clear();

                //JToken token = JToken.Parse(DtToJson(dt));
                JArray json = JArray.Parse(DtToJson(dt));
                sb.Append("{\"draw\":\"" + draw.ToString() + "\",");
                sb.Append("\"recordsTotal\":\"" + totalRows.ToString() + "\",");
                sb.Append("\"recordsFiltered\":\"" + totalDisplay.ToString() + "\",");
                sb.Append("\"data\":" + json.ToString() + "}");
            }
            catch (Exception ex)
            {

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
                        sb.Append("\"DT_ID\":\"r_" + dr["IDinv"].ToString() + "\",");
                        sb.Append("\"0\":\"" + dr["IDinv"].ToString() + "\",");
                        sb.Append("\"1\":\"<a href='/Inventory/Details/r_" + dr["IDinv"].ToString() + "'>" + dr["invName"].ToString() + "</a>\",");
                        sb.Append("\"2\":\"" + dr["invQty"].ToString() + "\"");
                        sb.Append("},");
                    }
                    outputJson = sb.ToString();
                    outputJson = outputJson.Remove(outputJson.Length - 1, 1);

                }
            }
            catch (Exception ex)
            {

            }

            outputJson = "[" + outputJson + "]";
            return outputJson;
        }

        // GET: Inventory/Details/5
        public ActionResult Details(string id)
        {
            id = id.Replace("r_", "");
            InventoryModel model = new InventoryModel();
            try
            {
                string sql = "select * from tbl_inventory where IDinv=" + id;
                DataTable dt = app.GetDataSet(sql, null).Tables[0];

                if(dt.Rows.Count > 0)
                {
                    
                    model.IDinv = Int32.Parse(dt.Rows[0]["IDinv"].ToString());
                    model.InvName = dt.Rows[0]["invName"].ToString();
                    model.InvQty = Int32.Parse(dt.Rows[0]["invQty"].ToString());
                    TempData["edit_value"] = 1;
                    TempData["edit_id"] = model.IDinv;
                }
            }
            catch(Exception ex)
            {
                TempData["edit_value"] = 0;
            }

            return View("Index", model);
        }

        // GET: Inventory/Create
        public ActionResult Create()
        {



            return View();
        }

        // POST: Inventory/Create
        [HttpPost]
        public ActionResult Create(FormCollection form)
        {
            try
            {

                    ITMSEntities1 et = new ITMSEntities1();
                    tbl_inventory tbl = new tbl_inventory();

                    DbSet test = et.Set<tbl_inventory>();
                    test.Add(new tbl_inventory { invName = form["invName"].ToString().Trim(), invQty = Int32.Parse(form["invQty"].ToString()) });
                    int return_value = et.SaveChanges();
                    System.Diagnostics.Debug.WriteLine("RETURN VALUE: " + return_value);
                    TempData["return_value"] = return_value;

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Inventory/Edit/5
        public ActionResult Edit(int id)
        {
            return View("Index");
        }

        // POST: Inventory/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection form)
        {
            try
            {
                // TODO: Add update logic here
                ITMSEntities1 et = new ITMSEntities1();
                int val = 0;
                tbl_inventory t = new tbl_inventory()
                {
                    IDinv = form.
                    invName = model.InvName.ToString().Trim(),
                    invQty = model.InvQty
                };

                et.tbl_inventory.Attach(t);
                et.Entry(t).State = EntityState.Modified;
                TempData["edit_return_value"] = et.SaveChanges();
                return View("Index", model);
            }
            catch
            {
                return View("Index", model);
            }
        }

        // GET: Inventory/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Inventory/Delete/5
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
