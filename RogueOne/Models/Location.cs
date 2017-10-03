using System.ComponentModel.DataAnnotations;
using System.Security.Policy;

namespace RogueOne.Models
{
    public class Location
    {
        [Key]
        public long LocationID { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string DisplayFriendlyName { get; set; }
        public string ImageUrl { get; set; }
    }
}