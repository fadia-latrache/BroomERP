using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BroomErp.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class StoreController : ControllerBase
{
    private readonly IStockService _stockService;
    private readonly IAssetService _assetService;

    public StoreController(IStockService stockService, IAssetService assetService)
    {
        _stockService = stockService;
        _assetService = assetService;
    }

    [HttpGet("stock")]
    public async Task<ActionResult<List<StockItemDto>>> GetStock([FromQuery] QueryParams query)
    {
        var result = await _stockService.GetStockItemsAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpGet("stock/{id:int}")]
    public async Task<ActionResult<StockItemDto>> GetStockById(int id)
    {
        var item = await _stockService.GetByIdAsync(id);
        return Ok(item);
    }

    [HttpPost("stock")]
    public async Task<ActionResult<StockItemDto>> CreateStock([FromBody] CreateStockItemRequest request)
    {
        var item = await _stockService.CreateAsync(request);
        return Created($"/api/stock/{item.Id}", item);
    }

    [HttpPatch("stock/{id:int}")]
    public async Task<ActionResult<StockItemDto>> UpdateStock(int id, [FromBody] UpdateStockItemRequest request)
    {
        var item = await _stockService.UpdateAsync(id, request);
        return Ok(item);
    }

    [HttpGet("stock/movements")]
    public async Task<ActionResult<List<StockMovementDto>>> GetMovements([FromQuery] QueryParams query)
    {
        var result = await _stockService.GetMovementsAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpPost("stock/{id:int}/movements")]
    public async Task<ActionResult<StockMovementDto>> RecordMovement(int id, [FromBody] CreateStockMovementRequest request)
    {
        var move = await _stockService.RecordMovementAsync(id, request);
        return Ok(move);
    }

    [HttpGet("stock-counts")]
    public async Task<ActionResult<List<StockCountDto>>> GetStockCounts([FromQuery] QueryParams query)
    {
        var result = await _stockService.GetStockCountsAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpPost("stock-counts")]
    public async Task<ActionResult<StockCountDto>> RecordCount([FromBody] CreateStockCountRequest request)
    {
        var count = await _stockService.RecordCountAsync(request);
        return Created($"/api/stock-counts/{count.Id}", count);
    }

    [HttpPost("stock-counts/{id:int}/apply")]
    public async Task<IActionResult> ApplyCount(int id)
    {
        await _stockService.ApplyCountAdjustmentAsync(id);
        return NoContent();
    }

    [HttpGet("assets")]
    public async Task<ActionResult<List<AssetDto>>> GetAssets([FromQuery] QueryParams query)
    {
        var result = await _assetService.GetAssetsAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpGet("assets/{id:int}")]
    public async Task<ActionResult<AssetDto>> GetAssetById(int id)
    {
        var asset = await _assetService.GetByIdAsync(id);
        return Ok(asset);
    }

    [HttpPost("assets")]
    public async Task<ActionResult<AssetDto>> CreateAsset([FromBody] CreateAssetRequest request)
    {
        var asset = await _assetService.CreateAsync(request);
        return Created($"/api/assets/{asset.Id}", asset);
    }

    [HttpPost("assets/{id:int}/issue")]
    public async Task<IActionResult> IssueAsset(int id, [FromBody] IssueAssetRequest request)
    {
        await _assetService.IssueAsync(id, request);
        return NoContent();
    }

    [HttpPost("assets/{id:int}/return")]
    public async Task<IActionResult> ReturnAsset(int id, [FromBody] ReturnAssetRequest request)
    {
        await _assetService.ReturnAsync(id, request);
        return NoContent();
    }

    [HttpGet("maintenance")]
    public async Task<ActionResult<List<MaintenanceRequestDto>>> GetMaintenance([FromQuery] QueryParams query)
    {
        var result = await _assetService.GetMaintenanceRequestsAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpPost("maintenance")]
    public async Task<ActionResult<MaintenanceRequestDto>> CreateMaintenance([FromBody] CreateMaintenanceRequest request)
    {
        var maint = await _assetService.CreateMaintenanceRequestAsync(request);
        return Created($"/api/maintenance/{maint.Id}", maint);
    }

    [HttpPost("maintenance/{id:int}/start")]
    public async Task<IActionResult> StartMaintenance(int id, [FromBody] StartMaintenanceRequest request)
    {
        await _assetService.StartMaintenanceAsync(id, request);
        return NoContent();
    }

    [HttpPost("maintenance/{id:int}/repair")]
    public async Task<IActionResult> CompleteRepair(int id, [FromBody] RepairMaintenanceRequest request)
    {
        await _assetService.CompleteRepairAsync(id, request);
        return NoContent();
    }

    [HttpGet("rentals")]
    public async Task<ActionResult<List<RentalDto>>> GetRentals([FromQuery] QueryParams query)
    {
        var result = await _assetService.GetRentalsAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpPost("rentals")]
    public async Task<ActionResult<RentalDto>> CreateRental([FromBody] CreateRentalRequest request)
    {
        var rental = await _assetService.CreateRentalAsync(request);
        return Created($"/api/rentals/{rental.Id}", rental);
    }

    [HttpPost("rentals/{id:int}/return")]
    public async Task<IActionResult> ReturnRental(int id, [FromBody] ReturnRentalRequest request)
    {
        await _assetService.ReturnRentalAsync(id, request);
        return NoContent();
    }

    [HttpPost("rentals/{id:int}/timesheets")]
    public async Task<ActionResult<RentalTimesheetDto>> RecordTimesheet(int id, [FromBody] CreateTimesheetRequest request)
    {
        var ts = await _assetService.RecordTimesheetAsync(id, request);
        return Ok(ts);
    }
}
