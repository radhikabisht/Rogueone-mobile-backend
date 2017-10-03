using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RogueOne.Models
{
    public class Settings
    {
        [Key]
        public long SettingsID { get; set; }
        public bool Safemode { get; set; }
    }
}