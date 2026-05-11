using AutoMapper;
using TMS.Application.DTOs.Attendance;
using TMS.Application.Interfaces;
using TMS.Domain.Entities;
using TMS.Domain.Enums;
using TMS.Domain.Exceptions;
using TMS.Domain.Interfaces;

namespace TMS.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public AttendanceService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    // ── Submit (online) ───────────────────────────────────────────────────────

    public async Task SubmitAsync(
        SubmitAttendanceRequest request, string recordedBy, CancellationToken ct = default)
    {
        var cls = await _uow.Classes.GetByIdAsync(request.ClassId, ct)
            ?? throw new NotFoundException(nameof(Class), request.ClassId);

        var date = request.Date.Date;  // strip time component

        // Upsert — update existing record or insert new one
        foreach (var entry in request.Entries)
        {
            var existing = await _uow.Attendances.FirstOrDefaultAsync(
                a => a.ClassId == request.ClassId &&
                     a.StudentId == entry.StudentId &&
                     a.Date == date, ct);

            if (existing is not null)
            {
                existing.Status = entry.Status;
                existing.ArrivalTime = entry.ArrivalTime;
                existing.Remarks = entry.Remarks;
                existing.RecordedBy = recordedBy;
                existing.IsSynced = true;
                await _uow.Attendances.UpdateAsync(existing, ct);
            }
            else
            {
                var attendance = new Attendance
                {
                    StudentId = entry.StudentId,
                    ClassId = request.ClassId,
                    Date = date,
                    Status = entry.Status,
                    ArrivalTime = entry.ArrivalTime,
                    Remarks = entry.Remarks,
                    RecordedBy = recordedBy,
                    IsSynced = true
                };
                await _uow.Attendances.AddAsync(attendance, ct);
            }
        }

        await _uow.SaveChangesAsync(ct);
    }

    // ── Get by class + date ────────────────────────────────────────────────────

    public async Task<List<AttendanceDto>> GetByClassAndDateAsync(
        Guid classId, DateTime date, CancellationToken ct = default)
    {
        var records = await _uow.Attendances.FindAsync(
            a => a.ClassId == classId && a.Date == date.Date, ct);

        return _mapper.Map<List<AttendanceDto>>(records);
    }

    // ── Summary (report) ──────────────────────────────────────────────────────

    public async Task<List<AttendanceSummaryDto>> GetSummaryAsync(
        Guid classId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var records = await _uow.Attendances.FindAsync(
            a => a.ClassId == classId &&
                 a.Date >= from.Date &&
                 a.Date <= to.Date, ct);

        return records
            .GroupBy(a => a.StudentId)
            .Select(g => new AttendanceSummaryDto
            {
                StudentId = g.Key,
                StudentName = g.First().Student?.FullName ?? string.Empty,
                StudentNumber = g.First().Student?.StudentNumber ?? string.Empty,
                TotalSessions = g.Count(),
                Present = g.Count(a => a.Status == AttendanceStatus.Present),
                Absent = g.Count(a => a.Status == AttendanceStatus.Absent),
                Late = g.Count(a => a.Status == AttendanceStatus.Late),
                Excused = g.Count(a => a.Status == AttendanceStatus.Excused),
            })
            .ToList();
    }

    // ── Offline sync ──────────────────────────────────────────────────────────

    public async Task SyncOfflineRecordsAsync(
        List<OfflineAttendanceRecord> records, string recordedBy, CancellationToken ct = default)
    {
        // Convert each offline record into a standard submit request and process
        foreach (var record in records.Where(r => !r.IsSynced))
        {
            var submitRequest = new SubmitAttendanceRequest
            {
                ClassId = record.ClassId,
                Date = record.Date,
                Entries = record.Entries
            };
            await SubmitAsync(submitRequest, recordedBy, ct);
        }
    }
}
