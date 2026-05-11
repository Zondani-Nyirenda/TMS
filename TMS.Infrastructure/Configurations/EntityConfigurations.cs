using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TMS.Domain.Entities;
using TMS.Domain.Enums;

namespace TMS.Infrastructure.Persistence.Configurations;

// ── Student ──────────────────────────────────────────────────────────────────

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.StudentNumber).IsRequired().HasMaxLength(20);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.PhoneNumber).HasMaxLength(30);
        builder.Property(e => e.ProfilePhotoUrl).HasMaxLength(500);
        builder.Property(e => e.GuardianName).HasMaxLength(150);
        builder.Property(e => e.GuardianPhone).HasMaxLength(30);
        builder.Property(e => e.GuardianEmail).HasMaxLength(256);
        builder.Property(e => e.GuardianRelationship).HasMaxLength(50);
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.Gender).HasConversion<string>().HasMaxLength(20);

        // Address as owned type (columns in Students table — no join)
        builder.OwnsOne(e => e.Address, a =>
        {
            a.Property(x => x.Street).HasColumnName("AddressStreet").HasMaxLength(200);
            a.Property(x => x.City).HasColumnName("AddressCity").HasMaxLength(100);
            a.Property(x => x.Province).HasColumnName("AddressProvince").HasMaxLength(100);
            a.Property(x => x.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(20);
            a.Property(x => x.Country).HasColumnName("AddressCountry").HasMaxLength(100);
        });

        builder.HasIndex(e => e.StudentNumber).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.IsDeleted);
        builder.HasIndex(e => e.IsActive);

        // Optional link to Identity user
        builder.HasOne(e => e.User)
               .WithMany()
               .HasForeignKey(e => e.UserId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

// ── Tutor ─────────────────────────────────────────────────────────────────────

public class TutorConfiguration : IEntityTypeConfiguration<Tutor>
{
    public void Configure(EntityTypeBuilder<Tutor> builder)
    {
        builder.ToTable("Tutors");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.StaffNumber).IsRequired().HasMaxLength(20);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.PhoneNumber).HasMaxLength(30);
        builder.Property(e => e.Qualification).HasMaxLength(200);
        builder.Property(e => e.Specialization).HasMaxLength(200);
        builder.Property(e => e.Bio).HasMaxLength(2000);
        builder.Property(e => e.AvailabilityNotes).HasMaxLength(500);
        builder.Property(e => e.ProfilePhotoUrl).HasMaxLength(500);
        builder.Property(e => e.Gender).HasConversion<string>().HasMaxLength(20);

        builder.OwnsOne(e => e.Address, a =>
        {
            a.Property(x => x.Street).HasColumnName("AddressStreet").HasMaxLength(200);
            a.Property(x => x.City).HasColumnName("AddressCity").HasMaxLength(100);
            a.Property(x => x.Province).HasColumnName("AddressProvince").HasMaxLength(100);
            a.Property(x => x.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(20);
            a.Property(x => x.Country).HasColumnName("AddressCountry").HasMaxLength(100);
        });

        builder.HasIndex(e => e.StaffNumber).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.IsDeleted);

        builder.HasOne(e => e.User)
               .WithMany()
               .HasForeignKey(e => e.UserId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

// ── Course ────────────────────────────────────────────────────────────────────

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.Subject).HasMaxLength(100);
        builder.Property(e => e.GradeLevel).HasMaxLength(50);
        builder.Property(e => e.ThumbnailUrl).HasMaxLength(500);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.FeeAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.FeeCurrency).HasMaxLength(5).HasDefaultValue("ZMW");
        builder.Property(e => e.FeeDescription).HasMaxLength(300);

        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.IsDeleted);
    }
}

// ── Class ─────────────────────────────────────────────────────────────────────

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("Classes");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Room).HasMaxLength(50);
        builder.Property(e => e.Location).HasMaxLength(200);
        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.DayOfWeek).HasConversion<string>().HasMaxLength(15);

        builder.HasOne(e => e.Course)
               .WithMany(c => c.Classes)
               .HasForeignKey(e => e.CourseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Tutor)
               .WithMany(t => t.Classes)
               .HasForeignKey(e => e.TutorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.CourseId);
        builder.HasIndex(e => e.TutorId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.IsDeleted);
    }
}

// ── StudentClass (M:M) ────────────────────────────────────────────────────────

public class StudentClassConfiguration : IEntityTypeConfiguration<StudentClass>
{
    public void Configure(EntityTypeBuilder<StudentClass> builder)
    {
        builder.ToTable("StudentClasses");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.WithdrawalReason).HasMaxLength(500);

        builder.HasOne(e => e.Student)
               .WithMany(s => s.StudentClasses)
               .HasForeignKey(e => e.StudentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Class)
               .WithMany(c => c.StudentClasses)
               .HasForeignKey(e => e.ClassId)
               .OnDelete(DeleteBehavior.Cascade);

        // A student can only be enrolled once per class (active enrollment)
        builder.HasIndex(e => new { e.StudentId, e.ClassId }).IsUnique();
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.IsDeleted);
    }
}

// ── TutorCourse (M:M) ─────────────────────────────────────────────────────────

public class TutorCourseConfiguration : IEntityTypeConfiguration<TutorCourse>
{
    public void Configure(EntityTypeBuilder<TutorCourse> builder)
    {
        builder.ToTable("TutorCourses");
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.Tutor)
               .WithMany(t => t.TutorCourses)
               .HasForeignKey(e => e.TutorId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Course)
               .WithMany(c => c.TutorCourses)
               .HasForeignKey(e => e.CourseId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.TutorId, e.CourseId }).IsUnique();
    }
}

