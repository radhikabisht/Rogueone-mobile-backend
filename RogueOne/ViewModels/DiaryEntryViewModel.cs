using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RogueOne.ViewModels
{
    public class DiaryEntryViewModel
    {
        public String DateCreated { get; set; }
        public String DisplayFriendlyName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<String> BadgeNames { get; set; }
        public String Description { get; set; }
    }
}