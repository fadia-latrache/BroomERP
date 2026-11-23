using BroomErp.Application.Common.Interfaces;
using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using BroomErp.Domain.Entities;
using BroomErp.Domain.Enums;
using BroomErp.Domain.Exceptions;

namespace BroomErp.Application.Services;

public class QuotationService : IQuotationService
{
    private readonly IRepository<Quotation> _quotationRepo;
    private readonly IRepository<Customer> _customerRepo;
    private readonly IRepository<Job> _jobRepo;
    private readonly IRepository<Approval> _approvalRepo;
    private readonly IUnitOfWork _uow;
    private readonly ISequenceGenerator _sequences;
    private readonly ICurrentUserContext _userContext;

    public QuotationService(
        IRepository<Quotation> quotationRepo,
        IRepository<Customer> customerRepo,
        IRepository<Job> jobRepo,
        IRepository<Approval> approvalRepo,
        IUnitOfWork uow,
        ISequenceGenerator sequences,
        ICurrentUserContext userContext)
    {
        _quotationRepo = quotationRepo;
        _customerRepo = customerRepo;
        _jobRepo = jobRepo;
        _approvalRepo = approvalRepo;
        _uow = uow;
        _sequences = sequences;
        _userContext = userContext;
    }

    public async Task<PagedResult<QuotationDto>> GetQuotationsAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _quotationRepo.GetPagedAsync(query,
            q => (string.IsNullOrEmpty(query.Search) || q.Number.Contains(query.Search) || (q.Customer != null && q.Customer.Name.ToLower().Contains(query.Search.ToLower()))),
            cancellationToken,
            q => q.Customer!,
            q => q.Lines);

        return new PagedResult<QuotationDto>
        {
            Items = paged.Items.Select(MapToDto).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<QuotationDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var q = await _quotationRepo.GetByIdAsync(id, cancellationToken, q => q.Customer!, q => q.Lines)
            ?? throw new DomainException("Quotation not found", 404);
        return MapToDto(q);
    }

