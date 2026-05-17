using AutoMapper;
using Microsoft.AspNetCore.Identity;
using TMS.Application.Common;
using TMS.Application.DTOs.Tutor;
using TMS.Application.Interfaces;
using TMS.Domain.Entities;
using TMS.Domain.Enums;
using TMS.Domain.Exceptions;
using TMS.Domain.Interfaces;
using TMS.Domain.ValueObjects;

namespace TMS.Application.Services;

public class TutorService : ITutorService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly INumberGeneratorService _numbers;
    private readonly UserManager<ApplicationUser> _userManager;

    public TutorService(
        IUnitOfWork uow,
        IMapper mapper,
        INumberGeneratorService numbers,
        UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _mapper = mapper;
        _numbers = numbers;
        _userManager = userManager;
    }

    public async Task<PagedResult<TutorSummaryDto>> GetAllAsync(
        PaginationQuery query, CancellationToken ct = default)
    {
        var (items, total) = await _uow.Tutors.GetPagedAsync(
            pageNumber: query.PageNumber,
            pageSize: query.PageSize,
            predicate: string.IsNullOrWhiteSpace(query.Search) ? null :
                        t => t.FirstName.Contains(query.Search) ||
                             t.LastName.Contains(query.Search) ||
                             t.Email.Contains(query.Search) ||
                             t.Specialization.Contains(query.Search),
            orderBy: t => t.LastName,
            ct: ct);

        var dtos = _mapper.Map<List<TutorSummaryDto>>(items);

        // Enrich with active class count
        foreach (var dto in dtos)
        {
            dto.ActiveClassCount = await _uow.Classes.CountAsync(
                c => c.TutorId == dto.Id && c.Status == ClassStatus.Scheduled, ct);
        }

        return PagedResult<TutorSummaryDto>.Create(dtos, total, query.PageNumber, query.PageSize);
    }

    public async Task<TutorDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tutor = await _uow.Tutors.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Tutor), id);

        var dto = _mapper.Map<TutorDto>(tutor);

        var tutorCourses = await _uow.TutorCourses.FindAsync(tc => tc.TutorId == id, ct);
        var courseIds = tutorCourses.Select(tc => tc.CourseId).ToList();
        var courses = await _uow.Courses.FindAsync(c => courseIds.Contains(c.Id), ct);

        dto.AssignedCourses = courses.Select(c => c.Name).ToList();
        dto.ActiveClassCount = await _uow.Classes.CountAsync(
            c => c.TutorId == id && c.Status == ClassStatus.Scheduled, ct);

        return dto;
    }

    public async Task<TutorDto> CreateAsync(
        CreateTutorRequest request, CancellationToken ct = default)
    {
        if (await _uow.Tutors.ExistsAsync(t => t.Email == request.Email, ct))
            throw new DomainException($"A tutor with email '{request.Email}' already exists.");

        // Always create a login account for tutors
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = UserRole.Tutor,
            IsActive = true,
            EmailConfirmed = true
        };

        var identityResult = await _userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            var errors = identityResult.Errors.Select(e => e.Description);
            throw new DomainException(string.Join(" | ", errors));
        }

        await _userManager.AddToRoleAsync(user, UserRole.Tutor.ToString());

        var tutor = new Tutor
        {
            StaffNumber = await _numbers.GenerateTutorStaffNumberAsync(ct),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber.Trim(),
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Qualification = request.Qualification.Trim(),
            Specialization = request.Specialization.Trim(),
            Bio = request.Bio,
            AvailabilityNotes = request.AvailabilityNotes,
            JoinDate = DateTime.UtcNow,
            IsActive = true,
            UserId = user.Id
        };

        if (!string.IsNullOrWhiteSpace(request.Street) && !string.IsNullOrWhiteSpace(request.City))
        {
            tutor.Address = new Address(
                request.Street!,
                request.City!,
                request.Province ?? string.Empty,
                request.PostalCode ?? string.Empty,
                request.Country ?? "Zambia");
        }

        await _uow.Tutors.AddAsync(tutor, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<TutorDto>(tutor);
    }

    public async Task<TutorDto> UpdateAsync(
        Guid id, UpdateTutorRequest request, CancellationToken ct = default)
    {
        var tutor = await _uow.Tutors.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Tutor), id);

        if (tutor.Email != request.Email.ToLowerInvariant() &&
            await _uow.Tutors.ExistsAsync(t => t.Email == request.Email && t.Id != id, ct))
            throw new DomainException($"Email '{request.Email}' is already in use.");

        tutor.FirstName = request.FirstName.Trim();
        tutor.LastName = request.LastName.Trim();
        tutor.Email = request.Email.Trim().ToLowerInvariant();
        tutor.PhoneNumber = request.PhoneNumber.Trim();
        tutor.Qualification = request.Qualification.Trim();
        tutor.Specialization = request.Specialization.Trim();
        tutor.Bio = request.Bio;
        tutor.AvailabilityNotes = request.AvailabilityNotes;
        tutor.ProfilePhotoUrl = request.ProfilePhotoUrl;
        tutor.IsActive = request.IsActive;

        await _uow.Tutors.UpdateAsync(tutor, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<TutorDto>(tutor);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var tutor = await _uow.Tutors.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Tutor), id);
        await _uow.Tutors.DeleteAsync(tutor, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task AssignToCourseAsync(
        Guid tutorId, Guid courseId, CancellationToken ct = default)
    {
        _ = await _uow.Tutors.GetByIdAsync(tutorId, ct)
            ?? throw new NotFoundException(nameof(Tutor), tutorId);
        _ = await _uow.Courses.GetByIdAsync(courseId, ct)
            ?? throw new NotFoundException(nameof(Course), courseId);

        if (await _uow.TutorCourses.ExistsAsync(
                tc => tc.TutorId == tutorId && tc.CourseId == courseId, ct))
            throw new DomainException("Tutor is already assigned to this course.");

        await _uow.TutorCourses.AddAsync(new TutorCourse
        {
            TutorId = tutorId,
            CourseId = courseId,
            AssignedDate = DateTime.UtcNow,
            IsPrimary = true
        }, ct);

        await _uow.SaveChangesAsync(ct);
    }

    public async Task RemoveFromCourseAsync(
        Guid tutorId, Guid courseId, CancellationToken ct = default)
    {
        var assignment = await _uow.TutorCourses.FirstOrDefaultAsync(
            tc => tc.TutorId == tutorId && tc.CourseId == courseId, ct)
            ?? throw new NotFoundException("TutorCourse", $"{tutorId}/{courseId}");

        await _uow.TutorCourses.DeleteAsync(assignment, ct);
        await _uow.SaveChangesAsync(ct);
    }
}