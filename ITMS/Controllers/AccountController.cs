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
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Account/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Account/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
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

        // GET: Account/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Account/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
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

        public virtual ActionResult logOut()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            HttpContext.User = null;

            return RedirectToAction("Index", "Home");

        }
    }
}
