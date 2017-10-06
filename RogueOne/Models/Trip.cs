using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RogueOne.Models
{
    public class Trip
    {
        [Key]
        public int TripID { get; set; }
        public String TripName { get; set; }
        public string Destination { get; set; }
        public int PlannedDuration { get; set; }
        public DateTime StartDate { get; set; }
        public virtual List<LocationEntry> TripEntries{ get; set; }
        public virtual List<TripMate> TripMates { get; set; }
        public string Description { get; set; }

    }

    public class TripMate
    {
        [Key]
        public long TripMateID { get; set; }
        public string Name { get; set; }
        public long PhoneNumber { get; set; }
        public string Email { get; set; }
    }
}