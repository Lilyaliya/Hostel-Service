using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HostelService.Models
{
    public class Booking_Payment
    {
        public int Receipt_ID { get; set; }
        public int Receipt_num { get; set; }
        public virtual Payment Payment { get; set; }
        public virtual Booking Booking { get; set; }
    }
}