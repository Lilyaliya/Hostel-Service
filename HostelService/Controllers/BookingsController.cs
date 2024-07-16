using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HostelService.Models;
using System.ComponentModel.DataAnnotations;
using PagedList;

namespace HostelService.Controllers
{
    public class BookingsController : Controller
    {
        private HostelRegDB_datEntities db = new HostelRegDB_datEntities();

        // GET: Bookings
        public ActionResult Index(string sortdir, int? page, DateTime? requestedDate, string currFilter = "", string sort = "Receipt_date", string search = "")
        {
            ViewBag.CurrentSort = sortdir;//sortdir
            ViewBag.CurrCol = sort;
            ViewBag.RequestedDate = requestedDate;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortdir) ? "desc" : "";

            //ViewBag.DateSortParm = sortOrder == "Date" ? "Date_desc" : "Date";
            int pageSize = 3;
            int totalRecord = 0;
            if (search != "") page = 1;
            else search = currFilter;
            ViewBag.currFilter = search;
            int pageNumber = (page ?? 1);
            //int skip = ((int)page * pageSize) - pageSize;
            var list = GetBookings(search, sort, ViewBag.NameSortParm, pageNumber, pageSize, ViewBag.RequestedDate, out totalRecord);
            ViewBag.TotalRows = totalRecord;
            ViewBag.search = search;

            return View(list);
            //booking = booking.OrderBy(x => x.Client.Surname);
            //return View(booking.ToList());
        }

