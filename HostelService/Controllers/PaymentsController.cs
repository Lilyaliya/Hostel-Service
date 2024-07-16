using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HostelService.Models;

namespace HostelService.Controllers
{
    public class PaymentsController : Controller
    {
        private HostelRegDB_datEntities db = new HostelRegDB_datEntities();

        // GET: Payments
        public ActionResult Index()
        {
            var payment = db.Payment.Include(p => p.Client).Include(p => p.Room);
            return View(payment.ToList());
        }

        // GET: Payments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payment.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            ViewBag.TotalCost = GetTotalCost(payment);
            return View(payment);
        }
        private decimal? GetTotalCost(Payment payment)
        {
            var pa =(from el in db.Booking
                     where payment.Room_ID == el.Room_ID && payment.Client_ID == el.Client_ID
                select el);
            var getDate = (from el in pa
                           //where DateTime.Now >= el.Arrival_date && DateTime.Now <= el.Departure_date
                           select new { el.Arrival_date, el.Departure_date , el.Room.Cost_p_day});
            TimeSpan subtr = getDate.First().Departure_date - getDate.First().Arrival_date;
            double val = subtr.TotalDays;
            var resultat = (double)getDate.First().Cost_p_day;
            decimal? totalCost = (decimal?)Math.Round(val * resultat, 2);
            return totalCost;
            
        }
        // GET: Payments/Create
        public ActionResult Create()
        {
            ViewBag.Client_ID = new SelectList(db.Client, "Client_ID", "Surname");
            ViewBag.Room_ID = new SelectList(db.Room, "Room_ID", "Room_num");
            return View();
        }

        // POST: Payments/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Receipt_num,Client_ID,Room_ID")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                db.Payment.Add(payment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Client_ID = new SelectList(db.Client, "Client_ID", "Surname", payment.Client_ID);
            ViewBag.Room_ID = new SelectList(db.Room, "Room_ID", "Room_num", payment.Room_ID);
            return View(payment);
        }

        // GET: Payments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payment.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            ViewBag.Client_ID = new SelectList(db.Client, "Client_ID", "Surname", payment.Client_ID);
            ViewBag.Room_ID = new SelectList(db.Room, "Room_ID", "Room_num", payment.Room_ID);
            return View(payment);
        }

        // POST: Payments/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Receipt_num,Client_ID,Room_ID")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(payment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Client_ID = new SelectList(db.Client, "Client_ID", "Surname", payment.Client_ID);
            ViewBag.Room_ID = new SelectList(db.Room, "Room_ID", "Room_num", payment.Room_ID);
            return View(payment);
        }

        // GET: Payments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payment.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Payment payment = db.Payment.Find(id);
            db.Payment.Remove(payment);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
