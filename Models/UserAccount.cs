using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;
using Microsoft.AspNetCore.Identity;

namespace mvc.Models
{
    public class UserAccount : IdentityUser
    {
        public string accountType { get; set; }
        public bool notificationSettings { get; set; } = true;
        public string githubLink { get; set; }
        public string? profilePhoto { get; set; }
    }
}