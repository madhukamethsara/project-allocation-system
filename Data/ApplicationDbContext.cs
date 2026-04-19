using BlindMatchPAS.Models;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ResearchArea> ResearchAreas { get; set; }
        public DbSet<BatchAccess> BatchAccesses { get; set; }
        public DbSet<RegistrationRequest> RegistrationRequests { get; set; }
        public DbSet<ProjectFeedback> ProjectFeedbacks { get; set; }
        public DbSet<PinnedProject> PinnedProjects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.ResearchArea)
                .WithMany()
                .HasForeignKey(u => u.ResearchAreaId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<BatchAccess>()
                .HasIndex(b => b.BatchName)
                .IsUnique();

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Student)
                .WithMany(u => u.StudentProjects)
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Supervisor)
                .WithMany(u => u.SupervisedProjects)
                .HasForeignKey(p => p.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.ResearchArea)
                .WithMany(r => r.Projects)
                .HasForeignKey(p => p.ResearchAreaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectFeedback>()
                .HasOne(f => f.Project)
                .WithMany(p => p.Feedbacks)
                .HasForeignKey(f => f.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectFeedback>()
                .HasOne(f => f.Supervisor)
                .WithMany()
                .HasForeignKey(f => f.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PinnedProject>()
                .HasOne(p => p.Project)
                .WithMany(p => p.PinnedBy)
                .HasForeignKey(p => p.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PinnedProject>()
                .HasOne(p => p.Supervisor)
                .WithMany()
                .HasForeignKey(p => p.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PinnedProject>()
                .HasIndex(p => new { p.ProjectId, p.SupervisorId })
                .IsUnique();

            modelBuilder.Entity<ResearchArea>().HasData(
                new ResearchArea { Id = 1, Name = "Artificial Intelligence" },
                new ResearchArea { Id = 2, Name = "Web Development" },
                new ResearchArea { Id = 3, Name = "Cybersecurity" },
                new ResearchArea { Id = 4, Name = "Cloud Computing" },
                new ResearchArea { Id = 5, Name = "Machine Learning" },
                new ResearchArea { Id = 6, Name = "Mobile Development" },
                new ResearchArea { Id = 7, Name = "Data Science" },
                new ResearchArea { Id = 8, Name = "Internet of Things" }
            );

            modelBuilder.Entity<BatchAccess>().HasData(
                new BatchAccess { Id = 1, BatchName = "24.1", IsLoginEnabled = true },
                new BatchAccess { Id = 2, BatchName = "24.2", IsLoginEnabled = true },
                new BatchAccess { Id = 3, BatchName = "23.2", IsLoginEnabled = true }
            );
        }
    }
}