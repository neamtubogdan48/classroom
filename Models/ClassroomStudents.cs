using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvc.Models
{
    public class ClassroomStudents
    {
        public int id { get; set; }
        [ForeignKey("UserAccount")]
        public string userId { get; set; }
        [ForeignKey("Classroom")]
        public int classroomId { get; set; }
    }
}
