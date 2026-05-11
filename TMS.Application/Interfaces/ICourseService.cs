using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Application.Common;
using TMS.Application.DTOs.Course;

namespace TMS.Application.Interfaces
{
    public interface ICourseService
    {
        Task<PagedResult<CourseDto>> GetAllAsync(PaginationQuery query, CancellationToken ct = default);
        Task<CourseDto> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<CourseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct = default);
        Task<CourseDto> UpdateAsync(Guid id, UpdateCourseRequest request, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
