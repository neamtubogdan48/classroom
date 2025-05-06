using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvc.Models
{
    public class Notification
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public DateTime timeSent { get; set; }
        [ForeignKey("UserAccount")]
        public string userId { get; set; }
    }
}
