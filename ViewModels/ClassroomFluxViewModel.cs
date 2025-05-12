using System.ComponentModel.DataAnnotations;
using mvc.Models;

namespace mvc.ViewModels
{
    public class ClassroomFluxViewModel
    {
        public Classroom Classroom { get; set; }
        public List<Assignment> Assignments { get; set; }
        public List<AssignmentChat> AssignmentChats { get; set; }
        public List<Document> Documents { get; set; }
        public UserAccount UserAccount { get; set; }
        public IEnumerable<UserAccount> Users { get; set; }

        // New property for ClassroomStudents
        public List<ClassroomStudents> ClassroomStudents { get; set; }


        //Document
        public string userId { get; set; }
        public int assignmentId { get; set; }
        public int grade {  get; set; }
        public IFormFile studentDocFile { get; set; }

        //Assignment
        [Required]
        public string name { get; set; }

        [Required]
        public string description { get; set; }

        [Required]
        public DateTime deadline { get; set; }

        public IFormFile requirementsDocFile { get; set; }

        [Required]
        public bool lateTurnInOption { get; set; }

        [Required]
        public bool noDeadlineOption { get; set; }

        [Required]
        public int classroomId { get; set; }
    }
}
