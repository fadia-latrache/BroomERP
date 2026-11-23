using BroomErp.Application.Common.Interfaces;
using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using BroomErp.Domain.Entities;
using BroomErp.Domain.Enums;
using BroomErp.Domain.Exceptions;

namespace BroomErp.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IRepository<Invoice> _invoiceRepo;
    private readonly IRepository<Job> _jobRepo;
    private readonly IRepository<Payment> _paymentRepo;
    private readonly IUnitOfWork _uow;
    private readonly ISequenceGenerator _sequences;
    private readonly ICurrentUserContext _userContext;

    public InvoiceService(
        IRepository<Invoice> invoiceRepo,
        IRepository<Job> jobRepo,
        IRepository<Payment> paymentRepo,
        IUnitOfWork uow,
        ISequenceGenerator sequences,
        ICurrentUserContext userContext)
    {
        _invoiceRepo = invoiceRepo;
        _jobRepo = jobRepo;
        _paymentRepo = paymentRepo;
        _uow = uow;
        _sequences = sequences;
        _userContext = userContext;
    }

    public async Task<PagedResult<InvoiceDto>> GetInvoicesAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _invoiceRepo.GetPagedAsync(query,
            i => (string.IsNullOrEmpty(query.Search) || i.Number.Contains(query.Search) || (i.Customer != null && i.Customer.Name.ToLower().Contains(query.Search.ToLower()))),
            cancellationToken,
            i => i.Customer!);

        return new PagedResult<InvoiceDto>
        {
            Items = paged.Items.Select(MapToDto).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<InvoiceDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var i = await _invoiceRepo.GetByIdAsync(id, cancellationToken, i => i.Customer!)
            ?? throw new DomainException("Invoice not found", 404);
        return MapToDto(i);
    }

    public async Task<InvoiceDto> CreateForJobAsync(int jobId, CreateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepo.GetByIdAsync(jobId, cancellationToken)
            ?? throw new DomainException("Job not found", 404);

        var existing = await _invoiceRepo.AnyAsync(i => i.JobId == jobId, cancellationToken);
        if (existing)
            throw new DomainException("Invoice already exists for this job");

        var number = await _sequences.NextAsync("INV-", cancellationToken);
        var inv = new Invoice
        {
            Number = number,
            CustomerId = job.CustomerId,
            JobId = job.Id,
            Total = job.Amount,
            DueDate = request.DueDate,
            Status = InvoiceStatus.DRAFT
        };

        await _invoiceRepo.AddAsync(inv, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        inv.Customer = job.Customer;
        return MapToDto(inv);
    }

    public async Task<InvoiceDto> IssueAsync(int id, CancellationToken cancellationToken = default)
    {
        var inv = await _invoiceRepo.GetByIdAsync(id, cancellationToken, i => i.Customer!)
            ?? throw new DomainException("Invoice not found", 404);

        if (inv.Status == InvoiceStatus.DRAFT)
        {
            inv.Status = InvoiceStatus.UNPAID;
            await _uow.SaveChangesAsync(cancellationToken);
        }
        return MapToDto(inv);
    }

    public async Task<PaymentDto> RecordPaymentAsync(int invoiceId, CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var inv = await _invoiceRepo.GetByIdAsync(invoiceId, cancellationToken)
            ?? throw new DomainException("Invoice not found", 404);

        if (request.Amount <= 0)
            throw new DomainException("Payment amount must be greater than zero");

        if (request.Amount > inv.Balance)
            throw new DomainException($"Payment amount ({request.Amount}) cannot exceed invoice balance ({inv.Balance})");

        var receiptNo = await _sequences.NextAsync("RCP-", cancellationToken);
        var payment = new Payment
        {
            ReceiptNo = receiptNo,
            InvoiceId = invoiceId,
            Amount = request.Amount,
            Method = request.Method,
            Reference = request.Reference,
            ReceivedBy = _userContext.Email ?? "finance"
        };

        await _paymentRepo.AddAsync(payment, cancellationToken);

        inv.Paid += request.Amount;
        if (inv.Paid >= inv.Total)
            inv.Status = InvoiceStatus.PAID;
        else if (inv.Paid > 0)
            inv.Status = InvoiceStatus.PARTIAL;

        await _uow.SaveChangesAsync(cancellationToken);

        return new PaymentDto(payment.Id, payment.ReceiptNo, payment.InvoiceId, payment.Amount, payment.Method, payment.Reference, payment.ReceivedBy, payment.CreatedAt);
    }

    public async Task RequestCancelAsync(int id, string reason, CancellationToken cancellationToken = default)
    {
        var inv = await _invoiceRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Invoice not found", 404);

        inv.CancelRequested = true;
        inv.CancelReason = reason;
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<PaymentDto>> GetPaymentsAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _paymentRepo.GetPagedAsync(query,
            p => (string.IsNullOrEmpty(query.Search) || p.ReceiptNo.Contains(query.Search) || p.Reference.Contains(query.Search)),
            cancellationToken);

        return new PagedResult<PaymentDto>
        {
            Items = paged.Items.Select(p => new PaymentDto(p.Id, p.ReceiptNo, p.InvoiceId, p.Amount, p.Method, p.Reference, p.ReceivedBy, p.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    private static InvoiceDto MapToDto(Invoice i)
    {
        return new InvoiceDto(
            i.Id,
            i.Number,
            i.CustomerId,
            i.CustomerName,
            i.JobId,
            i.Total,
            i.Paid,
            i.Balance,
            i.DueDate,
            i.Status,
            i.Overdue,
            i.CancelRequested,
            i.CancelReason,
            i.CreatedAt
        );
    }
}
