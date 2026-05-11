using AutoMapper;
using TMS.Application.Common;
using TMS.Application.DTOs.Course;
using TMS.Application.DTOs.Student;
using TMS.Application.Interfaces;
using TMS.Domain.Entities;
using TMS.Domain.Enums;
using TMS.Domain.Exceptions;
using TMS.Domain.Interfaces;
using TMS.Domain.ValueObjects;

namespace TMS.Application.Services;

public class StudentService : IStudentService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly INumberGeneratorService _numbers;

    public StudentService(IUnitOfWork uow, IMapper mapper, INumberGeneratorService numbers)
    {
        _uow = uow;
        _mapper = mapper;
        _numbers = numbers;
    }

    // ── List ──────────────────────────────────────────────────────────────────

    public async Task<PagedResult<StudentSummaryDto>> GetAllAsync(
        PaginationQuery query, CancellationToken ct = default)
    {
        var (items, total) = await _uow.Students.GetPagedAsync(
            pageNumber: query.PageNumber,
            pageSize: query.PageSize,
            predicate: string.IsNullOrWhiteSpace(query.Search) ? null :
                        s => s.FirstName.Contains(query.Search) ||
                             s.LastName.Contains(query.Search) ||
                             s.Email.Contains(query.Search) ||
                             s.StudentNumber.Contains(query.Search),
            orderBy: s => s.LastName,
            ct: ct);

        var dtos = _mapper.Map<List<StudentSummaryDto>>(items);
        return PagedResult<StudentSummaryDto>.Create(dtos, total, query.PageNumber, query.PageSize);
    }

    // ── Detail ────────────────────────────────────────────────────────────────

    public async Task<StudentDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var student = await _uow.Students.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Student), id);

        var dto = _mapper.Map<StudentDto>(student);

        // Enrich with live stats
        dto.ActiveClassCount = await _uow.StudentClasses.CountAsync(
            sc => sc.StudentId == id && sc.Status == EnrollmentStatus.Active, ct);

        var invoices = await _uow.Invoices.FindAsync(i => i.StudentId == id, ct);
        dto.OutstandingBalance = invoices.Sum(i => i.BalanceAmount);

        var allAttendance = await _uow.Attendances.FindAsync(a => a.StudentId == id, ct);
        dto.AttendancePercentage = allAttendance.Count == 0 ? 0 :
            allAttendance.Count(a => a.Status is AttendanceStatus.Present or AttendanceStatus.Late)
            * 100.0 / allAttendance.Count;

        return dto;
    }

    // ── Create ────────────────────────────────────────────────────────────────

    public async Task<StudentDto> CreateAsync(
        CreateStudentRequest request, CancellationToken ct = default)
    {
        // Duplicate email check
        if (await _uow.Students.ExistsAsync(s => s.Email == request.Email, ct))
            throw new DomainException($"A student with email '{request.Email}' already exists.");

        var student = new Student
        {
            StudentNumber = await _numbers.GenerateStudentNumberAsync(ct),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber.Trim(),
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            GuardianName = request.GuardianName.Trim(),
            GuardianPhone = request.GuardianPhone.Trim(),
            GuardianEmail = request.GuardianEmail?.Trim() ?? string.Empty,
            GuardianRelationship = request.GuardianRelationship.Trim(),
            Notes = request.Notes,
            EnrollmentDate = DateTime.UtcNow,
            IsActive = true,
        };

        // Build address value object if provided
        if (!string.IsNullOrWhiteSpace(request.Street) && !string.IsNullOrWhiteSpace(request.City))
        {
            student.Address = new Address(
                request.Street,
                request.City,
                request.Province ?? string.Empty,
                request.PostalCode ?? string.Empty,
                request.Country ?? "Zambia");
        }

        await _uow.Students.AddAsync(student, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<StudentDto>(student);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public async Task<StudentDto> UpdateAsync(
        Guid id, UpdateStudentRequest request, CancellationToken ct = default)
    {
        var student = await _uow.Students.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Student), id);

        // Email uniqueness (exclude self)
        if (student.Email != request.Email.ToLowerInvariant() &&
            await _uow.Students.ExistsAsync(s => s.Email == request.Email && s.Id != id, ct))
            throw new DomainException($"Email '{request.Email}' is already in use.");

        student.FirstName = request.FirstName.Trim();
        student.LastName = request.LastName.Trim();
        student.Email = request.Email.Trim().ToLowerInvariant();
        student.PhoneNumber = request.PhoneNumber.Trim();
        student.DateOfBirth = request.DateOfBirth;
        student.Gender = request.Gender;
        student.ProfilePhotoUrl = request.ProfilePhotoUrl;
        student.GuardianName = request.GuardianName.Trim();
        student.GuardianPhone = request.GuardianPhone.Trim();
        student.GuardianEmail = request.GuardianEmail?.Trim() ?? string.Empty;
        student.GuardianRelationship = request.GuardianRelationship.Trim();
        student.Notes = request.Notes;
        student.IsActive = request.IsActive;

        if (!string.IsNullOrWhiteSpace(request.Street) && !string.IsNullOrWhiteSpace(request.City))
        {
            student.Address = new Address(
                request.Street!,
                request.City!,
                request.Province ?? string.Empty,
                request.PostalCode ?? string.Empty,
                request.Country ?? "Zambia");
        }

        await _uow.Students.UpdateAsync(student, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<StudentDto>(student);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var student = await _uow.Students.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Student), id);

        await _uow.Students.DeleteAsync(student, ct);  // soft delete
        await _uow.SaveChangesAsync(ct);
    }

    // ── Enrollment ────────────────────────────────────────────────────────────

    public async Task EnrollInClassAsync(
        Guid studentId, Guid classId, CancellationToken ct = default)
    {
        var student = await _uow.Students.GetByIdAsync(studentId, ct)
            ?? throw new NotFoundException(nameof(Student), studentId);

        var cls = await _uow.Classes.GetByIdAsync(classId, ct)
            ?? throw new NotFoundException(nameof(Class), classId);

        if (cls.IsFull)
            throw new ClassFullException(cls.Name);

        // Already enrolled?
        if (await _uow.StudentClasses.ExistsAsync(
                sc => sc.StudentId == studentId &&
                      sc.ClassId == classId &&
                      sc.Status == EnrollmentStatus.Active, ct))
            throw new DomainException("Student is already enrolled in this class.");

        var enrollment = new StudentClass
        {
            StudentId = studentId,
            ClassId = classId,
            EnrollmentDate = DateTime.UtcNow,
            Status = EnrollmentStatus.Active
        };

        await _uow.StudentClasses.AddAsync(enrollment, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task WithdrawFromClassAsync(
        Guid studentId, Guid classId, string reason, CancellationToken ct = default)
    {
        var enrollment = await _uow.StudentClasses.FirstOrDefaultAsync(
            sc => sc.StudentId == studentId &&
                  sc.ClassId == classId &&
                  sc.Status == EnrollmentStatus.Active, ct)
            ?? throw new NotFoundException("Enrollment", $"Student {studentId} / Class {classId}");

        enrollment.Status = EnrollmentStatus.Withdrawn;
        enrollment.WithdrawalReason = reason;
        enrollment.WithdrawalDate = DateTime.UtcNow;

        await _uow.StudentClasses.UpdateAsync(enrollment, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<List<ClassDto>> GetStudentClassesAsync(
        Guid studentId, CancellationToken ct = default)
    {
        var enrollments = await _uow.StudentClasses.FindAsync(
            sc => sc.StudentId == studentId && sc.Status == EnrollmentStatus.Active, ct);

        var classIds = enrollments.Select(e => e.ClassId).ToList();
        var classes = await _uow.Classes.FindAsync(c => classIds.Contains(c.Id), ct);

        return _mapper.Map<List<ClassDto>>(classes);
    }
}
