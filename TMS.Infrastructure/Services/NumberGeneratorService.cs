using Microsoft.EntityFrameworkCore;
using TMS.Domain.Interfaces;
using TMS.Infrastructure.Persistence;

namespace TMS.Infrastructure.Services;

/// <summary>
/// Generates sequential, human-readable reference numbers.
/// Format examples: STU-2024-0001, INV-2024-0001, RCP-2024-0001
/// </summary>
public class NumberGeneratorService : INumberGeneratorService
{
    private readonly AppDbContext _context;

    public NumberGeneratorService(AppDbContext context) => _context = context;

    public async Task<string> GenerateStudentNumberAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Students
            .IgnoreQueryFilters()
            .CountAsync(s => s.CreatedAt.Year == year, ct);
        return $"STU-{year}-{(count + 1):D4}";
    }

    public async Task<string> GenerateTutorStaffNumberAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Tutors
            .IgnoreQueryFilters()
            .CountAsync(t => t.CreatedAt.Year == year, ct);
        return $"TUT-{year}-{(count + 1):D4}";
    }

    public async Task<string> GenerateInvoiceNumberAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Invoices
            .IgnoreQueryFilters()
            .CountAsync(i => i.CreatedAt.Year == year, ct);
        return $"INV-{year}-{(count + 1):D4}";
    }

    public async Task<string> GenerateReceiptNumberAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Payments
            .IgnoreQueryFilters()
            .CountAsync(p => p.CreatedAt.Year == year, ct);
        return $"RCP-{year}-{(count + 1):D4}";
    }
}
