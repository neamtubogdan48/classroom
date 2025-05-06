using System.ComponentModel.DataAnnotations;

namespace mvc.data.models
{
    public class Login
    {
        [Required]
        [Display(Name = "Username")]
        public string username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }
    }
}
