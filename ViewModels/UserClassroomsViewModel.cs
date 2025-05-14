using System.Collections.Generic;
using mvc.Models;

namespace mvc.ViewModels
{
    public class UserClassroomsViewModel
    {
        public UserAccount UserAccount { get; set; }
        public List<ClassroomViewModel> Classrooms { get; set; } // Update the type to ClassroomViewModel
        public IEnumerable<ClassroomViewModel> AllClassrooms { get; set; } // Update the type to ClassroomViewModel
        public string name { get; set; }
        public int code{ get; set; }
        public string professorId { get; set; }
        public string? photo { get; set; }

    }

    public class ClassroomViewModel
    {
        public Classroom Classroom { get; set; }
        public string ProfessorName { get; set; }
        public string ProfessorPhoto { get; set; }
    }
}