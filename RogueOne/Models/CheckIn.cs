using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RogueOne.Models
{
    public class CheckIn
    {
        [Key]
        public long CheckInID { get; set; }
        [ForeignKey("Location")]
        public long LocationID { get; set; }
        public Location Location { get; set; }
        public DateTime DateCreated { get; set; }
    }
}