        private IPagedList<Booking> GetBookings(string search, string sort, string sortdir, int pageNumber, int pageSize, DateTime? requestedDate , out int totalRecord)
        {
            /*IQueryable<Booking> v = from table in db.Booking
                    select table;*/
            var booking = db.Booking.Include(b => b.Client).Include(b => b.Room);
            if (requestedDate == null)
            {
                //requestedDate = DateTime.Now;
                if (search != "")
                {
                    booking = (from el in booking
                               where
                                    el.Client.Surname.Contains(search) ||
                                    el.Client.FName.Contains(search) ||
                                    el.Client.Second_name.Contains(search) ||
                                    el.Room.Room_num.ToString().StartsWith(search)
                               select el);
                }
            }
            else if (requestedDate != null)
            {
                booking = (from el in booking
                           where
                                (requestedDate <= el.Departure_date && requestedDate >= el.Arrival_date)
                           select el);
                if (search != "")
                {
                    booking = (from el in booking
                               where
                                    el.Client.Surname.Contains(search) ||
                                    el.Client.FName.Contains(search) ||
                                    el.Client.Second_name.Contains(search) ||
                                    el.Room.Room_num.ToString().StartsWith(search)
                               select el);
                }
            }

            totalRecord = booking.Count();
            switch (sort)
            {
                case "Receipt_date":
                    if (sortdir == "desc")
                        booking = booking.OrderByDescending(r => r.Receipt_date);
                    else
                        booking = booking.OrderBy(r => r.Receipt_date);
                    break;
                case "Arrival_date":
                    if (sortdir == "desc")
                        booking = booking.OrderByDescending(r => r.Arrival_date);
                    else
                        booking = booking.OrderBy(r => r.Arrival_date);
                    break;
                case "Departure_date":
                    if (sortdir == "desc")
                        booking = booking.OrderByDescending(r => r.Departure_date);
                    else
                        booking = booking.OrderBy(r => r.Departure_date);
                    break;
                case "Client_ID":
                    if (sortdir == "desc")
                        booking = booking.OrderByDescending(r => r.Client.Surname);
                    else
                        booking = booking.OrderBy(r => r.Client.Surname);
                    break;
                case "Room_ID":
                    if (sortdir == "desc")
                        booking = booking.OrderByDescending(r => r.Room.Room_num);
                    else
                        booking = booking.OrderBy(r => r.Room.Room_num);
                    break;
                default:
                    booking = booking.OrderBy(x => x.Receipt_date);
                    break;

            }
            return (booking.ToPagedList(pageNumber, pageSize));
        }

 
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Payment payment = db.Payment.Find(id);
            //Booking_Payment bp = new Booking_Payment();
            Booking booking = db.Booking.Find(id);
            var el = booking.Payment;
            Payment result = null;
            foreach (var e in el)
            {
                var temp = e.Booking;
                foreach (var ant in temp)
                {
                    if (ant.Receipt_ID == id)
                        result = e;
                }
            }
            //Booking_Payment bp = db.Booking_Payment.Where(x => x.Receipt_ID == id).First();
            //Payment payment = db.Payment.Find(bp.Receipt_num);
            if (result == null)
            {
                return HttpNotFound();
            }
            ViewBag.TotalCost = GetTotalCost(result);
            return View(result);
        }
        private decimal? GetTotalCost(Payment payment)
        {
            var pa = (from el in db.Booking
                      where payment.Room_ID == el.Room_ID && payment.Client_ID == el.Client_ID
                      select el);
            var getDate = (from el in pa
                               //where DateTime.Now >= el.Arrival_date && DateTime.Now <= el.Departure_date
                           select new { el.Arrival_date, el.Departure_date, el.Room.Cost_p_day });
            TimeSpan subtr = getDate.First().Departure_date - getDate.First().Arrival_date;
            double val = subtr.TotalDays;
            var resultat = (double)getDate.First().Cost_p_day;
            decimal? totalCost = (decimal?)Math.Round(val * resultat, 2);
            return totalCost;

        }
        // GET: Bookings/Create
        private IEnumerable<Room> FreeRooms()
        {
            var v = (from el in db.Room
                     select el);
            var booking = db.Booking.Include(b => b.Client).Include(b => b.Room);
            var active = (from p in booking
                          where (p.Departure_date >= DateTime.Now && p.Arrival_date <= DateTime.Now)
                          select p.Room);//
            v = v.Except(active);
            return v;
        }
        public ActionResult Create()
        {
            var list = FreeRooms();
            if (list == null)
                ViewBag.Message = "Все комнаты заняты на сегодня заняты!";
            else
                ViewBag.Message = String.Empty;
            ViewBag.Client_ID = new SelectList(db.Client, "Client_ID", "Surname");
            ViewBag.Room_ID = new SelectList(list, "Room_ID", "Room_num");//ViewBag.Room_ID = new SelectList(db.Room, "Room_ID", "Room_num");
            //CreatePayment();
            return View();
        }

        // POST: Bookings/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Receipt_ID,Receipt_date,Arrival_date,Departure_date,Client_ID,Room_ID")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                db.Booking.Add(booking);
                db.SaveChanges();
                Payment payment = new Payment();
                payment.Client_ID = booking.Client_ID;
                payment.Room_ID = booking.Room_ID;

                payment = db.Payment.Add(payment);
                payment.Booking.Add(booking);
                booking.Payment.Add(payment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            var list = FreeRooms();
            if (list == null)
                ViewBag.Message = "Все комнаты заняты на сегодня заняты!";
            else
                ViewBag.Message = String.Empty;
            ViewBag.Client_ID = new SelectList(db.Client, "Client_ID", "Surname", booking.Client_ID);
            ViewBag.Room_ID = new SelectList(list, "Room_ID", "Room_num", booking.Room_ID);
            return View(booking);
        }
        public void Add_Booking_Payment(Booking_Payment booking_Payment)
        {
            if (ModelState.IsValid)
            {
                //db..Add(booking_Payment);
                db.Booking_Payment.Add(booking_Payment);
                db.SaveChanges();
                //return RedirectToAction("Index");
            }
        }
        // GET: Bookings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                ModelState.AddModelError("Failure", "Невозможно найти текущую запись");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Booking.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            var list = FreeRooms();
            if (list == null)
                ViewBag.Message = "Все комнаты заняты на сегодня заняты!";
            else
                ViewBag.Message = String.Empty;
            ViewBag.Client_ID = new SelectList(db.Client, "Client_ID", "Surname", booking.Client_ID);
            ViewBag.Room_ID = new SelectList(list, "Room_ID", "Room_num", booking.Room_ID);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Receipt_ID,Receipt_date,Arrival_date,Departure_date,Client_ID,Room_ID")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                db.Entry(booking).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            var list = FreeRooms();
            if (list == null)
                ViewBag.Message = "Все комнаты заняты на сегодня заняты!";
            else
                ViewBag.Message = String.Empty;
            ViewBag.Client_ID = new SelectList(db.Client, "Client_ID", "Surname", booking.Client_ID);
            ViewBag.Room_ID = new SelectList(list, "Room_ID", "Room_num", booking.Room_ID);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Booking.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Booking booking = db.Booking.Find(id);
            while (booking.Payment.Count != 0)
            {
                db.Payment.Remove(booking.Payment.First());
                db.SaveChanges();}
            db.Booking.Remove(booking);
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
