using AutoMapper;
using TMS.Application.Common;
using TMS.Application.DTOs.Course;
using TMS.Application.DTOs.Student;
using TMS.Application.Interfaces;
using TMS.Domain.Entities;
using TMS.Domain.Enums;
using TMS.Domain.Exceptions;
using TMS.Domain.Interfaces;

namespace TMS.Application.Services;

// ── Course Service ─────────────────────────────────────────────────────────────

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CourseService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<CourseDto>> GetAllAsync(
        PaginationQuery query, CancellationToken ct = default)
    {
        var (items, total) = await _uow.Courses.GetPagedAsync(
            pageNumber: query.PageNumber,
            pageSize: query.PageSize,
            predicate: string.IsNullOrWhiteSpace(query.Search) ? null :
                        c => c.Name.Contains(query.Search) ||
                             c.Code.Contains(query.Search) ||
                             (c.Subject != null && c.Subject.Contains(query.Search)),
            orderBy: c => c.Name,
            ct: ct);

        var dtos = _mapper.Map<List<CourseDto>>(items);

        foreach (var dto in dtos)
        {
            dto.ActiveClassCount = await _uow.Classes.CountAsync(
                c => c.CourseId == dto.Id && c.Status == ClassStatus.Scheduled, ct);

            var tutorCourses = await _uow.TutorCourses.FindAsync(
                tc => tc.CourseId == dto.Id, ct);
            var tutorIds = tutorCourses.Select(tc => tc.TutorId).ToList();
            var tutors = await _uow.Tutors.FindAsync(t => tutorIds.Contains(t.Id), ct);
            dto.AssignedTutors = tutors.Select(t => t.FullName).ToList();
        }

        return PagedResult<CourseDto>.Create(dtos, total, query.PageNumber, query.PageSize);
    }

    public async Task<CourseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var course = await _uow.Courses.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Course), id);

        var dto = _mapper.Map<CourseDto>(course);

        dto.ActiveClassCount = await _uow.Classes.CountAsync(
            c => c.CourseId == id && c.Status == ClassStatus.Scheduled, ct);

        // Count enrolled students across all classes for this course
        var classIds = (await _uow.Classes.FindAsync(c => c.CourseId == id, ct))
            .Select(c => c.Id).ToList();
        dto.TotalEnrolledStudents = await _uow.StudentClasses.CountAsync(
            sc => classIds.Contains(sc.ClassId) && sc.Status == EnrollmentStatus.Active, ct);

        var tutorCourses = await _uow.TutorCourses.FindAsync(tc => tc.CourseId == id, ct);
        var tutorIds = tutorCourses.Select(tc => tc.TutorId).ToList();
        var tutors = await _uow.Tutors.FindAsync(t => tutorIds.Contains(t.Id), ct);
        dto.AssignedTutors = tutors.Select(t => t.FullName).ToList();

        return dto;
    }

    public async Task<CourseDto> CreateAsync(
        CreateCourseRequest request, CancellationToken ct = default)
    {
        if (await _uow.Courses.ExistsAsync(c => c.Code == request.Code.ToUpperInvariant(), ct))
            throw new DomainException($"Course code '{request.Code}' already exists.");

        var course = new Course
        {
            Code = request.Code.ToUpperInvariant().Trim(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Subject = request.Subject?.Trim(),
            GradeLevel = request.GradeLevel?.Trim(),
            MaxCapacity = request.MaxCapacity,
            DurationWeeks = request.DurationWeeks,
            FeeAmount = request.FeeAmount,
            FeeCurrency = request.FeeCurrency.ToUpperInvariant(),
            FeeDescription = request.FeeDescription,
            Status = CourseStatus.Active
        };

        await _uow.Courses.AddAsync(course, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<CourseDto>(course);
    }

    public async Task<CourseDto> UpdateAsync(
        Guid id, UpdateCourseRequest request, CancellationToken ct = default)
    {
        var course = await _uow.Courses.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Course), id);

        course.Name = request.Name.Trim();
        course.Description = request.Description.Trim();
        course.Subject = request.Subject?.Trim();
        course.GradeLevel = request.GradeLevel?.Trim();
        course.MaxCapacity = request.MaxCapacity;
        course.DurationWeeks = request.DurationWeeks;
        course.FeeAmount = request.FeeAmount;
        course.FeeCurrency = request.FeeCurrency.ToUpperInvariant();
        course.FeeDescription = request.FeeDescription;
        course.Status = request.Status;
        course.ThumbnailUrl = request.ThumbnailUrl;

        await _uow.Courses.UpdateAsync(course, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<CourseDto>(course);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var course = await _uow.Courses.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Course), id);
        await _uow.Courses.DeleteAsync(course, ct);
        await _uow.SaveChangesAsync(ct);
    }
}

