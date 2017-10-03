using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RogueOne.Models
{
    public class Request
    {
        [Key]
        public long RequestID { get; set; }
        [ForeignKey("Requestor")]
        public string RequestorID { get; set; }
        public virtual ApplicationUser Requestor { get; set; }
        [ForeignKey("Acceptor")]
        public string AcceptorID { get; set; }
        public virtual ApplicationUser Acceptor { get; set; }
        public bool Accepted { get; set; }
    }
}