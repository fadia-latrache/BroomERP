using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BroomErp.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class FinanceController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly ICashCloseService _cashService;

    public FinanceController(IInvoiceService invoiceService, ICashCloseService cashService)
    {
        _invoiceService = invoiceService;
        _cashService = cashService;
    }

    [HttpGet("invoices")]
    public async Task<ActionResult<List<InvoiceDto>>> GetInvoices([FromQuery] QueryParams query)
    {
        var result = await _invoiceService.GetInvoicesAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpGet("invoices/{id:int}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoiceById(int id)
    {
        var invoice = await _invoiceService.GetByIdAsync(id);
        return Ok(invoice);
    }

    [HttpPost("jobs/{jobId:int}/invoice")]
    public async Task<ActionResult<InvoiceDto>> CreateForJob(int jobId, [FromBody] CreateInvoiceRequest request)
    {
        var invoice = await _invoiceService.CreateForJobAsync(jobId, request);
        return Created($"/api/invoices/{invoice.Id}", invoice);
    }

    [HttpPost("invoices/{id:int}/issue")]
    public async Task<ActionResult<InvoiceDto>> Issue(int id)
    {
        var invoice = await _invoiceService.IssueAsync(id);
        return Ok(invoice);
    }

    [HttpPost("invoices/{id:int}/payments")]
    public async Task<ActionResult<PaymentDto>> RecordPayment(int id, [FromBody] CreatePaymentRequest request)
    {
        var payment = await _invoiceService.RecordPaymentAsync(id, request);
        return Ok(payment);
    }

    [HttpGet("receipts")]
    public async Task<ActionResult<List<PaymentDto>>> GetReceipts([FromQuery] QueryParams query)
    {
        var result = await _invoiceService.GetPaymentsAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpGet("cash-closes")]
    public async Task<ActionResult<List<CashCloseDto>>> GetCashCloses([FromQuery] QueryParams query)
    {
        var result = await _cashService.GetCashClosesAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpPost("cash-closes")]
    public async Task<ActionResult<CashCloseDto>> CreateCashClose([FromBody] CreateCashCloseRequest request)
    {
        var close = await _cashService.CreateCloseAsync(request);
        return Created($"/api/cash-closes/{close.Id}", close);
    }

    [HttpPost("cash-closes/{id:int}/approve")]
    [Authorize(Roles = "GM")]
    public async Task<IActionResult> ApproveCashClose(int id)
    {
        await _cashService.ApproveCloseAsync(id);
        return NoContent();
    }
}
