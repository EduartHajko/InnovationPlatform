using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InnovationPlatform.Models;

namespace InnovationPlatform.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Application> Applications { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ApplicationFile> ApplicationFiles { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<User> SimpleUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<Application>()
                .HasOne(a => a.User)
                .WithMany(u => u.Applications)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Application>()
                .HasOne(a => a.AssignedExpert)
                .WithMany(u => u.AssignedApplications)
                .HasForeignKey(a => a.AssignedExpertId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Application>()
                .HasOne(a => a.Category)
                .WithMany(c => c.Applications)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationFile>()
                .HasOne(f => f.Application)
                .WithMany(a => a.Files)
                .HasForeignKey(f => f.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Note>()
                .HasOne(n => n.Application)
                .WithMany(a => a.Notes)
                .HasForeignKey(n => n.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Note>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notes)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Teknologji dhe Inovacion Digjital", Description = "Aplikacione, platforma digjitale, AI, IoT" },
                new Category { Id = 2, Name = "Shëndetësi dhe Mirëqenie", Description = "Zgjidhje për përmirësimin e shëndetësisë publike" },
                new Category { Id = 3, Name = "Arsim dhe Formim", Description = "Metoda të reja mësimdhënieje dhe trajnimi" },
                new Category { Id = 4, Name = "Mjedis dhe Qëndrueshmëri", Description = "Zgjidhje për mbrojtjen e mjedisit" },
                new Category { Id = 5, Name = "Ekonomi dhe Biznes", Description = "Ide për zhvillimin ekonomik dhe sipërmarrjen" },
                new Category { Id = 6, Name = "Kulturë dhe Arte", Description = "Projekte kulturore dhe artistike" },
                new Category { Id = 7, Name = "Transport dhe Infrastrukturë", Description = "Zgjidhje për përmirësimin e transportit" },
                new Category { Id = 8, Name = "Bujqësi dhe Ushqim", Description = "Inovacione në bujqësi dhe industrinë ushqimore" }
            );
        }
    }
}
