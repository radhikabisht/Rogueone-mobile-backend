using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RogueOne.ViewModels
{
    public class LocationEntryViewModel
    {
        public long? LocationEntryID { get; set; }
        public string Address { get; set; }
        public string tripName { get; set; }
        public string DateCreated { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<string> BadgeNames { get; set; }
        public string Comments { get; set; }
    }
}