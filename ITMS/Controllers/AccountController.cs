using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ITMS.Models;
using System.Data.Entity.Core.Objects;
using System.Data;
using System.Data.SqlClient;
using System.Web.Security;
using System.Security;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Data.Entity;
using System.IO;

namespace ITMS.Controllers
{
    public class AccountController : Controller
    {
        QueryCode app = new QueryCode();
        // GET: Account
        [AllowAnonymous]
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated || Session["IDUser"] != null)
            {
                //System.Diagnostics.Debug.WriteLine("AUTHENTICATED.....");
                FormsIdentity id = (FormsIdentity)User.Identity;
                FormsAuthenticationTicket ticket = id.Ticket;
                System.Diagnostics.Debug.WriteLine("TICKET EMAIL: " + ticket.Name);
                System.Diagnostics.Debug.WriteLine("URL: " + FormsAuthentication.GetRedirectUrl(ticket.Name, false).ToString());
                return Redirect("/Home/Index");
            }
            return View();
        }


        public ActionResult EM()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            DataTable dt = app.loadList("user_cat");
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem()
                {
                    Text = row["item_desc"].ToString(),
                    Value = row["item_code"].ToString()
                });
            }

            ViewData["UserCatList"] = new SelectList(list, "Value", "Text", "1");

            return View("View");
        }

        public ActionResult ProfilePage()
        {
            string id = HttpContext.Session["IDUser"].ToString();
            UserModel model = new UserModel();
            loadData(id, model);
            ViewData["IDUser"] = model.IDUser;
            ViewData["UserName"] = model.User_Name;
            ViewData["UserEmail"] = model.UserEmail;
            ViewData["PhoneNo"] = model.phone_no;
            ViewData["addr"] = model.add_1 + ",<br>" + model.add_2 + ",<br>" + model.add_poscode + ", " + model.add_city + ",<br>" + model.add_state;
            ViewData["Position"] = model.User_Cat == 1 ? "ADMINISTRATOR" : model.User_Cat == 2 ? "TECHNICIAN" : "STAFF";
            ViewData["HasPic"] = model.HasPicture;

            string sql = "select * from (select (select COUNT(*) from tbl_report a inner join tbl_ticket b on a.IDrep=b.IDrep where (a.IDUser=@id or b.IDtechnician=@id))total," +
"(select COUNT(*) from tbl_report a inner join tbl_ticket b on a.IDrep = b.IDrep where (a.IDUser = @id or b.IDtechnician =@id) and b.ticketStatus = 'Pending')pending," +
"(select COUNT(*) from tbl_report a inner join tbl_ticket b on a.IDrep = b.IDrep where (a.IDUser = @id or b.IDtechnician = @id) and b.ticketStatus = 'Work Done')workdone," +
"(select COUNT(*) from tbl_report a inner join tbl_ticket b on a.IDrep = b.IDrep where (a.IDUser = @id or b.IDtechnician = @id) and b.ticketStatus = 'In Process')inprocess)tbl";

            List<SqlParameter> para = new List<SqlParameter>()
            {
                new SqlParameter(){ParameterName="@id", SqlDbType= SqlDbType.Int, Value=model.IDUser}
            };
            DataSet ds = app.GetDataSet(sql, para);
            if(ds.Tables.Count > 0)
                if(ds.Tables[0].Rows.Count > 0)
                {
                    ViewData["total"] = ds.Tables[0].Rows[0]["total"].ToString();
                    ViewData["inprocess"] = ds.Tables[0].Rows[0]["inprocess"].ToString();
                    ViewData["pending"] = ds.Tables[0].Rows[0]["pending"].ToString();
                    ViewData["workdone"] = ds.Tables[0].Rows[0]["workdone"].ToString();
                }

            return View("Profile", model);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Index(UserModel model)
        {
            if (model.UserName != null && model.Password != null)
            {

                string sql = "select * from tbl_admin where UserEmail=@email AND UserPassword=@pwd";

                List<SqlParameter> para = new List<SqlParameter>()
                {
                    new SqlParameter(){ParameterName="@email", SqlDbType=SqlDbType.VarChar, Value=model.UserName },
                    new SqlParameter(){ParameterName="@pwd", SqlDbType=SqlDbType.VarChar, Value=model.Password }
                };

                DataSet ds = app.GetDataSet(sql, para);

                if(ds.Tables.Count > 0)
                {
                    if(ds.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = ds.Tables[0].Rows[0];

                        HttpContext.Session["IDUser"] = dr["IDUser"].ToString();
                        HttpContext.Session["UserName"] = dr["UserName"].ToString();
                        HttpContext.Session["UserEmail"] = dr["UserEmail"].ToString();
                        HttpContext.Session["User_Cat"] = dr["User_Cat"].ToString();
                        HttpContext.Session["lastLoginDate"] = dr["lastLoginDate"].ToString();
                        if (dr["profile_pic"] != DBNull.Value)
                            HttpContext.Session["Haspic"] = 1;
                        else
                            HttpContext.Session["Haspic"] = 0;
                        HttpContext.Session.Timeout = 20;
                        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                            1,
                            dr["UserEmail"].ToString(),
                            DateTime.Now,
                            DateTime.Now.AddMinutes(20),
                            false,
                            dr["IDUser"].ToString(),
                            FormsAuthentication.FormsCookiePath);

                        string encTic = FormsAuthentication.Encrypt(ticket);

                        Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTic));

                        //Response.Cookies[FormsAuthentication.FormsCookieName].Expires = DateTime.Now.AddMinutes(20);
                        //System.Diagnostics.Debug.WriteLine("URL: " + FormsAuthentication.GetRedirectUrl(dr["UserEmail"].ToString(), false).ToString());
                        //return Redirect("/Home/Index");

                        sql = "UPDATE tbl_admin SET lastLoginDate='"+ DateTime.Now +"' WHERE IDUser=" + dr["IDUser"].ToString();
                        string res = app.Exec(sql, null);

                        return Redirect(FormsAuthentication.GetRedirectUrl(dr["UserEmail"].ToString(), false));
                    }
                    else
                    {
                        ViewBag.NotValidUser = "Wrong Combination of Email and Password!";
                    }
                }
                else
                {
                    ViewBag.NotValidUser = "Wrong Combination of Email and Password!";
                }

                //ITMSEntities cbe = new ITMSEntities();
                //ObjectResult<int?> s = cbe.GetLoginResult(model.UserName, model.Password);
                //string item = s.FirstOrDefault().ToString();

            }
            return View();
        }

        // GET: Account/Details/5
        public ActionResult Details(string id)
        {
            id = id.Replace("r_", "");
            UserModel model = new UserModel();
            loadData(id, model);
            return View("View", model);
        }

        void loadData(string id, UserModel model)
        {
            //UserModel model = new UserModel();
            try
            {
                string sql = "select a.*, (case when a.User_Cat is not null then a.User_Cat else 1 end)user_cat from tbl_admin a " +
                    "left join cfg_user_cat b ON a.User_Cat=b.id where a.IDUser=" + id;
                DataTable dt = app.GetDataSet(sql, null).Tables[0];

                if (dt.Rows.Count > 0)
                {

                    model.IDUser = Int32.Parse(dt.Rows[0]["IDUser"].ToString());
                    model.User_Name = dt.Rows[0]["UserName"].ToString();
                    model.add_1 = dt.Rows[0]["add_1"].ToString();
                    model.add_2 = dt.Rows[0]["add_2"].ToString();
                    model.add_poscode = dt.Rows[0]["add_poscode"].ToString();
                    model.add_city = dt.Rows[0]["add_city"].ToString();
                    model.add_state = dt.Rows[0]["add_state"].ToString();
                    model.UserEmail = dt.Rows[0]["UserEmail"].ToString();
                    model.UserPassword = dt.Rows[0]["UserPassword"].ToString();
                    model.User_Cat = Int32.Parse(dt.Rows[0]["user_cat"].ToString());
                    model.phone_no = dt.Rows[0]["phone_no"].ToString();
                    if (dt.Rows[0]["profile_pic"] != DBNull.Value)
                        model.HasPicture = 1;
                    else
                        model.HasPicture = 0;

                    List<SelectListItem> list = new List<SelectListItem>();
                    DataTable sl = app.loadList("user_cat");
                    foreach (DataRow row in sl.Rows)
                    {
                        list.Add(new SelectListItem()
                        {
                            Text = row["item_desc"].ToString(),
                            Value = row["item_code"].ToString()
                        });
                    }

                    ViewData["UserCatList"] = new SelectList(list, "Value", "Text", model.User_Cat.ToString());
                    TempData["edit_value"] = 1;

                    TempData["edit_id"] = model.IDUser;
                }
            }
            catch (Exception ex)
            {
                TempData["edit_value"] = 0;
                TempData["error"] = ex.Message;
            }
        }

        // GET: Account/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Account/Create
        [HttpPost]
        public ActionResult Create(FormCollection form)
        {
            try
            {
                if (form["UserName"].ToString() != "" && form["email"].ToString() != "" && form["password"].ToString() != "" && form["position"].ToString() != "")
                {
                    string cmd = "insert into tbl_admin(UserName, add_1, add_2, add_poscode, add_city, add_state, UserEmail, UserPassword, User_Cat, phone_no)" +
                        "VALUES(@username, @add1, @add2, @poscode, @city, @state, @email, @password, @usercat, @phoneno)";

                    List<SqlParameter> para = new List<SqlParameter>()
                    {
                        new SqlParameter(){ParameterName="@username", SqlDbType=SqlDbType.VarChar, Value=form["UserName"].ToString()},
                        new SqlParameter(){ParameterName="@add1", SqlDbType=SqlDbType.VarChar, Value=form["add_1"].ToString()},
                        new SqlParameter(){ParameterName="@add2", SqlDbType=SqlDbType.VarChar, Value=form["add_2"].ToString()},
                        new SqlParameter(){ParameterName="@poscode", SqlDbType=SqlDbType.VarChar, Value=form["add_poscode"].ToString()},
                        new SqlParameter(){ParameterName="@city", SqlDbType=SqlDbType.VarChar, Value=form["add_city"].ToString()},
                        new SqlParameter(){ParameterName="@state", SqlDbType=SqlDbType.VarChar, Value=form["add_state"].ToString()},
                        new SqlParameter(){ParameterName="@email", SqlDbType=SqlDbType.VarChar, Value=form["email"].ToString()},
                        new SqlParameter(){ParameterName="@password", SqlDbType=SqlDbType.VarChar, Value=form["password"].ToString()},
                        new SqlParameter(){ParameterName="@usercat", SqlDbType=SqlDbType.VarChar, Value=form["position"].ToString()},
                        new SqlParameter(){ParameterName="@phoneno", SqlDbType=SqlDbType.VarChar, Value=form["phone_no"].ToString()}
                    };

                    string result = app.Exec(cmd, para);

                    if(result == "")
                    {
                        TempData["return_value"] = 1;
                    }
                    else
                        TempData["return_value"] = 0;
                }
                List<SelectListItem> list = new List<SelectListItem>();
                DataTable dt = app.loadList("user_cat");
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new SelectListItem()
                    {
                        Text = row["item_desc"].ToString(),
                        Value = row["item_code"].ToString()
                    });
                }

                ViewData["UserCatList"] = new SelectList(list, "Value", "Text", "1");

                return View("View");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return View("View");
            }
        }

        // GET: Account/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Account/Edit/5
        [HttpPost]
        public ActionResult Edit(UserModel model)
        {
            try
            {
                // TODO: Add update logic here
                string sql = "update tbl_admin set UserName=@username, add_1=@add_1, add_2=@add_2, add_poscode=@add_poscode, add_city=@add_city, add_state=@add_state, " +
                    "UserPassword=@password,phone_no=@phone_no where IDUser=@id";

                List<SqlParameter> para = new List<SqlParameter>()
                {
                    new SqlParameter(){ParameterName="@username", SqlDbType=SqlDbType.VarChar, Value=model.User_Name},
                    new SqlParameter(){ParameterName="@add_1", SqlDbType=SqlDbType.VarChar, Value=model.add_1},
                    new SqlParameter(){ParameterName="@add_2", SqlDbType=SqlDbType.VarChar, Value=model.add_2},
                    new SqlParameter(){ParameterName="@add_poscode", SqlDbType=SqlDbType.VarChar, Value=model.add_poscode},
                    new SqlParameter(){ParameterName="@add_city", SqlDbType=SqlDbType.VarChar, Value=model.add_city},
                    new SqlParameter(){ParameterName="@add_state", SqlDbType=SqlDbType.VarChar, Value=model.add_state},
                    new SqlParameter(){ParameterName="@password", SqlDbType=SqlDbType.VarChar, Value=model.UserPassword},
                    new SqlParameter(){ParameterName="@user_cat", SqlDbType=SqlDbType.Int, Value=model.User_Cat},
                    new SqlParameter(){ParameterName="@phone_no", SqlDbType=SqlDbType.VarChar, Value=model.phone_no},
                    new SqlParameter(){ParameterName="@id", SqlDbType=SqlDbType.Int, Value=model.IDUser}
                };

                string result = app.Exec(sql, para);

                if (result == "")
                    TempData["edit_return_value"] = 1;
                else
                    TempData["edit_return_value"] = 0;

                List<SelectListItem> list = new List<SelectListItem>();
                DataTable sl = app.loadList("user_cat");
                foreach (DataRow row in sl.Rows)
                {
                    list.Add(new SelectListItem()
                    {
                        Text = row["item_desc"].ToString(),
                        Value = row["item_code"].ToString()
                    });
                }

                ViewData["UserCatList"] = new SelectList(list, "Value", "Text", model.User_Cat.ToString());

                return View("View", model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("TEST ERROR:" + ex.Message);
                TempData["error"] = ex.Message;
                return View("View", model);
            }
        }

        // GET: Account/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Account/Delete/5
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

        [HttpPost]
        public ActionResult Picture(HttpPostedFileBase fileUpload, FormCollection form)
        {
            TempData.Clear();
            try
            {
                if(fileUpload != null)
                {
                    string resultmsg = "Invalid Image";
                    string filetype = Path.GetExtension(fileUpload.FileName);
                    string filename = Path.GetFileName(fileUpload.FileName);
                    string IDUser = form["hidIDUser"].ToString();

                    string contenttype = String.Empty;

                    switch (filetype)
                    {
                        case ".jpeg":
                            contenttype = "image/jpg";

                            break;
                        case ".jpg":
                            contenttype = "image/jpg";

                            break;
                        case ".png":
                            contenttype = "image/png";

                            break;
                        case ".gif":
                            contenttype = "image/gif";

                            break;
                    }

                    if (contenttype != String.Empty)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            fileUpload.InputStream.CopyTo(ms);
                            byte[] array = ms.GetBuffer();

                            string strQuery = "UPDATE tbl_admin SET profile_pic=@pic, profile_pic_type=@type WHERE IDUser=@id";

                            List<SqlParameter> para = new List<SqlParameter>()
                            {
                                new SqlParameter() {ParameterName="@pic", SqlDbType=SqlDbType.VarBinary, Value=array },
                                new SqlParameter() {ParameterName="@type", SqlDbType=SqlDbType.VarChar, Value=contenttype },
                                new SqlParameter() {ParameterName="@id", SqlDbType=SqlDbType.Int, Value=IDUser }
                            };
                            string resultType = "0";
                            string result = app.Exec(strQuery, para);
                            if (result == "")
                            {
                                TempData["return_upload"] = 1;
                            }
                            else
                            {
                                TempData["return_upload"] = 0;
                            }
                        }
                    }
                    UserModel model = new UserModel();
                    loadData(IDUser, model);
                }
                else
                {
                    
                }
            }
            catch(Exception ex)
            {
                TempData["return_upload_error"] = ex.Message;
                System.Diagnostics.Debug.WriteLine("ERROR EX PIC: " + ex.Message);
            }

            return View("View");
        }

        public void getImage(string id)
        {
            DataSet ds = app.GetDataSet("select profile_pic, profile_pic_type from tbl_admin where IDUser='" + id + "'", null);
            byte[] btImage = null;

            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    if(ds.Tables[0].Rows[0]["profile_pic"] != DBNull.Value)
                    {
                        btImage = (byte[])ds.Tables[0].Rows[0]["profile_pic"];

                        HttpContext.Response.Buffer = true;

                        HttpContext.Response.Charset = "";

                        HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);

                        HttpContext.Response.ContentType = ds.Tables[0].Rows[0]["profile_pic_type"].ToString();

                        HttpContext.Response.AddHeader("content-disposition", "attachment;filename=Profile");

                        HttpContext.Response.BinaryWrite(btImage);

                        HttpContext.Response.Flush();

                        HttpContext.Response.End();
                    }

                }
            }
        }

        public virtual ActionResult logOut()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            HttpContext.User = null;

            return RedirectToAction("Index", "Home");

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
                    sb.Append(" and (UserName LIKE '%");
                    sb.Append(search);
                    sb.Append("%' OR UserEmail LIKE '%");
                    sb.Append(search);
                    sb.Append("%' OR b.Category_User LIKE '%");
                    sb.Append(search);
                    sb.Append("%' OR phone_no LIKE '%");
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
                    orderByClause = orderByClause.Replace("0", ", UserName");
                    orderByClause = orderByClause.Replace("1", ", UserEmail");
                    orderByClause = orderByClause.Replace("2", ", Category_User");
                    orderByClause = orderByClause.Replace("3", ", phone_no");
                    //Eliminate the first comma of the variable "order"status_emp
                    orderByClause = orderByClause.Remove(0, 1);
                }
                else
                    orderByClause = "IDUser asc";
                orderByClause = "ORDER BY " + orderByClause;

                string filterUser = "";
                if (HttpContext.Session["User_Cat"] != null && HttpContext.Session["IDUser"] != null)
                {
                    if (HttpContext.Session["User_Cat"].ToString() == "1")
                        filterUser = "";
                    else
                        filterUser = " AND IDUser" + HttpContext.Session["IDUser"].ToString();
                }



                DataTable dt = new DataTable();

                string sql = string.Format("  select * from " +
                                        "(select ROW_NUMBER() OVER({0}) as rownumber, *from " +
                                        "(select(select COUNT(*) from tbl_admin a left join cfg_user_cat b on a.User_Cat = b.id) as TotalRows, " +
                                        "(select COUNT(*) from tbl_admin a left join cfg_user_cat b on a.User_Cat = b.id WHERE 1 = 1{1}{4}) as TotalDisplayRows, " +
                                        "a.IDUser, a.UserName, a.UserEmail, a.phone_no, b.Category_User from tbl_admin a left " +
                                        "join cfg_user_cat b on a.User_Cat = b.id WHERE 1 = 1{1}{4})det)tbl WHERE rownumber between {2} AND {3}",
                                            orderByClause, whereClause, start + 1, length + 1, filterUser);

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
                        sb.Append("\"DT_ID\":\"r_" + dr["IDUser"].ToString() + "\",");
                        sb.Append("\"0\":\"<a href='/Account/Details/r_" + dr["IDUser"].ToString() + "'>" + dr["UserName"].ToString() + "</a>\",");
                        sb.Append("\"1\":\"" + dr["UserEmail"].ToString() + "\",");
                        sb.Append("\"2\":\"" +  dr["Category_User"].ToString() + "\",");
                        sb.Append("\"3\":\"" + dr["phone_no"].ToString() + "\"");
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
    }
}