// ── Class Service ─────────────────────────────────────────────────────────────

public class ClassService : IClassService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ClassService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<ClassDto>> GetAllAsync(
        PaginationQuery query, CancellationToken ct = default)
    {
        var (items, total) = await _uow.Classes.GetPagedAsync(
            pageNumber: query.PageNumber,
            pageSize: query.PageSize,
            predicate: string.IsNullOrWhiteSpace(query.Search) ? null :
                        c => c.Name.Contains(query.Search),
            orderBy: c => c.StartDate,
            descending: true,
            ct: ct);

        return PagedResult<ClassDto>.Create(
            _mapper.Map<List<ClassDto>>(items), total, query.PageNumber, query.PageSize);
    }

    public async Task<ClassDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var cls = await _uow.Classes.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Class), id);
        return _mapper.Map<ClassDto>(cls);
    }

    public async Task<ClassDto> CreateAsync(
        CreateClassRequest request, CancellationToken ct = default)
    {
        _ = await _uow.Courses.GetByIdAsync(request.CourseId, ct)
            ?? throw new NotFoundException(nameof(Course), request.CourseId);
        _ = await _uow.Tutors.GetByIdAsync(request.TutorId, ct)
            ?? throw new NotFoundException(nameof(Tutor), request.TutorId);

        if (request.EndTime <= request.StartTime)
            throw new DomainException("End time must be after start time.");

        var cls = new Class
        {
            Name = request.Name.Trim(),
            CourseId = request.CourseId,
            TutorId = request.TutorId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Room = request.Room?.Trim(),
            Location = request.Location?.Trim(),
            MaxCapacity = request.MaxCapacity,
            Notes = request.Notes,
            Status = ClassStatus.Scheduled
        };

        await _uow.Classes.AddAsync(cls, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<ClassDto>(cls);
    }

    public async Task<ClassDto> UpdateAsync(
        Guid id, UpdateClassRequest request, CancellationToken ct = default)
    {
        var cls = await _uow.Classes.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Class), id);

        _ = await _uow.Tutors.GetByIdAsync(request.TutorId, ct)
            ?? throw new NotFoundException(nameof(Tutor), request.TutorId);

        cls.Name = request.Name.Trim();
        cls.TutorId = request.TutorId;
        cls.DayOfWeek = request.DayOfWeek;
        cls.StartTime = request.StartTime;
        cls.EndTime = request.EndTime;
        cls.StartDate = request.StartDate;
        cls.EndDate = request.EndDate;
        cls.Room = request.Room?.Trim();
        cls.Location = request.Location?.Trim();
        cls.MaxCapacity = request.MaxCapacity;
        cls.Status = request.Status;
        cls.Notes = request.Notes;

        await _uow.Classes.UpdateAsync(cls, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<ClassDto>(cls);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var cls = await _uow.Classes.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Class), id);
        await _uow.Classes.DeleteAsync(cls, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<List<StudentSummaryDto>> GetEnrolledStudentsAsync(
        Guid classId, CancellationToken ct = default)
    {
        var enrollments = await _uow.StudentClasses.FindAsync(
            sc => sc.ClassId == classId && sc.Status == EnrollmentStatus.Active, ct);

        var studentIds = enrollments.Select(e => e.StudentId).ToList();
        var students = await _uow.Students.FindAsync(
            s => studentIds.Contains(s.Id), ct);

        return students.Select(s => new StudentSummaryDto
        {
            Id = s.Id,
            StudentNumber = s.StudentNumber,
            FullName = s.FullName,
            Email = s.Email,
            PhoneNumber = s.PhoneNumber,
            IsActive = s.IsActive,
            ProfilePhotoUrl = s.ProfilePhotoUrl
        }).ToList();
    }
}