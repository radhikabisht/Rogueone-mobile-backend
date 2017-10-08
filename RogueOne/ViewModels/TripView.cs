using RogueOne.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RogueOne.ViewModels
{
    public class TripView
    {
        public int TripID { get; set; }
        public string TripName { get; set; }
        public string Destination { get; set; }
        public int PlannedDuration { get; set; }
        public string StartDate { get; set; }
        public List<LocationEntryViewModel> TripEntries { get; set; }
        public List<TripMate> TripMates { get; set; }
        public string Description { get; set; }
    }
    
}