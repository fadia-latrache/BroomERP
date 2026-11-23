using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;

namespace BroomErp.Application.Interfaces.Services;

public interface ICashCloseService
{
    Task<PagedResult<CashCloseDto>> GetCashClosesAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<CashCloseDto> CreateCloseAsync(CreateCashCloseRequest request, CancellationToken cancellationToken = default);
    Task ApproveCloseAsync(int id, CancellationToken cancellationToken = default);
}

public interface IAdminService
{
    Task<PagedResult<EnquiryDto>> GetEnquiriesAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<EnquiryDto> CreateEnquiryAsync(CreateEnquiryRequest request, CancellationToken cancellationToken = default);
    Task UpdateEnquiryStatusAsync(int id, UpdateEnquiryStatusRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<ApprovalDto>> GetApprovalsAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task DecideApprovalAsync(int id, DecideApprovalRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(QueryParams query, CancellationToken cancellationToken = default);
    Task<PagedResult<AuditChangeDto>> GetAuditChangesAsync(QueryParams query, CancellationToken cancellationToken = default);

    Task<List<ServiceTypeDto>> GetServiceTypesAsync(CancellationToken cancellationToken = default);
    Task<ServiceTypeDto> CreateServiceTypeAsync(CreateServiceTypeRequest request, CancellationToken cancellationToken = default);

    Task<CompanyProfileDto> GetCompanyProfileAsync(CancellationToken cancellationToken = default);
    Task<CompanyProfileDto> UpdateCompanyProfileAsync(UpdateCompanyProfileRequest request, CancellationToken cancellationToken = default);
}

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}

public interface IReportService
{
    Task<ReportSummaryDto> GetFinancialSummaryAsync(CancellationToken cancellationToken = default);
}
