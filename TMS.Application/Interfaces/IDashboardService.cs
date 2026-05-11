using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Application.DTOs.Common;

namespace TMS.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetAdminDashboardAsync(CancellationToken ct = default);
        Task<DashboardDto> GetTutorDashboardAsync(string tutorUserId, CancellationToken ct = default);
        Task<DashboardDto> GetStudentDashboardAsync(string studentUserId, CancellationToken ct = default);
    }
}
