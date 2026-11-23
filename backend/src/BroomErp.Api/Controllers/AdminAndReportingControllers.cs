using BroomErp.Application.Common.Interfaces;
using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BroomErp.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("approvals")]
    public async Task<ActionResult<List<ApprovalDto>>> GetApprovals([FromQuery] QueryParams query)
    {
        var result = await _adminService.GetApprovalsAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpPost("approvals/{id:int}/decide")]
    [Authorize(Roles = "GM")]
    public async Task<IActionResult> DecideApproval(int id, [FromBody] DecideApprovalRequest request)
    {
        await _adminService.DecideApprovalAsync(id, request);
        return NoContent();
    }

    [HttpGet("audit")]
    [Authorize(Roles = "GM")]
    public async Task<ActionResult<List<AuditLogDto>>> GetAuditLogs([FromQuery] QueryParams query)
    {
        var result = await _adminService.GetAuditLogsAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpGet("audit-changes")]
    [Authorize(Roles = "GM")]
    public async Task<ActionResult<List<AuditChangeDto>>> GetAuditChanges([FromQuery] QueryParams query)
    {
        var result = await _adminService.GetAuditChangesAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpGet("service-types")]
    public async Task<ActionResult<List<ServiceTypeDto>>> GetServiceTypes()
    {
        var list = await _adminService.GetServiceTypesAsync();
        return Ok(list);
    }

    [HttpPost("service-types")]
    [Authorize(Roles = "GM")]
    public async Task<ActionResult<ServiceTypeDto>> CreateServiceType([FromBody] CreateServiceTypeRequest request)
    {
        var st = await _adminService.CreateServiceTypeAsync(request);
        return Created($"/api/service-types/{st.Id}", st);
    }

    [HttpGet("company")]
    public async Task<ActionResult<CompanyProfileDto>> GetCompanyProfile()
    {
        var cp = await _adminService.GetCompanyProfileAsync();
        return Ok(cp);
    }

    [HttpPatch("company")]
    [Authorize(Roles = "GM")]
    public async Task<ActionResult<CompanyProfileDto>> UpdateCompanyProfile([FromBody] UpdateCompanyProfileRequest request)
    {
        var cp = await _adminService.UpdateCompanyProfileAsync(request);
        return Ok(cp);
    }
}

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        return Ok(summary);
    }
}

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ReportSummaryDto>> GetSummary()
    {
        var summary = await _reportService.GetFinancialSummaryAsync();
        return Ok(summary);
    }
}

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IPdfGenerator _pdfGenerator;

    public DocumentsController(IPdfGenerator pdfGenerator)
    {
        _pdfGenerator = pdfGenerator;
    }

    [HttpGet("quotation/{id:int}/pdf")]
    public async Task<IActionResult> GetQuotationPdf(int id)
    {
        var pdf = await _pdfGenerator.GenerateQuotationPdfAsync(id);
        return File(pdf, "application/pdf", $"Quotation-{id}.pdf");
    }

    [HttpGet("invoice/{id:int}/pdf")]
    public async Task<IActionResult> GetInvoicePdf(int id)
    {
        var pdf = await _pdfGenerator.GenerateInvoicePdfAsync(id);
        return File(pdf, "application/pdf", $"Invoice-{id}.pdf");
    }
}
