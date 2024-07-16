using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HostelService.Models;
using HostelService.ViewHistoryRooms;
using PagedList;

namespace HostelService.Controllers
{
    
    public class HistoryRoomsController : Controller
    {
        private HostelRegDB_datEntities db = new HostelRegDB_datEntities();

        // GET: HistoryRooms
        public ActionResult Index(string sortdir, int? page, DateTime? requestedDate, string currFilter = "", string sort = "Room_num", string search = "")
        {
            ViewBag.CurrentSort = sortdir;//sortdir
            ViewBag.CurrCol = sort;
            ViewBag.RequestedDate = requestedDate;
            if (ViewBag.RequestedDate == null)
                ViewBag.RequestedDate = DateTime.Now;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortdir) ? "desc" : "";

            //ViewBag.DateSortParm = sortOrder == "Date" ? "Date_desc" : "Date";
            int pageSize = 3;
            int totalRecord = 0;
            if (search != "") page = 1;
            else search = currFilter;
            ViewBag.currFilter = search;
            int pageNumber = (page ?? 1);
            //int skip = ((int)page * pageSize) - pageSize;
            var list = GetHistory(search, sort, ViewBag.NameSortParm, pageNumber, pageSize, ViewBag.RequestedDate, out totalRecord);
            ViewBag.TotalRows = totalRecord;
            ViewBag.search = search;
            return View(list);
        }

        public IPagedList<HistoryRooms> GetHistory(string search, string sort, string sortdir, int skip, int pageSize, DateTime? requestedDate, out int totalRecord)
        {
            /*if (requestedDate == null)
            {
                requestedDate = DateTime.Now;
            }*/
            var collectionC = (from t in db.Booking
                               join c in db.Client on t.Client_ID equals c.Client_ID
                               where (requestedDate >= t.Arrival_date && requestedDate <= t.Departure_date)
                               select new { t.Receipt_ID, c.Surname, c.FName, c.Second_name, c.Phone });
            var collectionR = (from t in db.Booking
                               join r in db.Room on t.Room_ID equals r.Room_ID
                               where (requestedDate >= t.Arrival_date && requestedDate <= t.Departure_date)
                               select new { t.Receipt_ID, r.Room_num, r.Floor_n });
            IQueryable<HistoryRooms> result = (from first in collectionR
                          join second in collectionC on first.Receipt_ID equals second.Receipt_ID
                          select new HistoryRooms{ Room_num = first.Room_num, Floor_n = first.Floor_n, Surname = second.Surname,  FName = second.FName, Second_name = second.Second_name, Phone = second.Phone });
            if (search != "")
            {
                result = (from el in result
                     where
                            el.Room_num.ToString().StartsWith(search) ||
                            el.Floor_n.ToString().StartsWith(search) ||
                            el.Surname.ToString().Contains(search) ||
                            el.FName.ToString().Contains(search) ||
                            el.Second_name.Contains(search)        ||
                            el.Phone.StartsWith(search)
                     select el);

            }
            totalRecord = result.Count();
            //result = result.OrderBy(sort + " " + sortdir);
            switch (sort)
            {
                case "Room_num":
                    if (sortdir == "desc")
                        result = result.OrderByDescending(r => r.Room_num);
                    else
                        result = result.OrderBy(r=>r.Room_num);
                    break;
                case "Floor_n":
                    if (sortdir == "desc")
                        result = result.OrderByDescending(r => r.Floor_n);
                    else
                        result = result.OrderBy(r => r.Floor_n);
                    break;
                case "Surname":
                    if (sortdir == "desc")
                        result = result.OrderByDescending(r => r.Surname);
                    else
                        result = result.OrderBy(r => r.Surname);
                    break;
                case "FName":
                    if (sortdir == "desc")
                        result = result.OrderByDescending(r => r.FName);
                    else
                        result = result.OrderBy(r => r.FName);
                    break;
                case "Second_name":
                    if (sortdir == "desc")
                        result = result.OrderByDescending(r => r.Second_name);
                    else
                        result = result.OrderBy(r => r.Second_name);
                    break;
                case "Phone":
                    if (sortdir == "desc")
                        result = result.OrderByDescending(r => r.Phone);
                    else
                        result = result.OrderBy(r => r.Phone);
                    break;
                default:
                    result = result.OrderBy(x=>x.Room_num);
                    break;

            }
            return (result.ToPagedList(skip, pageSize));
        }
    }
}