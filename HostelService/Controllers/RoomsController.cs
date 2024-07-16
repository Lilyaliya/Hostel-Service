using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HostelService.Models;
using PagedList;

namespace HostelService.Controllers
{
    public class RoomsController : Controller
    {
        private HostelRegDB_datEntities db = new HostelRegDB_datEntities();

        // GET: Rooms
        public ActionResult Index(string sortdir, int? page, string currFilter = "", string sort = "Room_num", string search = "", string key_Temp = "false")
        {
            ViewBag.CurrentSort = sortdir;//sortdir
            ViewBag.CurrCol = sort;
            ViewBag.KeyTemp = key_Temp;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortdir) ? "desc" : "";

            //ViewBag.DateSortParm = sortOrder == "Date" ? "Date_desc" : "Date";
            int pageSize = 3;
            int totalRecord = 0;
            if (search != "") page = 1;
            else search = currFilter;
            ViewBag.currFilter = search;
            int pageNumber = (page ?? 1);
            //int skip = ((int)page * pageSize) - pageSize;
            var list = GetRooms(search, sort, ViewBag.NameSortParm, pageNumber, pageSize, key_Temp, out totalRecord);
            ViewBag.TotalRows = totalRecord;
            ViewBag.search = search;

            return View(list);
            //return View(db.Room.ToList());
        }

        // GET: Rooms/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = db.Room.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            return View(room);
        }

        // GET: Rooms/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Rooms/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Room_ID,Room_num,Floor_n,Places,Cost_p_day,Category")] Room room)
        {
            if (ModelState.IsValid)
            {
                db.Room.Add(room);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(room);
        }

        // GET: Rooms/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = db.Room.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            return View(room);
        }

        // POST: Rooms/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Room_ID,Room_num,Floor_n,Places,Cost_p_day,Category")] Room room)
        {
            if (ModelState.IsValid)
            {
                db.Entry(room).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(room);
        }

        // GET: Rooms/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Room room = db.Room.Find(id);
            if (room == null)
            {
                return HttpNotFound();
            }
            return View(room);
        }

        public IPagedList<Room> GetRooms(string search, string sort, string sortdir, int skip, int pageSize, string key_Temp, out int totalRecord)
        {
            var v = from table in db.Room
                    select table;
            if (key_Temp == "true")
            {
                var booking = db.Booking.Include(b => b.Client).Include(b => b.Room);
                var active = (from p in booking
                     where (p.Departure_date >= DateTime.Now && p.Arrival_date <= DateTime.Now)
                     select p.Room);//
                v = v.Except(active);
                if (search != "")
                {
                    v = (from el in v
                         where
                                el.Room_num.ToString().Contains(search) ||
                                el.Floor_n.ToString().StartsWith(search) ||
                                el.Places.ToString().Contains(search) ||
                                el.Cost_p_day.ToString().StartsWith(search) ||
                                el.Category.Contains(search)
                         select el);

                }
            }
            else if (key_Temp == "false")
            {
               
                if (search != "")
                {
                    v = (from el in db.Room
                         where
                                el.Room_num.ToString().Contains(search) ||
                                el.Floor_n.ToString().StartsWith(search) ||
                                el.Places.ToString().Contains(search) ||
                                el.Cost_p_day.ToString().StartsWith(search) ||
                                el.Category.Contains(search)
                         select el);

                }
            } 
            totalRecord = v.Count();
            v = v.OrderBy(sort + " " + sortdir);
            return (v.ToPagedList(skip, pageSize));
        }
        // POST: Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Room room = db.Room.Find(id);
            db.Room.Remove(room);
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
