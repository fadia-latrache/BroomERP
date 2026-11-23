using BroomErp.Application.Common.Interfaces;
using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using BroomErp.Domain.Entities;
using BroomErp.Domain.Enums;
using BroomErp.Domain.Exceptions;

namespace BroomErp.Application.Services;

public class CashCloseService : ICashCloseService
{
    private readonly IRepository<CashClose> _closeRepo;
    private readonly IRepository<Payment> _paymentRepo;
    private readonly IRepository<Voucher> _voucherRepo;
    private readonly IUnitOfWork _uow;

    public CashCloseService(
        IRepository<CashClose> closeRepo,
        IRepository<Payment> paymentRepo,
        IRepository<Voucher> voucherRepo,
        IUnitOfWork uow)
    {
        _closeRepo = closeRepo;
        _paymentRepo = paymentRepo;
        _voucherRepo = voucherRepo;
        _uow = uow;
    }

    public async Task<PagedResult<CashCloseDto>> GetCashClosesAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _closeRepo.GetPagedAsync(query, null, cancellationToken);
        return new PagedResult<CashCloseDto>
        {
            Items = paged.Items.Select(c => new CashCloseDto(c.Id, c.CloseDate, c.OpeningFloat, c.CashReceived, c.CashPaidOut, c.HandedOver, c.Expected, c.Counted, c.Difference, c.Status, c.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<CashCloseDto> CreateCloseAsync(CreateCashCloseRequest request, CancellationToken cancellationToken = default)
    {
        var startUtc = request.CloseDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endUtc = request.CloseDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var payments = await _paymentRepo.ListAsync(p => p.Method == PaymentMethod.CASH && p.CreatedAt >= startUtc && p.CreatedAt <= endUtc, cancellationToken);
        var cashIn = payments.Sum(p => p.Amount);

        var vouchers = await _voucherRepo.ListAsync(v => v.Method == PaymentMethod.CASH && v.PaidOn == request.CloseDate, cancellationToken);
        var cashOut = vouchers.Sum(v => v.Amount);

        var expected = request.OpeningFloat + cashIn - cashOut - request.HandedOver;
        var diff = request.Counted - expected;

        var close = new CashClose
        {
            CloseDate = request.CloseDate,
            OpeningFloat = request.OpeningFloat,
            CashReceived = cashIn,
            CashPaidOut = cashOut,
            HandedOver = request.HandedOver,
            Expected = expected,
            Counted = request.Counted,
            Difference = diff,
            Status = diff == 0 ? CloseStatus.CLOSED : CloseStatus.NEEDS_APPROVAL
        };

        await _closeRepo.AddAsync(close, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new CashCloseDto(close.Id, close.CloseDate, close.OpeningFloat, close.CashReceived, close.CashPaidOut, close.HandedOver, close.Expected, close.Counted, close.Difference, close.Status, close.CreatedAt);
    }

    public async Task ApproveCloseAsync(int id, CancellationToken cancellationToken = default)
    {
        var close = await _closeRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Cash close record not found", 404);
        close.Status = CloseStatus.APPROVED;
        await _uow.SaveChangesAsync(cancellationToken);
    }
}

public class AdminService : IAdminService
{
    private readonly IRepository<Enquiry> _enquiryRepo;
    private readonly IRepository<Customer> _customerRepo;
    private readonly IRepository<Approval> _approvalRepo;
    private readonly IRepository<AuditLog> _auditRepo;
    private readonly IRepository<AuditChange> _changeRepo;
    private readonly IRepository<ServiceTypeRef> _serviceTypeRepo;
    private readonly IRepository<CompanyProfile> _companyRepo;
    private readonly IUnitOfWork _uow;
    private readonly ISequenceGenerator _sequences;
    private readonly ICurrentUserContext _userContext;

    public AdminService(
        IRepository<Enquiry> enquiryRepo,
        IRepository<Customer> customerRepo,
        IRepository<Approval> approvalRepo,
        IRepository<AuditLog> auditRepo,
        IRepository<AuditChange> changeRepo,
        IRepository<ServiceTypeRef> serviceTypeRepo,
        IRepository<CompanyProfile> companyRepo,
        IUnitOfWork uow,
        ISequenceGenerator sequences,
        ICurrentUserContext userContext)
    {
        _enquiryRepo = enquiryRepo;
        _customerRepo = customerRepo;
        _approvalRepo = approvalRepo;
        _auditRepo = auditRepo;
        _changeRepo = changeRepo;
        _serviceTypeRepo = serviceTypeRepo;
        _companyRepo = companyRepo;
        _uow = uow;
        _sequences = sequences;
        _userContext = userContext;
    }

    public async Task<PagedResult<EnquiryDto>> GetEnquiriesAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _enquiryRepo.GetPagedAsync(query, null, cancellationToken, e => e.Customer!);
        return new PagedResult<EnquiryDto>
        {
            Items = paged.Items.Select(e => new EnquiryDto(e.Id, e.Number, e.CustomerId, e.CustomerName, e.ServiceType, e.Address, e.RequestedDate, e.Notes, e.Status, e.CreatedBy, e.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<EnquiryDto> CreateEnquiryAsync(CreateEnquiryRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepo.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new DomainException("Customer not found", 404);

        var number = await _sequences.NextAsync("ENQ-", cancellationToken);
        var enquiry = new Enquiry
        {
            Number = number,
            CustomerId = request.CustomerId,
            ServiceType = request.ServiceType,
            Address = string.IsNullOrWhiteSpace(request.Address) ? customer.Address : request.Address,
            RequestedDate = request.RequestedDate,
            Notes = request.Notes,
            CreatedBy = _userContext.Email ?? "sales",
            Status = EnquiryStatus.NEW
        };

        await _enquiryRepo.AddAsync(enquiry, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        enquiry.Customer = customer;
        return new EnquiryDto(enquiry.Id, enquiry.Number, enquiry.CustomerId, customer.Name, enquiry.ServiceType, enquiry.Address, enquiry.RequestedDate, enquiry.Notes, enquiry.Status, enquiry.CreatedBy, enquiry.CreatedAt);
    }

    public async Task UpdateEnquiryStatusAsync(int id, UpdateEnquiryStatusRequest request, CancellationToken cancellationToken = default)
    {
        var e = await _enquiryRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Enquiry not found", 404);
        e.Status = request.Status;
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<ApprovalDto>> GetApprovalsAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _approvalRepo.GetPagedAsync(query, null, cancellationToken);
        return new PagedResult<ApprovalDto>
        {
            Items = paged.Items.Select(a => new ApprovalDto(a.Id, a.Kind, a.DocType, a.DocId, a.DocNumber, a.Status, a.Note, a.RequestedBy, a.DecidedBy, a.DecidedAt, a.Reason, a.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task DecideApprovalAsync(int id, DecideApprovalRequest request, CancellationToken cancellationToken = default)
    {
        var a = await _approvalRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Approval not found", 404);

        a.Status = request.Approved ? ApprovalStatus.APPROVED : ApprovalStatus.REJECTED;
        a.DecidedBy = _userContext.Email;
        a.DecidedAt = DateTime.UtcNow;
        a.Reason = request.Reason;

        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _auditRepo.GetPagedAsync(query, null, cancellationToken);
        return new PagedResult<AuditLogDto>
        {
            Items = paged.Items.Select(a => new AuditLogDto(a.Id, a.Actor, a.Role, a.Method, a.Path, a.StatusCode, a.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<PagedResult<AuditChangeDto>> GetAuditChangesAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _changeRepo.GetPagedAsync(query, null, cancellationToken);
        return new PagedResult<AuditChangeDto>
        {
            Items = paged.Items.Select(c => new AuditChangeDto(c.Id, c.Entity, c.EntityId, c.Field, c.Old, c.New, c.Actor, c.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<List<ServiceTypeDto>> GetServiceTypesAsync(CancellationToken cancellationToken = default)
    {
        var list = await _serviceTypeRepo.ListAsync(null, cancellationToken);
        return list.Select(s => new ServiceTypeDto(s.Id, s.Code, s.Name, s.Active)).ToList();
    }

    public async Task<ServiceTypeDto> CreateServiceTypeAsync(CreateServiceTypeRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var exists = await _serviceTypeRepo.AnyAsync(s => s.Code == code, cancellationToken);
        if (exists)
            throw new DomainException("Service type code already exists");

        var st = new ServiceTypeRef
        {
            Code = code,
            Name = request.Name.Trim(),
            Active = true
        };
        await _serviceTypeRepo.AddAsync(st, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new ServiceTypeDto(st.Id, st.Code, st.Name, st.Active);
    }

    public async Task<CompanyProfileDto> GetCompanyProfileAsync(CancellationToken cancellationToken = default)
    {
        var list = await _companyRepo.ListAsync(null, cancellationToken);
        var cp = list.FirstOrDefault();
        if (cp == null)
        {
            cp = new CompanyProfile();
            await _companyRepo.AddAsync(cp, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
        }
        return new CompanyProfileDto(cp.Id, cp.Name, cp.Address, cp.Phone, cp.CrNumber, cp.BankDetails, cp.LogoAttachmentId);
    }

    public async Task<CompanyProfileDto> UpdateCompanyProfileAsync(UpdateCompanyProfileRequest request, CancellationToken cancellationToken = default)
    {
        var list = await _companyRepo.ListAsync(null, cancellationToken);
        var cp = list.FirstOrDefault() ?? new CompanyProfile();

        if (request.Name != null) cp.Name = request.Name;
        if (request.Address != null) cp.Address = request.Address;
        if (request.Phone != null) cp.Phone = request.Phone;
        if (request.CrNumber != null) cp.CrNumber = request.CrNumber;
        if (request.BankDetails != null) cp.BankDetails = request.BankDetails;

        if (cp.Id == 0)
            await _companyRepo.AddAsync(cp, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);
        return new CompanyProfileDto(cp.Id, cp.Name, cp.Address, cp.Phone, cp.CrNumber, cp.BankDetails, cp.LogoAttachmentId);
    }
}

public class DashboardService : IDashboardService
{
    private readonly IRepository<Job> _jobRepo;
    private readonly IRepository<Quotation> _quotationRepo;
    private readonly IRepository<Invoice> _invoiceRepo;
    private readonly IRepository<StockItem> _stockRepo;
    private readonly IRepository<Approval> _approvalRepo;

    public DashboardService(
        IRepository<Job> jobRepo,
        IRepository<Quotation> quotationRepo,
        IRepository<Invoice> invoiceRepo,
        IRepository<StockItem> stockRepo,
        IRepository<Approval> approvalRepo)
    {
        _jobRepo = jobRepo;
        _quotationRepo = quotationRepo;
        _invoiceRepo = invoiceRepo;
        _stockRepo = stockRepo;
        _approvalRepo = approvalRepo;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var activeJobs = await _jobRepo.CountAsync(j => j.Status == JobStatus.PLANNED || j.Status == JobStatus.IN_PROGRESS, cancellationToken);
        var pendingQuotes = await _quotationRepo.CountAsync(q => q.Status == QuotationStatus.PENDING_APPROVAL || q.Status == QuotationStatus.DRAFT, cancellationToken);

        var firstDayOfMonth = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var invoicesThisMonth = await _invoiceRepo.ListAsync(i => i.CreatedAt >= firstDayOfMonth.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc), cancellationToken);
        var monthlyRevenue = invoicesThisMonth.Sum(i => i.Paid);

        var allUnpaid = await _invoiceRepo.ListAsync(i => i.Status == InvoiceStatus.UNPAID || i.Status == InvoiceStatus.PARTIAL, cancellationToken);
        var outstanding = allUnpaid.Sum(i => i.Balance);

        var lowStock = await _stockRepo.CountAsync(s => s.Qty <= s.ReorderLevel, cancellationToken);
        var pendingApprovals = await _approvalRepo.CountAsync(a => a.Status == ApprovalStatus.PENDING, cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayJobs = await _jobRepo.ListAsync(j => j.ScheduledFor == today, cancellationToken, j => j.Customer!, j => j.Assignments);
        var recentQuotes = await _quotationRepo.ListAsync(null, cancellationToken, q => q.Customer!, q => q.Lines);

        return new DashboardSummaryDto(
            activeJobs,
            pendingQuotes,
            monthlyRevenue,
            outstanding,
            lowStock,
            pendingApprovals,
            todayJobs.Take(10).Select(j => new JobDto(
                j.Id, j.Number, j.CustomerId, j.CustomerName, j.QuotationId, j.ServiceType,
                j.Address, j.ScheduledFor, j.StartTime, j.DurationHours, j.Notes, j.ProblemNotes,
                j.EndDate, j.TeamId, j.CancelRequested, j.CancelReason, j.Amount, j.Status,
                j.StaffCount, j.Delayed, j.CreatedAt
            )).ToList(),
            recentQuotes.Take(5).Select(q => new QuotationDto(
                q.Id, q.Number, q.CustomerId, q.CustomerName, q.Subtotal, q.DiscountPct, q.Total,
                q.Status, q.PaymentTerms, q.ServiceAddress, q.Basis, q.Notes, q.CreatedBy,
                q.ApprovedBy, q.CancelRequested, q.CancelReason,
                q.Lines.Select(l => new QuotationLineDto(l.Id, l.Description, l.Qty, l.UnitPrice, l.Days, l.Location)).ToList(),
                q.CreatedAt
            )).ToList()
        );
    }
}

public class ReportService : IReportService
{
    private readonly IRepository<Job> _jobRepo;
    private readonly IRepository<Invoice> _invoiceRepo;
    private readonly IRepository<Voucher> _voucherRepo;

    public ReportService(
        IRepository<Job> jobRepo,
        IRepository<Invoice> invoiceRepo,
        IRepository<Voucher> voucherRepo)
    {
        _jobRepo = jobRepo;
        _invoiceRepo = invoiceRepo;
        _voucherRepo = voucherRepo;
    }

    public async Task<ReportSummaryDto> GetFinancialSummaryAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await _invoiceRepo.ListAsync(null, cancellationToken);
        var totalRev = invoices.Sum(i => i.Total);
        var totalPaid = invoices.Sum(i => i.Paid);
        var totalBal = invoices.Sum(i => i.Balance);

        var vouchers = await _voucherRepo.ListAsync(v => v.Status == VoucherStatus.PAID, cancellationToken);
        var totalExp = vouchers.Sum(v => v.Amount);

        var jobs = await _jobRepo.ListAsync(null, cancellationToken);
        var completed = jobs.Count(j => j.Status == JobStatus.COMPLETED);
        var cancelled = jobs.Count(j => j.Status == JobStatus.CANCELLED);

        // Group invoices by month
        var monthly = invoices
            .GroupBy(i => i.CreatedAt.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .Take(12)
            .Select(g => new MonthlySalesDto(g.Key, g.Sum(i => i.Total)))
            .ToList();

        return new ReportSummaryDto(
            totalRev,
            totalPaid,
            totalBal,
            totalExp,
            jobs.Count,
            completed,
            cancelled,
            monthly
        );
    }
}
