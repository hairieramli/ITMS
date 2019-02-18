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

        // GET: Account
        [AllowAnonymous]
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated && Session["IDUser"] != null)
            {
                System.Diagnostics.Debug.WriteLine("AUTHENTICATED.....");
                FormsIdentity id = (FormsIdentity)User.Identity;
                FormsAuthenticationTicket ticket = id.Ticket;
                System.Diagnostics.Debug.WriteLine("TICKET EMAIL: " + ticket.Name);
                Response.Redirect("/Home/Index");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Index(UserModel model)
        {


            if (model.UserName != null && model.Password != null)
            {
                
                ITMSEntities cbe = new ITMSEntities();
                ObjectResult<int?> s = cbe.GetLoginResult(model.UserName, model.Password);
                string item = s.FirstOrDefault().ToString();

                if (item != "")
                {
                    try
                    {
                        DataRow dr = model.getUser(item);

                        if (dr != null)
                        {
                            Session["IDUser"] = dr["IDUser"].ToString();
                            Session["UserName"] = dr["UserName"].ToString();
                            Session["UserEmail"] = dr["UserEmail"].ToString();
                            Session["UserType"] = dr["UserType"].ToString();
                            Session["lastLoginDate"] = dr["lastLoginDate"].ToString();
                            Session.Timeout = 20;
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

                            return Redirect(FormsAuthentication.GetRedirectUrl(dr["UserEmail"].ToString(), false));
                        }
                        else
                        {
                            ViewBag.NotValidUser = "Wrong Combination of Email and Password!";
                        }
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("ERRORORR: " + ex.Message);
                    }

                }
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
            HttpContext.User = null;

            return RedirectToAction("Index", "Home");

        }
    }
}
