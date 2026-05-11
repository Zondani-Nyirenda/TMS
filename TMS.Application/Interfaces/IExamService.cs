using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Application.Common;
using TMS.Application.DTOs.Exam;

namespace TMS.Application.Interfaces
{
    public interface IExamService
    {
        Task<PagedResult<ExamDto>> GetAllAsync(PaginationQuery query, Guid? classId = null, CancellationToken ct = default);
        Task<ExamDto> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<ExamDto> CreateAsync(CreateExamRequest request, CancellationToken ct = default);
        Task<ExamDto> UpdateAsync(Guid id, UpdateExamRequest request, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task SubmitResultsAsync(SubmitResultsRequest request, string gradedByUserId, CancellationToken ct = default);
        Task<List<ResultDto>> GetResultsAsync(Guid examId, CancellationToken ct = default);
        Task<StudentPerformanceDto> GetStudentPerformanceAsync(Guid studentId, Guid classId, CancellationToken ct = default);
        Task PublishResultsAsync(Guid examId, CancellationToken ct = default);
    }
}
