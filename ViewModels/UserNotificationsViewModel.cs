using System.Collections.Generic;
using mvc.Models;

namespace mvc.ViewModels
{
    public class UserNotificationsViewModel
    {
        public UserAccount UserAccount { get; set; } // Detalii despre utilizator
        public List<Notification> Notifications { get; set; } // Lista notific?rilor
        public List<Assignment> Assignments { get; set; } // Lista temelor
        public List<Document> Documents { get; set; } // Lista documentelor
        public List<Classroom> Classrooms { get; set; } // Lista claselor
    }
}
