using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TMS.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TMS.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── Domain DbSets ──────────────────────────────────────────────────────────
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Tutor> Tutors => Set<Tutor>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<StudentClass> StudentClasses => Set<StudentClass>();
    public DbSet<TutorCourse> TutorCourses => Set<TutorCourse>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<FeeStructure> FeeStructures => Set<FeeStructure>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all entity configurations
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // ── Global soft-delete query filters ────────────────────────────────
        builder.Entity<Student>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Tutor>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Course>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Class>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<StudentClass>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<TutorCourse>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Attendance>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<FeeStructure>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Invoice>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<InvoiceLineItem>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Payment>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Exam>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Result>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Notification>().HasQueryFilter(e => !e.IsDeleted);

        // ── Rename Identity tables ───────────────────────────────────────────
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}