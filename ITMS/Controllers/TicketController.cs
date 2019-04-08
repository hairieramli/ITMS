using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ITMS.Models;

namespace ITMS.Controllers
{
    public class TicketController : Controller
    {
        // GET: Ticket
        QueryCode app = new QueryCode();
        public ActionResult Index()
        {
            return View();
        }

        // GET: Ticket/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Ticket/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Ticket/Create
        [HttpPost]
        public ActionResult Create(FormCollection form)
        {
            try
            {
                if(form["hidTic"].ToString() != "0" && HttpContext.Session["UserName"] != null)
                {
                    ITMSEntities2 et = new ITMSEntities2();
                    tbl_ticket t = new tbl_ticket()
                    {
                        IDticket = Int32.Parse(form["hidTic"].ToString()),
                        ticketStatus = form["ddl_status"].ToString(),
                        task_type = form["txt_task_type"].ToString(),
                        priority = Int32.Parse(form["ddl_prio"].ToString()),
                        TicketDate = DateTime.Parse(form["TicDate"].ToString()),
                        IDrep = Int32.Parse(form["hidRep"].ToString()),
                        IDtechnician = Int32.Parse(form["ddl_tech"].ToString())
                    };

                    et.tbl_ticket.Attach(t);
                    et.Entry(t).State = EntityState.Modified;
                    int return_value = et.SaveChanges();
                    if (return_value > 0)
                        app.NotifyUser("1", "Ticket", "Has Changed of Ticket to WORK DONE.");
                    TempData["edit_tic_value"] = return_value;
                }
                else if (HttpContext.Session["IDUser"] != null && HttpContext.Session["UserName"] != null && form["hidRep"].ToString() != "0" && form["ddl_tech"].ToString() != "0")
                {
                    

                    ITMSEntities2 et = new ITMSEntities2();
                    tbl_ticket tbl = new tbl_ticket();

                    DbSet test = et.Set<tbl_ticket>();
                    test.Add(new tbl_ticket { task_type = form["txt_task_type"].ToString().Trim(), priority = Int32.Parse(form["ddl_prio"].ToString()), TicketDate = DateTime.Now, IDrep=Int32.Parse(form["hidRep"].ToString()), IDtechnician=Int32.Parse(form["ddl_tech"].ToString()), ticketStatus="In Process" });
                    int return_value = et.SaveChanges();
                    if (return_value > 0)
                        app.NotifyUser(form["ddl_tech"].ToString(), "Report", "Has Assigned You A Task.");
                    TempData["tic_return_value"] = return_value;
                }


                return RedirectToAction("Index", "Report");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MASUK:::" + ex.Message);

                TempData["tic_return_value"] = 0;
                TempData["error"] = ex.Message;
                return RedirectToAction("Index", "Report");
            }
        }

        

        // GET: Ticket/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Ticket/Edit/5
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

        // GET: Ticket/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Ticket/Delete/5
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
