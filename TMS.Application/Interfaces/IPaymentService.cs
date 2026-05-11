using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Application.Common;
using TMS.Application.DTOs.Payment;

namespace TMS.Application.Interfaces
{

    public interface IPaymentService
    {
        Task<PagedResult<InvoiceDto>> GetInvoicesAsync(PaginationQuery query, Guid? studentId = null, CancellationToken ct = default);
        Task<InvoiceDto> GetInvoiceByIdAsync(Guid id, CancellationToken ct = default);
        Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, string createdByUserId, CancellationToken ct = default);
        Task<PaymentDto> RecordPaymentAsync(RecordPaymentRequest request, string processedByUserId, CancellationToken ct = default);
        Task<FinancialSummaryDto> GetFinancialSummaryAsync(int year, CancellationToken ct = default);
        Task<List<FeeStructureDto>> GetFeeStructuresAsync(Guid? courseId = null, CancellationToken ct = default);
        Task<FeeStructureDto> CreateFeeStructureAsync(CreateFeeStructureRequest request, CancellationToken ct = default);
    }
}
