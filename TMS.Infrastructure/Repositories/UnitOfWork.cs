using Microsoft.EntityFrameworkCore.Storage;
using TMS.Domain.Entities;
using TMS.Domain.Interfaces;
using TMS.Infrastructure.Persistence;

namespace TMS.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    // ── Lazy-initialised repositories ─────────────────────────────────────────
    private IRepository<Student>? _students;
    private IRepository<Tutor>? _tutors;
    private IRepository<Course>? _courses;
    private IRepository<Class>? _classes;
    private IRepository<StudentClass>? _studentClasses;
    private IRepository<TutorCourse>? _tutorCourses;
    private IRepository<Attendance>? _attendances;
    private IRepository<FeeStructure>? _feeStructures;
    private IRepository<Invoice>? _invoices;
    private IRepository<InvoiceLineItem>? _invoiceLineItems;
    private IRepository<Payment>? _payments;
    private IRepository<Exam>? _exams;
    private IRepository<Result>? _results;
    private IRepository<Notification>? _notifications;

    // ── Study Materials Module ────────────────────────────────────────────────
    private IRepository<Subject>? _subjects;
    private IRepository<MaterialCategory>? _materialCategories;
    private IRepository<StudyMaterial>? _studyMaterials;
    private IRepository<SubjectClassMapping>? _subjectClassMappings;
    private IRepository<MaterialDownload>? _materialDownloads;
    private IRepository<MaterialBookmark>? _materialBookmarks;
    private IRepository<VideoProgress>? _videoProgresses;

    private IRepository<ContentModule>? _contentModules;
    private IRepository<ContentItem>? _contentItems;
    private IRepository<ContentItemAccess>? _contentItemAccesses;
    private IRepository<StudentContentProgress>? _studentContentProgress;

    public UnitOfWork(AppDbContext context) => _context = context;

    // ── Existing repositories ─────────────────────────────────────────────────
    public IRepository<Student> Students
        => _students ??= new Repository<Student>(_context);

    public IRepository<Tutor> Tutors
        => _tutors ??= new Repository<Tutor>(_context);

    public IRepository<Course> Courses
        => _courses ??= new Repository<Course>(_context);

    public IRepository<Class> Classes
        => _classes ??= new Repository<Class>(_context);

    public IRepository<StudentClass> StudentClasses
        => _studentClasses ??= new Repository<StudentClass>(_context);

    public IRepository<TutorCourse> TutorCourses
        => _tutorCourses ??= new Repository<TutorCourse>(_context);

    public IRepository<Attendance> Attendances
        => _attendances ??= new Repository<Attendance>(_context);

    public IRepository<FeeStructure> FeeStructures
        => _feeStructures ??= new Repository<FeeStructure>(_context);

    public IRepository<Invoice> Invoices
        => _invoices ??= new Repository<Invoice>(_context);

    public IRepository<InvoiceLineItem> InvoiceLineItems
        => _invoiceLineItems ??= new Repository<InvoiceLineItem>(_context);

    public IRepository<Payment> Payments
        => _payments ??= new Repository<Payment>(_context);

    public IRepository<Exam> Exams
        => _exams ??= new Repository<Exam>(_context);

    public IRepository<Result> Results
        => _results ??= new Repository<Result>(_context);

    public IRepository<Notification> Notifications
        => _notifications ??= new Repository<Notification>(_context);

    // ── Study Materials Module ────────────────────────────────────────────────
    public IRepository<Subject> Subjects
        => _subjects ??= new Repository<Subject>(_context);

    public IRepository<MaterialCategory> MaterialCategories
        => _materialCategories ??= new Repository<MaterialCategory>(_context);

    public IRepository<StudyMaterial> StudyMaterials
        => _studyMaterials ??= new Repository<StudyMaterial>(_context);

    public IRepository<SubjectClassMapping> SubjectClassMappings
        => _subjectClassMappings ??= new Repository<SubjectClassMapping>(_context);

    public IRepository<MaterialDownload> MaterialDownloads
        => _materialDownloads ??= new Repository<MaterialDownload>(_context);

    public IRepository<MaterialBookmark> MaterialBookmarks
        => _materialBookmarks ??= new Repository<MaterialBookmark>(_context);

    public IRepository<VideoProgress> VideoProgresses
        => _videoProgresses ??= new Repository<VideoProgress>(_context);

    // ── Unit of Work operations ───────────────────────────────────────────────
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _context.Database.BeginTransactionAsync(ct);

    public IRepository<ContentModule> ContentModules => _contentModules ??= new Repository<ContentModule>(_context);
    public IRepository<ContentItem> ContentItems => _contentItems ??= new Repository<ContentItem>(_context);
    public IRepository<ContentItemAccess> ContentItemAccesses => _contentItemAccesses ??= new Repository<ContentItemAccess>(_context);
    public IRepository<StudentContentProgress> StudentContentProgress => _studentContentProgress ??= new Repository<StudentContentProgress>(_context);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
            throw new InvalidOperationException("No active transaction.");
        await _transaction.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null) return;
        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}