// ── Attendance ────────────────────────────────────────────────────────────────

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("Attendances");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Remarks).HasMaxLength(500);
        builder.Property(e => e.RecordedBy).HasMaxLength(256);

        builder.HasOne(e => e.Student)
               .WithMany(s => s.Attendances)
               .HasForeignKey(e => e.StudentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Class)
               .WithMany(c => c.Attendances)
               .HasForeignKey(e => e.ClassId)
               .OnDelete(DeleteBehavior.Cascade);

        // Unique: one record per student per class per date
        builder.HasIndex(e => new { e.StudentId, e.ClassId, e.Date }).IsUnique();
        builder.HasIndex(e => e.Date);
        builder.HasIndex(e => e.IsSynced);
        builder.HasIndex(e => e.IsDeleted);
    }
}

// ── FeeStructure ──────────────────────────────────────────────────────────────

public class FeeStructureConfiguration : IEntityTypeConfiguration<FeeStructure>
{
    public void Configure(EntityTypeBuilder<FeeStructure> builder)
    {
        builder.ToTable("FeeStructures");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Amount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.Currency).HasMaxLength(5).HasDefaultValue("ZMW");

        builder.HasOne(e => e.Course)
               .WithMany(c => c.FeeStructures)
               .HasForeignKey(e => e.CourseId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.CourseId);
        builder.HasIndex(e => e.IsActive);
    }
}

// ── Invoice ───────────────────────────────────────────────────────────────────

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(30);
        builder.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
        builder.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
        builder.Property(e => e.Currency).HasMaxLength(5).HasDefaultValue("ZMW");
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.Notes).HasMaxLength(1000);

        // BalanceAmount is computed — ignore it in EF
        builder.Ignore(e => e.BalanceAmount);

        builder.HasOne(e => e.Student)
               .WithMany(s => s.Invoices)
               .HasForeignKey(e => e.StudentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.InvoiceNumber).IsUnique();
        builder.HasIndex(e => e.StudentId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.DueDate);
        builder.HasIndex(e => e.IsDeleted);
    }
}

// ── InvoiceLineItem ───────────────────────────────────────────────────────────

public class InvoiceLineItemConfiguration : IEntityTypeConfiguration<InvoiceLineItem>
{
    public void Configure(EntityTypeBuilder<InvoiceLineItem> builder)
    {
        builder.ToTable("InvoiceLineItems");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description).IsRequired().HasMaxLength(300);
        builder.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(e => e.Discount).HasColumnType("decimal(18,2)").HasDefaultValue(0);

        builder.Ignore(e => e.LineTotal);

        builder.HasOne(e => e.Invoice)
               .WithMany(i => i.LineItems)
               .HasForeignKey(e => e.InvoiceId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.FeeStructure)
               .WithMany()
               .HasForeignKey(e => e.FeeStructureId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}

// ── Payment ───────────────────────────────────────────────────────────────────

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReceiptNumber).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Amount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.Currency).HasMaxLength(5).HasDefaultValue("ZMW");
        builder.Property(e => e.Method).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Reference).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.HasOne(e => e.Student)
               .WithMany(s => s.Payments)
               .HasForeignKey(e => e.StudentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Invoice)
               .WithMany(i => i.Payments)
               .HasForeignKey(e => e.InvoiceId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ReceiptNumber).IsUnique();
        builder.HasIndex(e => e.StudentId);
        builder.HasIndex(e => e.PaymentDate);
        builder.HasIndex(e => e.IsDeleted);
    }
}

// ── Exam ──────────────────────────────────────────────────────────────────────

public class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.ToTable("Exams");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.TotalMarks).HasColumnType("decimal(10,2)");
        builder.Property(e => e.PassMark).HasColumnType("decimal(10,2)");
        builder.Property(e => e.Instructions).HasMaxLength(2000);

        builder.HasOne(e => e.Class)
               .WithMany(c => c.Exams)
               .HasForeignKey(e => e.ClassId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ClassId);
        builder.HasIndex(e => e.ExamDate);
        builder.HasIndex(e => e.IsPublished);
        builder.HasIndex(e => e.IsDeleted);
    }
}

// ── Result ────────────────────────────────────────────────────────────────────

public class ResultConfiguration : IEntityTypeConfiguration<Result>
{
    public void Configure(EntityTypeBuilder<Result> builder)
    {
        builder.ToTable("Results");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.MarksObtained).HasColumnType("decimal(10,2)");
        builder.Property(e => e.Grade).HasConversion<string>().HasMaxLength(10);
        builder.Property(e => e.Remarks).HasMaxLength(500);

        // Percentage is computed — do not persist
        builder.Ignore(e => e.Percentage);

        builder.HasOne(e => e.Exam)
               .WithMany(e => e.Results)
               .HasForeignKey(e => e.ExamId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Student)
               .WithMany(s => s.Results)
               .HasForeignKey(e => e.StudentId)
               .OnDelete(DeleteBehavior.Cascade);

        // A student can only have one result per exam
        builder.HasIndex(e => new { e.ExamId, e.StudentId }).IsUnique();
        builder.HasIndex(e => e.IsDeleted);
    }
}

// ── Notification ──────────────────────────────────────────────────────────────

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Message).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.ActionUrl).HasMaxLength(500);

        builder.HasOne(e => e.User)
               .WithMany(u => u.Notifications)
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.IsRead);
        builder.HasIndex(e => e.IsDeleted);
    }
}

// ── ApplicationUser (extends Identity) ───────────────────────────────────────

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Role).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.ProfilePhotoUrl).HasMaxLength(500);
        builder.Property(e => e.RefreshToken).HasMaxLength(500);
        builder.Property(e => e.PushSubscriptionJson).HasMaxLength(2000);

        builder.HasIndex(e => e.Role);
        builder.HasIndex(e => e.IsActive);
    }
}
