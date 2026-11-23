using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BroomErp.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class PurchasingController : ControllerBase
{
    private readonly IPurchasingService _purchasingService;

    public PurchasingController(IPurchasingService purchasingService)
    {
        _purchasingService = purchasingService;
    }

    [HttpGet("suppliers")]
    public async Task<ActionResult<List<SupplierDto>>> GetSuppliers([FromQuery] QueryParams query)
    {
        var result = await _purchasingService.GetSuppliersAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpPost("suppliers")]
    public async Task<ActionResult<SupplierDto>> CreateSupplier([FromBody] CreateSupplierRequest request)
    {
        var supplier = await _purchasingService.CreateSupplierAsync(request);
        return Created($"/api/suppliers/{supplier.Id}", supplier);
    }

    [HttpGet("requisitions")]
    public async Task<ActionResult<List<RequisitionDto>>> GetRequisitions([FromQuery] QueryParams query)
    {
        var result = await _purchasingService.GetRequisitionsAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpPost("requisitions")]
    public async Task<ActionResult<RequisitionDto>> CreateRequisition([FromBody] CreateRequisitionRequest request)
    {
        var requisition = await _purchasingService.CreateRequisitionAsync(request);
        return Created($"/api/requisitions/{requisition.Id}", requisition);
    }

    [HttpPost("requisitions/{id:int}/approve")]
    [Authorize(Roles = "GM")]
    public async Task<IActionResult> ApproveRequisition(int id)
    {
        await _purchasingService.ApproveRequisitionAsync(id);
        return NoContent();
    }

    [HttpGet("requisitions/{id:int}/quotations")]
    public async Task<ActionResult<List<SupplierQuotationDto>>> GetSupplierQuotations(int id)
    {
        var list = await _purchasingService.GetSupplierQuotationsAsync(id);
        return Ok(list);
    }

    [HttpPost("requisitions/{id:int}/quotations")]
    public async Task<ActionResult<SupplierQuotationDto>> AddSupplierQuotation(int id, [FromBody] CreateSupplierQuotationRequest request)
    {
        var sq = await _purchasingService.AddSupplierQuotationAsync(id, request);
        return Ok(sq);
    }

    [HttpPost("supplier-quotations/{id:int}/select")]
    public async Task<IActionResult> SelectSupplierQuotation(int id)
    {
        await _purchasingService.SelectSupplierQuotationAsync(id);
        return NoContent();
    }

    [HttpGet("lpos")]
    public async Task<ActionResult<List<LpoDto>>> GetLpos([FromQuery] QueryParams query)
    {
        var result = await _purchasingService.GetLposAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpPost("requisitions/{id:int}/lpo")]
    public async Task<ActionResult<LpoDto>> CreateLpo(int id, [FromBody] CreateLpoRequest request)
    {
        var lpo = await _purchasingService.CreateLpoAsync(id, request);
        return Created($"/api/lpos/{lpo.Id}", lpo);
    }

    [HttpPost("lpos/{id:int}/approve")]
    [Authorize(Roles = "GM")]
    public async Task<IActionResult> ApproveLpo(int id)
    {
        await _purchasingService.ApproveLpoAsync(id);
        return NoContent();
    }

    [HttpGet("supplier-invoices")]
    public async Task<ActionResult<List<SupplierInvoiceDto>>> GetSupplierInvoices([FromQuery] QueryParams query)
    {
        var result = await _purchasingService.GetSupplierInvoicesAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpPost("supplier-invoices")]
    public async Task<ActionResult<SupplierInvoiceDto>> RecordSupplierInvoice([FromBody] CreateSupplierInvoiceRequest request)
    {
        var sinv = await _purchasingService.RecordSupplierInvoiceAsync(request);
        return Created($"/api/supplier-invoices/{sinv.Id}", sinv);
    }

    [HttpGet("vouchers")]
    public async Task<ActionResult<List<VoucherDto>>> GetVouchers([FromQuery] QueryParams query)
    {
        var result = await _purchasingService.GetVouchersAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpPost("vouchers")]
    public async Task<ActionResult<VoucherDto>> CreateVoucher([FromBody] CreateVoucherRequest request)
    {
        var voucher = await _purchasingService.CreateVoucherAsync(request);
        return Created($"/api/vouchers/{voucher.Id}", voucher);
    }

    [HttpPost("vouchers/{id:int}/approve")]
    [Authorize(Roles = "GM")]
    public async Task<IActionResult> ApproveVoucher(int id)
    {
        await _purchasingService.ApproveVoucherAsync(id);
        return NoContent();
    }

    [HttpPost("vouchers/{id:int}/pay")]
    public async Task<IActionResult> PayVoucher(int id)
    {
        await _purchasingService.PayVoucherAsync(id);
        return NoContent();
    }
}
