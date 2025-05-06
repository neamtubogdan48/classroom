using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvc.Models
{
    public class Assignment
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public DateTime deadline { get; set; }
        public string? requirementsDoc { get; set; }
        public bool lateTurnInOption { get; set; }
        public bool noDeadlineOption { get; set; }
        [ForeignKey("Classroom")]
        public int classroomId { get; set; }
    }
}
