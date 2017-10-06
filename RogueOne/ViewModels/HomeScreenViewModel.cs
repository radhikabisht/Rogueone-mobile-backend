using RogueOne.Models;
using System.Collections.Generic;

namespace RogueOne.ViewModels
{
    public class HomeScreenViewModel
    {
        public Settings UserSettings { get; set; }
        public List<LocationEntry> Diary { get; set; }
        public List<Trip> TripEntries { get; set; }
        public int PendingRequests { get; set; }
        public int NoOfFriends { get; set; }
    }
}