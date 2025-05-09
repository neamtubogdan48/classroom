using System.Collections.Generic;
using mvc.Models;

namespace mvc.ViewModels
{
    public class UserClassroomsViewModel
    {
        public UserAccount UserAccount { get; set; }
        public List<ClassroomViewModel> Classrooms { get; set; } // Update the type to ClassroomViewModel
    }

    public class ClassroomViewModel
    {
        public Classroom Classroom { get; set; }
        public string ProfessorName { get; set; }
        public string ProfessorPhoto { get; set; }
    }
}