using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;

namespace BroomErp.Application.Interfaces.Services;

public interface IQuotationService
{
    Task<PagedResult<QuotationDto>> GetQuotationsAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<QuotationDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<QuotationDto> CreateAsync(CreateQuotationRequest request, CancellationToken cancellationToken = default);
    Task<QuotationDto> UpdateAsync(int id, UpdateQuotationRequest request, CancellationToken cancellationToken = default);
    Task RequestApprovalAsync(int id, CancellationToken cancellationToken = default);
    Task ApproveAsync(int id, CancellationToken cancellationToken = default);
    Task<JobDto> AcceptAsync(int id, AcceptQuotationRequest request, CancellationToken cancellationToken = default);
    Task RejectAsync(int id, string reason, CancellationToken cancellationToken = default);
    Task CancelAsync(int id, string reason, CancellationToken cancellationToken = default);
    Task<VoiceQuotationResponse> ParseVoiceAsync(VoiceQuotationRequest request, CancellationToken cancellationToken = default);
}