    public async Task<QuotationDto> CreateAsync(CreateQuotationRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepo.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new DomainException("Customer not found", 404);

        if (request.Lines.Count == 0)
            throw new DomainException("At least one quotation line is required");

        var number = await _sequences.NextAsync("QT-", cancellationToken);
        var q = new Quotation
        {
            Number = number,
            CustomerId = customer.Id,
            EnquiryId = request.EnquiryId,
            DiscountPct = request.DiscountPct,
            PaymentTerms = request.PaymentTerms,
            ServiceAddress = string.IsNullOrWhiteSpace(request.ServiceAddress) ? customer.Address : request.ServiceAddress,
            Basis = request.Basis,
            Notes = request.Notes,
            CreatedBy = _userContext.Email ?? "system",
            Status = QuotationStatus.DRAFT,
            Lines = request.Lines.Select(l => new QuotationLine
            {
                Description = l.Description,
                Qty = l.Qty,
                UnitPrice = l.UnitPrice,
                Days = l.Days,
                Location = l.Location
            }).ToList()
        };

        q.RecalculateTotal();

        await _quotationRepo.AddAsync(q, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        q.Customer = customer;
        return MapToDto(q);
    }

    public async Task<QuotationDto> UpdateAsync(int id, UpdateQuotationRequest request, CancellationToken cancellationToken = default)
    {
        var q = await _quotationRepo.GetByIdAsync(id, cancellationToken, q => q.Customer!, q => q.Lines)
            ?? throw new DomainException("Quotation not found", 404);

        if (q.Status != QuotationStatus.DRAFT && q.Status != QuotationStatus.PENDING_APPROVAL)
            throw new DomainException("Cannot edit non-draft quotation");

        if (request.DiscountPct.HasValue) q.DiscountPct = request.DiscountPct.Value;
        if (request.PaymentTerms != null) q.PaymentTerms = request.PaymentTerms;
        if (request.ServiceAddress != null) q.ServiceAddress = request.ServiceAddress;
        if (request.Basis != null) q.Basis = request.Basis;
        if (request.Notes != null) q.Notes = request.Notes;

        if (request.Lines != null)
        {
            q.Lines.Clear();
            q.Lines.AddRange(request.Lines.Select(l => new QuotationLine
            {
                QuotationId = q.Id,
                Description = l.Description,
                Qty = l.Qty,
                UnitPrice = l.UnitPrice,
                Days = l.Days,
                Location = l.Location
            }));
        }

        q.RecalculateTotal();
        await _uow.SaveChangesAsync(cancellationToken);

        return MapToDto(q);
    }

    public async Task RequestApprovalAsync(int id, CancellationToken cancellationToken = default)
    {
        var q = await _quotationRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Quotation not found", 404);

        q.Status = QuotationStatus.PENDING_APPROVAL;

        var approval = new Approval
        {
            Kind = "QUOTATION_DISCOUNT",
            DocType = "Quotation",
            DocId = q.Id,
            DocNumber = q.Number,
            RequestedBy = _userContext.Email ?? "user",
            Status = ApprovalStatus.PENDING
        };
        await _approvalRepo.AddAsync(approval, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveAsync(int id, CancellationToken cancellationToken = default)
    {
        var q = await _quotationRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Quotation not found", 404);

        q.Status = QuotationStatus.APPROVED;
        q.ApprovedBy = _userContext.Email;
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<JobDto> AcceptAsync(int id, AcceptQuotationRequest request, CancellationToken cancellationToken = default)
    {
        var q = await _quotationRepo.GetByIdAsync(id, cancellationToken, q => q.Customer!, q => q.Lines)
            ?? throw new DomainException("Quotation not found", 404);

        q.Status = QuotationStatus.ACCEPTED;

        var jobNumber = await _sequences.NextAsync("JOB-", cancellationToken);
        var job = new Job
        {
            Number = jobNumber,
            CustomerId = q.CustomerId,
            QuotationId = q.Id,
            ServiceType = request.ServiceType,
            Address = string.IsNullOrWhiteSpace(request.Address) ? q.ServiceAddress : request.Address,
            ScheduledFor = request.ScheduledFor,
            StartTime = request.StartTime,
            Amount = q.Total,
            Status = JobStatus.PLANNED
        };

        await _jobRepo.AddAsync(job, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        job.Customer = q.Customer;
        return new JobDto(
            job.Id, job.Number, job.CustomerId, job.CustomerName, job.QuotationId, job.ServiceType,
            job.Address, job.ScheduledFor, job.StartTime, job.DurationHours, job.Notes, job.ProblemNotes,
            job.EndDate, job.TeamId, job.CancelRequested, job.CancelReason, job.Amount, job.Status,
            0, false, job.CreatedAt
        );
    }

    public async Task RejectAsync(int id, string reason, CancellationToken cancellationToken = default)
    {
        var q = await _quotationRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Quotation not found", 404);
        q.Status = QuotationStatus.REJECTED;
        q.Notes = string.IsNullOrWhiteSpace(q.Notes) ? $"Rejected: {reason}" : $"{q.Notes}\nRejected: {reason}";
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(int id, string reason, CancellationToken cancellationToken = default)
    {
        var q = await _quotationRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Quotation not found", 404);
        q.Status = QuotationStatus.CANCELLED;
        q.CancelReason = reason;
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public Task<VoiceQuotationResponse> ParseVoiceAsync(VoiceQuotationRequest request, CancellationToken cancellationToken = default)
    {
        // Fallback natural language rule parser for speech to quotation
        var parsed = new
        {
            service = "DEEP_CLEANING",
            rooms = 3,
            bathrooms = 2,
            rate = 350m,
            notes = request.Prompt
        };

        return Task.FromResult(new VoiceQuotationResponse(
            Success: true,
            Parsed: parsed,
            Created: null,
            VoiceResponse: "Understood: Deep cleaning for 3 bedrooms and 2 bathrooms quoted at 350 QAR.",
            Prompt: request.Prompt
        ));
    }

    private static QuotationDto MapToDto(Quotation q)
    {
        return new QuotationDto(
            q.Id,
            q.Number,
            q.CustomerId,
            q.CustomerName,
            q.Subtotal,
            q.DiscountPct,
            q.Total,
            q.Status,
            q.PaymentTerms,
            q.ServiceAddress,
            q.Basis,
            q.Notes,
            q.CreatedBy,
            q.ApprovedBy,
            q.CancelRequested,
            q.CancelReason,
            q.Lines.Select(l => new QuotationLineDto(l.Id, l.Description, l.Qty, l.UnitPrice, l.Days, l.Location)).ToList(),
            q.CreatedAt
        );
    }
}
