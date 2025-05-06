using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using mvc.data.models;
using mvc.Models;

namespace mvc.Data
{
    // Update the AppDbContext class to inherit from IdentityDbContext<Users>
    public class AppDbContext : IdentityDbContext<UserAccount>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Assignment> Assignment { get; set; }
        public DbSet<AssignmentChat> AssignmentChat { get; set; }
        public DbSet<Classroom> Classroom { get; set; }
        public DbSet<ClassroomStudents> ClassroomStudents { get; set; }
        public DbSet<Document> Document { get; set; }
        public DbSet<Notification> Notification { get; set; }
    }
}
