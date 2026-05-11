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
    public interface IStudentService
    {
        Task<PagedResult<StudentSummaryDto>> GetAllAsync(PaginationQuery query, CancellationToken ct = default);
        Task<StudentDto> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken ct = default);
        Task<StudentDto> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task EnrollInClassAsync(Guid studentId, Guid classId, CancellationToken ct = default);
        Task WithdrawFromClassAsync(Guid studentId, Guid classId, string reason, CancellationToken ct = default);
        Task<List<ClassDto>> GetStudentClassesAsync(Guid studentId, CancellationToken ct = default);
    }
}
