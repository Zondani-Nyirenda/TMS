using TMS.Domain.Entities;

namespace TMS.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern — wraps all repositories in a single transaction boundary.
/// Call SaveChangesAsync() once after all operations to commit atomically.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // ── Core repositories ─────────────────────────────────────────────────────
    IRepository<Student> Students { get; }
    IRepository<Tutor> Tutors { get; }
    IRepository<Course> Courses { get; }
    IRepository<Class> Classes { get; }
    IRepository<StudentClass> StudentClasses { get; }
    IRepository<TutorCourse> TutorCourses { get; }
    IRepository<Attendance> Attendances { get; }
    IRepository<FeeStructure> FeeStructures { get; }
    IRepository<Invoice> Invoices { get; }
    IRepository<InvoiceLineItem> InvoiceLineItems { get; }
    IRepository<Payment> Payments { get; }
    IRepository<Exam> Exams { get; }
    IRepository<Result> Results { get; }
    IRepository<Notification> Notifications { get; }

    // ── Study Materials Module ────────────────────────────────────────────────
    IRepository<Subject> Subjects { get; }
    IRepository<MaterialCategory> MaterialCategories { get; }
    IRepository<StudyMaterial> StudyMaterials { get; }
    IRepository<SubjectClassMapping> SubjectClassMappings { get; }
    IRepository<MaterialDownload> MaterialDownloads { get; }
    IRepository<MaterialBookmark> MaterialBookmarks { get; }
    IRepository<VideoProgress> VideoProgresses { get; }

    IRepository<ContentModule> ContentModules { get; }
    IRepository<ContentItem> ContentItems { get; }
    IRepository<ContentItemAccess> ContentItemAccesses { get; }
    IRepository<StudentContentProgress> StudentContentProgress { get; }



    // ── Unit of Work operations ───────────────────────────────────────────────
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}