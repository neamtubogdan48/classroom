using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvc.Models
{
    public class AssignmentChat
    {
        public int id { get; set; }
        public string message { get; set; }
        [ForeignKey("UserAccount")]
        public string userId { get; set; }
        [ForeignKey("UserAccount")]
        public string studentId { get; set; }
        [ForeignKey("Assignment")]
        public int assignmentId { get; set; }
    }
}
