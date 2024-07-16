using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace HostelService.ViewHistoryRooms
{
    public class HistoryRooms
    {
        public int Room_num { get; set; }
        public int Floor_n { get; set; }
        public string Surname { get; set; }
        public string FName { get; set; }
        public string Second_name { get; set; }
        public string Phone { get; set; }



    }
}