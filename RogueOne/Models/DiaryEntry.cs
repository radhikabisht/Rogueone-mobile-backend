using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RogueOne.Models
{
    public class DiaryEntry
    {
        [Key]
        public long DiaryEntryID { get; set; }
        public virtual List<CheckIn> Visits { get; set; }
        public DateTime DateCreated { get; set; }
        [ForeignKey("Location")]
        public long LocationID { get; set; }
        public virtual Location Location { get; set; }
        public virtual List<Badge> LocationBadge { get; set; }
        
    }

    public class Badge
    {
        [Key]
        public int BadgeID { get; set; }
        public string BadgeName { get; set; }
    }
}