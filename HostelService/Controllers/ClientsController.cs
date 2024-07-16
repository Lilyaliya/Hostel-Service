using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq.Dynamic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using HostelService.Models;
using PagedList;

namespace HostelService.Controllers
{
    public class ClientsController : Controller
    {
        private HostelRegDB_datEntities db = new HostelRegDB_datEntities();

        // GET: Clients
        public ActionResult Index(string sortdir,  int? page, string currFilter = "", string sort = "Surname", string search = "")
        {
            ViewBag.CurrentSort = sortdir;//sortdir
            ViewBag.CurrCol = sort;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortdir) ? "desc" : "";
            
            //ViewBag.DateSortParm = sortOrder == "Date" ? "Date_desc" : "Date";
            int pageSize = 3;
            int totalRecord = 0;
            if (search != "") page = 1;
            else search = currFilter;
            ViewBag.currFilter = search;
            int pageNumber = (page ?? 1);
            //int skip = ((int)page * pageSize) - pageSize;
            var list = GetClients(search, sort, ViewBag.NameSortParm, pageNumber, pageSize, out totalRecord);
            ViewBag.TotalRows = totalRecord;
            ViewBag.search = search;
            
            return View(list);
            /*
               
               
             */
        }

        // GET: Clients/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Client.Find(id);
            if (client == null)
            {
                return HttpNotFound();
            }
            return View(client);
        }

        // GET: Clients/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Client_ID,Surname,FName,Second_name,Passport,Phone")] Client client)
        {
            if (ModelState.IsValid)
            {
                db.Client.Add(client);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(client);
        }

        // GET: Clients/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Client.Find(id);
            if (client == null)
            {
                return HttpNotFound();
            }
            return View(client);
        }

        // POST: Clients/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. 
        // Дополнительные сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Client_ID,Surname,FName,Second_name,Passport,Phone")] Client client)
        {
            if (ModelState.IsValid)
            {
                db.Entry(client).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(client);
        }

        // GET: Clients/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Client.Find(id);
            if (client == null)
            {
                return HttpNotFound();
            }
            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Client client = db.Client.Find(id);
            db.Client.Remove(client);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IPagedList<Client> GetClients(string search, string sort, string sortdir, int skip, int pageSize, out int totalRecord)
        {
            var v = from table in db.Client
                    select table;
            if (search != "")
            {
                v = (from el in db.Client
                         where
                                el.Surname.Contains(search) ||
                                el.FName.Contains(search) ||
                                el.Second_name.Contains(search) ||
                                el.Passport.Contains(search) ||
                                el.Phone.Contains(search)
                         select el);
               /* var a = (from p in db.Room
                         join b in db.Booking on p.Room_ID equals b.Room_ID
                         where b.Arrival_date < DateTime.Now || b.Room_ID==0
                         select p);*/
            }
            totalRecord = v.Count();
            v = v.OrderBy(sort + " " + sortdir);
            /*if (pageSize > 0)
            {
                v = v.Skip(skip).Take(pageSize); используется внутри ToPagedList метода
            }*/
            return (v.ToPagedList(skip, pageSize));
        }

        public ActionResult GetData()
        {
            List<Client> clientsList = db.Client.ToList<Client>();
            return Json(new { data = clientsList }, JsonRequestBehavior.AllowGet);
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
