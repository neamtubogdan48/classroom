using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvc.Models
{
    public class Classroom
    {
        public int id { get; set; }
        public string name { get; set; }
        public string? photo { get; set; }
        public int code { get; set; }
        [ForeignKey("UserAccount")]
        public string professorId { get; set; }
    }
}
