using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BroomErp.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly IQuotationService _quotationService;
    private readonly IAdminService _adminService;

    public SalesController(IQuotationService quotationService, IAdminService adminService)
    {
        _quotationService = quotationService;
        _adminService = adminService;
    }

    [HttpGet("enquiries")]
    public async Task<ActionResult<List<EnquiryDto>>> GetEnquiries([FromQuery] QueryParams query)
    {
        var result = await _adminService.GetEnquiriesAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpPost("enquiries")]
    public async Task<ActionResult<EnquiryDto>> CreateEnquiry([FromBody] CreateEnquiryRequest request)
    {
        var enquiry = await _adminService.CreateEnquiryAsync(request);
        return Created($"/api/enquiries/{enquiry.Id}", enquiry);
    }

    [HttpPatch("enquiries/{id:int}/status")]
    public async Task<IActionResult> UpdateEnquiryStatus(int id, [FromBody] UpdateEnquiryStatusRequest request)
    {
        await _adminService.UpdateEnquiryStatusAsync(id, request);
        return NoContent();
    }

    [HttpGet("quotations")]
    public async Task<ActionResult<List<QuotationDto>>> GetQuotations([FromQuery] QueryParams query)
    {
        var result = await _quotationService.GetQuotationsAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpGet("quotations/{id:int}")]
    public async Task<ActionResult<QuotationDto>> GetQuotationById(int id)
    {
        var quotation = await _quotationService.GetByIdAsync(id);
        return Ok(quotation);
    }

    [HttpPost("quotations")]
    public async Task<ActionResult<QuotationDto>> CreateQuotation([FromBody] CreateQuotationRequest request)
    {
        var quotation = await _quotationService.CreateAsync(request);
        return Created($"/api/quotations/{quotation.Id}", quotation);
    }

    [HttpPatch("quotations/{id:int}")]
    public async Task<ActionResult<QuotationDto>> UpdateQuotation(int id, [FromBody] UpdateQuotationRequest request)
    {
        var quotation = await _quotationService.UpdateAsync(id, request);
        return Ok(quotation);
    }

    [HttpPost("quotations/{id:int}/request-approval")]
    public async Task<IActionResult> RequestApproval(int id)
    {
        await _quotationService.RequestApprovalAsync(id);
        return NoContent();
    }

    [HttpPost("quotations/{id:int}/approve")]
    [Authorize(Roles = "GM")]
    public async Task<IActionResult> Approve(int id)
    {
        await _quotationService.ApproveAsync(id);
        return NoContent();
    }

    [HttpPost("quotations/{id:int}/accept")]
    public async Task<ActionResult<JobDto>> Accept(int id, [FromBody] AcceptQuotationRequest request)
    {
        var job = await _quotationService.AcceptAsync(id, request);
        return Ok(job);
    }

    [HttpPost("quotations/{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] Dictionary<string, string> body)
    {
        var reason = body.GetValueOrDefault("reason", "Customer rejected quotation");
        await _quotationService.RejectAsync(id, reason);
        return NoContent();
    }

    [HttpPost("quotations/{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] Dictionary<string, string> body)
    {
        var reason = body.GetValueOrDefault("reason", "Cancelled");
        await _quotationService.CancelAsync(id, reason);
        return NoContent();
    }

    [HttpPost("quotations/voice")]
    public async Task<ActionResult<VoiceQuotationResponse>> VoiceQuotation([FromBody] VoiceQuotationRequest request)
    {
        var result = await _quotationService.ParseVoiceAsync(request);
        return Ok(result);
    }
}
