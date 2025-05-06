using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvc.Models
{
    public class Document
    {
        public int id { get; set; }
        public string? studentDoc { get; set; }
        public int grade { get; set; }
        [ForeignKey("Assignment")]
        public int assignmentId { get; set; }
        [ForeignKey("UserAccount")]
        public string userId { get; set; }
    }
}
