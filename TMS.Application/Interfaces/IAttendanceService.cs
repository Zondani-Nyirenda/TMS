using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Application.DTOs.Attendance;

namespace TMS.Application.Interfaces
{
    public interface IAttendanceService
    {
        Task SubmitAsync(SubmitAttendanceRequest request, string recordedBy, CancellationToken ct = default);
        Task<List<AttendanceDto>> GetByClassAndDateAsync(Guid classId, DateTime date, CancellationToken ct = default);
        Task<List<AttendanceSummaryDto>> GetSummaryAsync(Guid classId, DateTime from, DateTime to, CancellationToken ct = default);
        Task SyncOfflineRecordsAsync(List<OfflineAttendanceRecord> records, string recordedBy, CancellationToken ct = default);
    }
}
