using Microsoft.EntityFrameworkCore;
using RVAegis.Models.UserModels;

namespace RVAegis.Contexts
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext() { }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserStatus> UserStatuses { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
