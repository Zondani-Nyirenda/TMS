using AutoMapper;
using TMS.Application.Common;
using TMS.Application.DTOs.Exam;
using TMS.Application.Interfaces;
using TMS.Domain.Entities;
using TMS.Domain.Enums;
using TMS.Domain.Exceptions;
using TMS.Domain.Interfaces;

namespace TMS.Application.Services;

public class ExamService : IExamService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ExamService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<ExamDto>> GetAllAsync(
        PaginationQuery query, Guid? classId = null, CancellationToken ct = default)
    {
        var (items, total) = await _uow.Exams.GetPagedAsync(
            pageNumber: query.PageNumber,
            pageSize: query.PageSize,
            predicate: classId.HasValue ? e => e.ClassId == classId.Value : null,
            orderBy: e => e.ExamDate,
            descending: true,
            ct: ct);

        return PagedResult<ExamDto>.Create(
            _mapper.Map<List<ExamDto>>(items), total, query.PageNumber, query.PageSize);
    }

    public async Task<ExamDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var exam = await _uow.Exams.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Exam), id);

        var dto = _mapper.Map<ExamDto>(exam);
        var results = await _uow.Results.FindAsync(r => r.ExamId == id, ct);

        if (results.Any())
        {
            dto.AverageScore = (double)results.Average(r => r.MarksObtained / exam.TotalMarks * 100);
            dto.PassRate = results.Count(r => r.IsPassed) * 100.0 / results.Count;
        }
        return dto;
    }

    public async Task<ExamDto> CreateAsync(
        CreateExamRequest request, CancellationToken ct = default)
    {
        _ = await _uow.Classes.GetByIdAsync(request.ClassId, ct)
            ?? throw new NotFoundException(nameof(Class), request.ClassId);

        var exam = new Exam
        {
            Title = request.Title,
            Description = request.Description,
            ClassId = request.ClassId,
            Type = request.Type,
            ExamDate = request.ExamDate,
            TotalMarks = request.TotalMarks,
            PassMark = request.PassMark,
            Instructions = request.Instructions,
            IsPublished = false
        };

        await _uow.Exams.AddAsync(exam, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<ExamDto>(exam);
    }

    public async Task<ExamDto> UpdateAsync(
        Guid id, UpdateExamRequest request, CancellationToken ct = default)
    {
        var exam = await _uow.Exams.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Exam), id);

        exam.Title = request.Title;
        exam.Description = request.Description;
        exam.Type = request.Type;
        exam.ExamDate = request.ExamDate;
        exam.TotalMarks = request.TotalMarks;
        exam.PassMark = request.PassMark;
        exam.Instructions = request.Instructions;
        exam.IsPublished = request.IsPublished;

        await _uow.Exams.UpdateAsync(exam, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<ExamDto>(exam);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var exam = await _uow.Exams.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Exam), id);
        await _uow.Exams.DeleteAsync(exam, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task SubmitResultsAsync(
        SubmitResultsRequest request, string gradedByUserId, CancellationToken ct = default)
    {
        var exam = await _uow.Exams.GetByIdAsync(request.ExamId, ct)
            ?? throw new NotFoundException(nameof(Exam), request.ExamId);

        foreach (var entry in request.Entries)
        {
            if (entry.MarksObtained > exam.TotalMarks)
                throw new DomainException(
                    $"Marks {entry.MarksObtained} exceed total marks {exam.TotalMarks} for exam '{exam.Title}'.");

            var existing = await _uow.Results.FirstOrDefaultAsync(
                r => r.ExamId == request.ExamId && r.StudentId == entry.StudentId, ct);

            var grade = CalculateGrade(entry.MarksObtained, exam.TotalMarks);
            var isPassed = entry.MarksObtained >= exam.PassMark;

            if (existing is not null)
            {
                existing.MarksObtained = entry.MarksObtained;
                existing.Grade = grade;
                existing.IsPassed = isPassed;
                existing.Remarks = entry.Remarks;
                existing.GradedByUserId = gradedByUserId;
                existing.GradedAt = DateTime.UtcNow;
                await _uow.Results.UpdateAsync(existing, ct);
            }
            else
            {
                var result = new Result
                {
                    ExamId = request.ExamId,
                    StudentId = entry.StudentId,
                    MarksObtained = entry.MarksObtained,
                    Grade = grade,
                    IsPassed = isPassed,
                    Remarks = entry.Remarks,
                    GradedByUserId = gradedByUserId,
                    GradedAt = DateTime.UtcNow
                };
                await _uow.Results.AddAsync(result, ct);
            }
        }

        await _uow.SaveChangesAsync(ct);
    }

    public async Task<List<ResultDto>> GetResultsAsync(
        Guid examId, CancellationToken ct = default)
    {
        var results = await _uow.Results.FindAsync(r => r.ExamId == examId, ct);
        return _mapper.Map<List<ResultDto>>(results);
    }

    public async Task<StudentPerformanceDto> GetStudentPerformanceAsync(
        Guid studentId, Guid classId, CancellationToken ct = default)
    {
        var student = await _uow.Students.GetByIdAsync(studentId, ct)
            ?? throw new NotFoundException(nameof(Student), studentId);

        var exams = await _uow.Exams.FindAsync(e => e.ClassId == classId && e.IsPublished, ct);
        var examIds = exams.Select(e => e.Id).ToList();
        var results = await _uow.Results.FindAsync(
            r => r.StudentId == studentId && examIds.Contains(r.ExamId), ct);

        var summaries = results.Join(exams, r => r.ExamId, e => e.Id,
            (r, e) => new ExamResultSummary
            {
                ExamTitle = e.Title,
                ExamType = e.Type,
                ExamDate = e.ExamDate,
                Percentage = e.TotalMarks == 0 ? 0 : r.MarksObtained / e.TotalMarks * 100,
                Grade = r.Grade,
                IsPassed = r.IsPassed
            }).OrderBy(s => s.ExamDate).ToList();

        return new StudentPerformanceDto
        {
            StudentId = studentId,
            StudentName = student.FullName,
            ExamResults = summaries,
            OverallAverage = summaries.Any() ? (double)summaries.Average(s => s.Percentage) : 0,
            PassRate = summaries.Any() ? summaries.Count(s => s.IsPassed) * 100.0 / summaries.Count : 0,
            OverallGrade = summaries.Any()
                ? CalculateGradeFromPercentage((decimal)summaries.Average(s => s.Percentage))
                : GradeLevel.F
        };
    }

    public async Task PublishResultsAsync(Guid examId, CancellationToken ct = default)
    {
        var exam = await _uow.Exams.GetByIdAsync(examId, ct)
            ?? throw new NotFoundException(nameof(Exam), examId);
        exam.IsPublished = true;
        await _uow.Exams.UpdateAsync(exam, ct);
        await _uow.SaveChangesAsync(ct);
    }

    // ── Grade helpers ─────────────────────────────────────────────────────────

    private static GradeLevel CalculateGrade(decimal marks, decimal totalMarks)
    {
        var pct = totalMarks == 0 ? 0 : marks / totalMarks * 100;
        return CalculateGradeFromPercentage(pct);
    }

    private static GradeLevel CalculateGradeFromPercentage(decimal pct) => pct switch
    {
        >= 95 => GradeLevel.APlus,
        >= 90 => GradeLevel.A,
        >= 85 => GradeLevel.AMinus,
        >= 80 => GradeLevel.BPlus,
        >= 75 => GradeLevel.B,
        >= 70 => GradeLevel.BMinus,
        >= 65 => GradeLevel.CPlus,
        >= 60 => GradeLevel.C,
        >= 55 => GradeLevel.CMinus,
        >= 50 => GradeLevel.D,
        _ => GradeLevel.F
    };
}
