using HelpDesk.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<WorkLog> WorkLogs { get; set; }
        public DbSet<UserPassword> UserPasswords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships if needed, e.g.,
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.CreatedBy)
                .WithMany()
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.AssignedEngineer)
                .WithMany()
                .HasForeignKey(t => t.AssignedEngineerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
