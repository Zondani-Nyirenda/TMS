using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Application.Common;
using TMS.Application.DTOs.Course;
using TMS.Application.DTOs.Student;

namespace TMS.Application.Interfaces
{
    public interface IClassService
    {
        Task<PagedResult<ClassDto>> GetAllAsync(PaginationQuery query, CancellationToken ct = default);
        Task<ClassDto> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<ClassDto> CreateAsync(CreateClassRequest request, CancellationToken ct = default);
        Task<ClassDto> UpdateAsync(Guid id, UpdateClassRequest request, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<List<StudentSummaryDto>> GetEnrolledStudentsAsync(Guid classId, CancellationToken ct = default);
    }
}
