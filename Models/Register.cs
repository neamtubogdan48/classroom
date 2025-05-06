using System.ComponentModel.DataAnnotations;

namespace mvc.data.models
{
    public class Register
    {
        [Required]
        [Display(Name = "Username")]
        public string username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string email { get; set; }
    }
}
