using BroomErp.Application.Common.Interfaces;
using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using BroomErp.Domain.Entities;
using BroomErp.Domain.Exceptions;

namespace BroomErp.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _customerRepo;
    private readonly IRepository<CustomerAddress> _addressRepo;
    private readonly IRepository<Enquiry> _enquiryRepo;
    private readonly IRepository<Quotation> _quotationRepo;
    private readonly IRepository<Job> _jobRepo;
    private readonly IRepository<Invoice> _invoiceRepo;
    private readonly IRepository<Payment> _paymentRepo;
    private readonly IUnitOfWork _uow;
    private readonly ISequenceGenerator _sequences;

    public CustomerService(
        IRepository<Customer> customerRepo,
        IRepository<CustomerAddress> addressRepo,
        IRepository<Enquiry> enquiryRepo,
        IRepository<Quotation> quotationRepo,
        IRepository<Job> jobRepo,
        IRepository<Invoice> invoiceRepo,
        IRepository<Payment> paymentRepo,
        IUnitOfWork uow,
        ISequenceGenerator sequences)
    {
        _customerRepo = customerRepo;
        _addressRepo = addressRepo;
        _enquiryRepo = enquiryRepo;
        _quotationRepo = quotationRepo;
        _jobRepo = jobRepo;
        _invoiceRepo = invoiceRepo;
        _paymentRepo = paymentRepo;
        _uow = uow;
        _sequences = sequences;
    }

    public async Task<PagedResult<CustomerDto>> GetCustomersAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _customerRepo.GetPagedAsync(query,
            c => (string.IsNullOrEmpty(query.Search) || c.Name.ToLower().Contains(query.Search.ToLower()) || c.Phone.Contains(query.Search)),
            cancellationToken);

        return new PagedResult<CustomerDto>
        {
            Items = paged.Items.Select(c => new CustomerDto(c.Id, c.Number, c.Name, c.Phone, c.Email, c.Address, c.Notes, c.Active, c.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<CustomerDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var c = await _customerRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Customer not found", 404);
        return new CustomerDto(c.Id, c.Number, c.Name, c.Phone, c.Email, c.Address, c.Notes, c.Active, c.CreatedAt);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = new Customer
        {
            Number = await _sequences.NextAsync("CUST-", cancellationToken),
            Name = request.Name.Trim(),
            Phone = request.Phone.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Address = request.Address.Trim(),
            Notes = request.Notes.Trim(),
            Active = true
        };

        await _customerRepo.AddAsync(customer, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new CustomerDto(customer.Id, customer.Number, customer.Name, customer.Phone, customer.Email, customer.Address, customer.Notes, customer.Active, customer.CreatedAt);
    }

    public async Task<CustomerDto> UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var c = await _customerRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Customer not found", 404);

        if (request.Name != null) c.Name = request.Name.Trim();
        if (request.Phone != null) c.Phone = request.Phone.Trim();
        if (request.Email != null) c.Email = request.Email.Trim().ToLowerInvariant();
        if (request.Address != null) c.Address = request.Address.Trim();
        if (request.Notes != null) c.Notes = request.Notes.Trim();
        if (request.Active.HasValue) c.Active = request.Active.Value;

        await _uow.SaveChangesAsync(cancellationToken);
        return new CustomerDto(c.Id, c.Number, c.Name, c.Phone, c.Email, c.Address, c.Notes, c.Active, c.CreatedAt);
    }

    public async Task ToggleActiveAsync(int id, bool active, CancellationToken cancellationToken = default)
    {
        var c = await _customerRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Customer not found", 404);
        c.Active = active;
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<CustomerHistoryDto> GetHistoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var addresses = await _addressRepo.ListAsync(a => a.CustomerId == id, cancellationToken);
        var enquiries = await _enquiryRepo.ListAsync(e => e.CustomerId == id, cancellationToken);
        var quotations = await _quotationRepo.ListAsync(q => q.CustomerId == id, cancellationToken, q => q.Lines, q => q.Customer!);
        var jobs = await _jobRepo.ListAsync(j => j.CustomerId == id, cancellationToken, j => j.Assignments);
        var invoices = await _invoiceRepo.ListAsync(i => i.CustomerId == id, cancellationToken);
        var invoiceIds = invoices.Select(i => i.Id).ToList();
        var payments = await _paymentRepo.ListAsync(p => invoiceIds.Contains(p.InvoiceId), cancellationToken);

        return new CustomerHistoryDto(
            addresses.Select(a => new AddressDto(a.Id, a.CustomerId, a.Label, a.Address)).ToList(),
            enquiries.Select(e => new EnquiryDto(e.Id, e.Number, e.CustomerId, e.CustomerName, e.ServiceType, e.Address, e.RequestedDate, e.Notes, e.Status, e.CreatedBy, e.CreatedAt)).ToList(),
            quotations.Select(q => new QuotationDto(
                q.Id, q.Number, q.CustomerId, q.CustomerName, q.Subtotal, q.DiscountPct, q.Total,
                q.Status, q.PaymentTerms, q.ServiceAddress, q.Basis, q.Notes, q.CreatedBy,
                q.ApprovedBy, q.CancelRequested, q.CancelReason,
                q.Lines.Select(l => new QuotationLineDto(l.Id, l.Description, l.Qty, l.UnitPrice, l.Days, l.Location)).ToList(),
                q.CreatedAt
            )).ToList(),
            jobs.Select(j => new JobDto(
                j.Id, j.Number, j.CustomerId, j.CustomerName, j.QuotationId, j.ServiceType,
                j.Address, j.ScheduledFor, j.StartTime, j.DurationHours, j.Notes, j.ProblemNotes,
                j.EndDate, j.TeamId, j.CancelRequested, j.CancelReason, j.Amount, j.Status,
                j.StaffCount, j.Delayed, j.CreatedAt
            )).ToList(),
            invoices.Select(i => new InvoiceDto(
                i.Id, i.Number, i.CustomerId, i.CustomerName, i.JobId, i.Total, i.Paid,
                i.Balance, i.DueDate, i.Status, i.Overdue, i.CancelRequested, i.CancelReason, i.CreatedAt
            )).ToList(),
            payments.Select(p => new PaymentDto(p.Id, p.ReceiptNo, p.InvoiceId, p.Amount, p.Method, p.Reference, p.ReceivedBy, p.CreatedAt)).ToList()
        );
    }

    public async Task<List<AddressDto>> GetAddressesAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var list = await _addressRepo.ListAsync(a => a.CustomerId == customerId, cancellationToken);
        return list.Select(a => new AddressDto(a.Id, a.CustomerId, a.Label, a.Address)).ToList();
    }

    public async Task<AddressDto> AddAddressAsync(int customerId, CreateAddressRequest request, CancellationToken cancellationToken = default)
    {
        var address = new CustomerAddress
        {
            CustomerId = customerId,
            Label = request.Label.Trim(),
            Address = request.Address.Trim()
        };
        await _addressRepo.AddAsync(address, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return new AddressDto(address.Id, address.CustomerId, address.Label, address.Address);
    }
}
