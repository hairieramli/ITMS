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
                System.Diagnostics.Debug.WriteLine("EnTER : " + form["ddl_prio"].ToString());
                if (HttpContext.Session["IDUser"] != null && HttpContext.Session["UserName"] != null && form["hidRep"].ToString() != "0" && form["hidEmp"].ToString() != "")
                {
                    

                    ITMSEntities2 et = new ITMSEntities2();
                    tbl_ticket tbl = new tbl_ticket();

                    DbSet test = et.Set<tbl_ticket>();
                    test.Add(new tbl_ticket { task_type = form["txt_task_type"].ToString().Trim(), priority = Int32.Parse(form["ddl_prio"].ToString()), TicketDate = DateTime.Now, IDrep=Int32.Parse(form["hidRep"].ToString()), IDtechnician=Int32.Parse(form["hidEmp"].ToString()), ticketStatus="In Process" });
                    int return_value = et.SaveChanges();
                    System.Diagnostics.Debug.WriteLine("RETURN VALUE : " + return_value);
                    System.Diagnostics.Debug.WriteLine("TASK TYPE : " + form["txt_task_type"].ToString().Trim());
                    TempData["tic_return_value"] = return_value;
                }


                return RedirectToAction("Index", "Report");
            }
            catch (Exception ex)
            {
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
