using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BroomErp.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetAll([FromQuery] QueryParams query)
    {
        var result = await _customerService.GetCustomersAsync(query);
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return Ok(result.Items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerRequest request)
    {
        var customer = await _customerService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<CustomerDto>> Update(int id, [FromBody] UpdateCustomerRequest request)
    {
        var customer = await _customerService.UpdateAsync(id, request);
        return Ok(customer);
    }

    [HttpGet("{id:int}/history")]
    public async Task<ActionResult<CustomerHistoryDto>> GetHistory(int id)
    {
        var history = await _customerService.GetHistoryAsync(id);
        return Ok(history);
    }

    [HttpGet("{id:int}/addresses")]
    public async Task<ActionResult<List<AddressDto>>> GetAddresses(int id)
    {
        var list = await _customerService.GetAddressesAsync(id);
        return Ok(list);
    }

    [HttpPost("{id:int}/addresses")]
    public async Task<ActionResult<AddressDto>> AddAddress(int id, [FromBody] CreateAddressRequest request)
    {
        var address = await _customerService.AddAddressAsync(id, request);
        return Ok(address);
    }
}
