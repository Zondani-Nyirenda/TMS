using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Application.Common;
using TMS.Application.DTOs.Tutor;

namespace TMS.Application.Interfaces
{
    public interface ITutorService
    {
        Task<PagedResult<TutorSummaryDto>> GetAllAsync(PaginationQuery query, CancellationToken ct = default);
        Task<TutorDto> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<TutorDto> CreateAsync(CreateTutorRequest request, CancellationToken ct = default);
        Task<TutorDto> UpdateAsync(Guid id, UpdateTutorRequest request, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task AssignToCourseAsync(Guid tutorId, Guid courseId, CancellationToken ct = default);
        Task RemoveFromCourseAsync(Guid tutorId, Guid courseId, CancellationToken ct = default);
    }
}
