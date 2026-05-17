using TMS.Application.DTOs.Common;
using TMS.Application.Interfaces;
using TMS.Domain.Enums;
using TMS.Domain.Interfaces;

namespace TMS.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _uow;

    public DashboardService(IUnitOfWork uow) => _uow = uow;

    // ── Admin dashboard ───────────────────────────────────────────────────────

    public async Task<DashboardDto> GetAdminDashboardAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var thisMonth = new DateTime(now.Year, now.Month, 1);
        var thisYear = new DateTime(now.Year, 1, 1);

        // Counts
        var totalStudents = await _uow.Students.CountAsync(ct: ct);
        var activeStudents = await _uow.Students.CountAsync(s => s.IsActive, ct);
        var totalTutors = await _uow.Tutors.CountAsync(ct: ct);
        var activeTutors = await _uow.Tutors.CountAsync(t => t.IsActive, ct);
        var totalCourses = await _uow.Courses.CountAsync(ct: ct);
        var activeClasses = await _uow.Classes.CountAsync(
            c => c.Status == ClassStatus.Scheduled, ct);

        // Finance
        var allInvoices = await _uow.Invoices.GetAllAsync(ct);
        var allPayments = await _uow.Payments.GetAllAsync(ct);

        var revenueThisMonth = allPayments
            .Where(p => p.PaymentDate >= thisMonth)
            .Sum(p => p.Amount);

        var revenueThisYear = allPayments
            .Where(p => p.PaymentDate >= thisYear)
            .Sum(p => p.Amount);

        var outstandingBalance = allInvoices
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
            .Sum(i => i.BalanceAmount);

        var overdueBalance = allInvoices
            .Where(i => i.DueDate < now && i.Status != InvoiceStatus.Paid)
            .Sum(i => i.BalanceAmount);

        // Attendance
        var totalAttendances = await _uow.Attendances.CountAsync(ct: ct);
        var presentAttendances = await _uow.Attendances.CountAsync(
            a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late, ct);
        var attendancePct = totalAttendances == 0 ? 0 :
            presentAttendances * 100.0 / totalAttendances;

        // Upcoming classes (next 7 days)
        var upcomingClasses = (await _uow.Classes.FindAsync(
            c => c.Status == ClassStatus.Scheduled &&
                 c.StartDate <= now.AddDays(7), ct))
            .Take(5)
            .Select(c => new UpcomingClassDto
            {
                ClassId = c.Id,
                ClassName = c.Name,
                CourseName = c.Course?.Name ?? string.Empty,
                TutorName = c.Tutor?.FullName ?? string.Empty,
                NextSessionDate = c.StartDate,
                StartTime = c.StartTime,
                Room = c.Room,
                EnrolledCount = c.EnrolledCount
            }).ToList();

        // Overdue invoices
        var overdueInvoices = allInvoices
            .Where(i => i.DueDate < now &&
                        i.Status != InvoiceStatus.Paid &&
                        i.Status != InvoiceStatus.Cancelled)
            .OrderByDescending(i => i.DueDate)
            .Take(10)
            .Select(i => new OverdueInvoiceDto
            {
                InvoiceId = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                StudentName = i.Student?.FullName ?? string.Empty,
                BalanceAmount = i.BalanceAmount,
                DueDate = i.DueDate,
                DaysOverdue = (int)(now - i.DueDate).TotalDays
            }).ToList();

        // Monthly enrollments (last 6 months)
        var sixMonthsAgo = now.AddMonths(-6);
        var enrollments = await _uow.StudentClasses.FindAsync(
            sc => sc.CreatedAt >= sixMonthsAgo, ct);

        var monthlyEnrollments = enrollments
            .GroupBy(e => new { e.CreatedAt.Year, e.CreatedAt.Month })
            .Select(g => new MonthlyEnrollmentDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"),
                NewEnrollments = g.Count(e => e.Status == EnrollmentStatus.Active),
                Withdrawals = g.Count(e => e.Status == EnrollmentStatus.Withdrawn)
            })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToList();

        return new DashboardDto
        {
            TotalStudents = totalStudents,
            ActiveStudents = activeStudents,
            TotalTutors = totalTutors,
            ActiveTutors = activeTutors,
            TotalCourses = totalCourses,
            ActiveClasses = activeClasses,
            TotalRevenueThisMonth = revenueThisMonth,
            TotalRevenueThisYear = revenueThisYear,
            OutstandingBalance = outstandingBalance,
            OverdueBalance = overdueBalance,
            OverallAttendancePercentage = attendancePct,
            UpcomingClasses = upcomingClasses,
            OverdueInvoices = overdueInvoices,
            MonthlyEnrollments = monthlyEnrollments
        };
    }

    // ── Tutor dashboard ───────────────────────────────────────────────────────

    public async Task<DashboardDto> GetTutorDashboardAsync(
        string tutorUserId, CancellationToken ct = default)
    {
        var tutor = await _uow.Tutors.FirstOrDefaultAsync(
            t => t.UserId == tutorUserId, ct);

        if (tutor is null)
            return new DashboardDto();

        var myClasses = await _uow.Classes.FindAsync(
            c => c.TutorId == tutor.Id && c.Status == ClassStatus.Scheduled, ct);

        var classIds = myClasses.Select(c => c.Id).ToList();

        var enrolledCount = await _uow.StudentClasses.CountAsync(
            sc => classIds.Contains(sc.ClassId) && sc.Status == EnrollmentStatus.Active, ct);

        var upcomingClasses = myClasses
            .Take(5)
            .Select(c => new UpcomingClassDto
            {
                ClassId = c.Id,
                ClassName = c.Name,
                CourseName = c.Course?.Name ?? string.Empty,
                TutorName = tutor.FullName,
                NextSessionDate = c.StartDate,
                StartTime = c.StartTime,
                Room = c.Room,
                EnrolledCount = c.EnrolledCount
            }).ToList();

        return new DashboardDto
        {
            ActiveClasses = myClasses.Count,
            ActiveStudents = enrolledCount,
            UpcomingClasses = upcomingClasses
        };
    }

    // ── Student dashboard ─────────────────────────────────────────────────────

    public async Task<DashboardDto> GetStudentDashboardAsync(
        string studentUserId, CancellationToken ct = default)
    {
        var student = await _uow.Students.FirstOrDefaultAsync(
            s => s.UserId == studentUserId, ct);

        if (student is null)
            return new DashboardDto();

        var enrollments = await _uow.StudentClasses.FindAsync(
            sc => sc.StudentId == student.Id && sc.Status == EnrollmentStatus.Active, ct);

        var classIds = enrollments.Select(e => e.ClassId).ToList();

        var invoices = await _uow.Invoices.FindAsync(
            i => i.StudentId == student.Id, ct);

        var outstanding = invoices
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
            .Sum(i => i.BalanceAmount);

        var overdue = invoices
            .Where(i => i.DueDate < DateTime.UtcNow && i.Status != InvoiceStatus.Paid)
            .Sum(i => i.BalanceAmount);

        var allAttendances = await _uow.Attendances.FindAsync(
            a => a.StudentId == student.Id, ct);

        var attendancePct = allAttendances.Count == 0 ? 0 :
            allAttendances.Count(a =>
                a.Status == AttendanceStatus.Present ||
                a.Status == AttendanceStatus.Late)
            * 100.0 / allAttendances.Count;

        return new DashboardDto
        {
            ActiveClasses = classIds.Count,
            OutstandingBalance = outstanding,
            OverdueBalance = overdue,
            OverallAttendancePercentage = attendancePct
        };
    }
}