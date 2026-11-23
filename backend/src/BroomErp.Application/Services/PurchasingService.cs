using BroomErp.Application.Common.Interfaces;
using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using BroomErp.Domain.Entities;
using BroomErp.Domain.Enums;
using BroomErp.Domain.Exceptions;

namespace BroomErp.Application.Services;

public class PurchasingService : IPurchasingService
{
    private readonly IRepository<Supplier> _supplierRepo;
    private readonly IRepository<Requisition> _reqRepo;
    private readonly IRepository<SupplierQuotation> _sqRepo;
    private readonly IRepository<Lpo> _lpoRepo;
    private readonly IRepository<SupplierInvoice> _supInvRepo;
    private readonly IRepository<Voucher> _voucherRepo;
    private readonly IUnitOfWork _uow;
    private readonly ISequenceGenerator _sequences;
    private readonly ICurrentUserContext _userContext;

    public PurchasingService(
        IRepository<Supplier> supplierRepo,
        IRepository<Requisition> reqRepo,
        IRepository<SupplierQuotation> sqRepo,
        IRepository<Lpo> lpoRepo,
        IRepository<SupplierInvoice> supInvRepo,
        IRepository<Voucher> voucherRepo,
        IUnitOfWork uow,
        ISequenceGenerator sequences,
        ICurrentUserContext userContext)
    {
        _supplierRepo = supplierRepo;
        _reqRepo = reqRepo;
        _sqRepo = sqRepo;
        _lpoRepo = lpoRepo;
        _supInvRepo = supInvRepo;
        _voucherRepo = voucherRepo;
        _uow = uow;
        _sequences = sequences;
        _userContext = userContext;
    }

    public async Task<PagedResult<SupplierDto>> GetSuppliersAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _supplierRepo.GetPagedAsync(query,
            s => (string.IsNullOrEmpty(query.Search) || s.Name.ToLower().Contains(query.Search.ToLower()) || s.Phone.Contains(query.Search)),
            cancellationToken);

        return new PagedResult<SupplierDto>
        {
            Items = paged.Items.Select(s => new SupplierDto(s.Id, s.Name, s.Phone, s.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        var supplier = new Supplier
        {
            Name = request.Name.Trim(),
            Phone = request.Phone.Trim()
        };
        await _supplierRepo.AddAsync(supplier, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return new SupplierDto(supplier.Id, supplier.Name, supplier.Phone, supplier.CreatedAt);
    }

    public async Task<PagedResult<RequisitionDto>> GetRequisitionsAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _reqRepo.GetPagedAsync(query, null, cancellationToken);
        return new PagedResult<RequisitionDto>
        {
            Items = paged.Items.Select(r => new RequisitionDto(r.Id, r.Number, r.Description, r.Qty, r.EstUnitPrice, r.ItemId, r.Department, r.Reason, r.RequestedBy, r.Status, r.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<RequisitionDto> CreateRequisitionAsync(CreateRequisitionRequest request, CancellationToken cancellationToken = default)
    {
        var number = await _sequences.NextAsync("REQ-", cancellationToken);
        var req = new Requisition
        {
            Number = number,
            Description = request.Description,
            Qty = request.Qty,
            EstUnitPrice = request.EstUnitPrice,
            ItemId = request.ItemId,
            Department = request.Department,
            Reason = request.Reason,
            RequestedBy = _userContext.Email ?? "user",
            Status = RequisitionStatus.DRAFT
        };
        await _reqRepo.AddAsync(req, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return new RequisitionDto(req.Id, req.Number, req.Description, req.Qty, req.EstUnitPrice, req.ItemId, req.Department, req.Reason, req.RequestedBy, req.Status, req.CreatedAt);
    }

    public async Task ApproveRequisitionAsync(int id, CancellationToken cancellationToken = default)
    {
        var req = await _reqRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Requisition not found", 404);
        req.Status = RequisitionStatus.APPROVED;
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<SupplierQuotationDto>> GetSupplierQuotationsAsync(int requisitionId, CancellationToken cancellationToken = default)
    {
        var list = await _sqRepo.ListAsync(q => q.RequisitionId == requisitionId, cancellationToken, q => q.Supplier!);
        return list.Select(q => new SupplierQuotationDto(q.Id, q.Number, q.RequisitionId, q.SupplierId, q.SupplierName, q.UnitPrice, q.Notes, q.Selected, q.CreatedAt)).ToList();
    }

    public async Task<SupplierQuotationDto> AddSupplierQuotationAsync(int requisitionId, CreateSupplierQuotationRequest request, CancellationToken cancellationToken = default)
    {
        var number = await _sequences.NextAsync("SQ-", cancellationToken);
        var sq = new SupplierQuotation
        {
            Number = number,
            RequisitionId = requisitionId,
            SupplierId = request.SupplierId,
            UnitPrice = request.UnitPrice,
            Notes = request.Notes
        };
        await _sqRepo.AddAsync(sq, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var sup = await _supplierRepo.GetByIdAsync(request.SupplierId, cancellationToken);
        return new SupplierQuotationDto(sq.Id, sq.Number, sq.RequisitionId, sq.SupplierId, sup?.Name, sq.UnitPrice, sq.Notes, sq.Selected, sq.CreatedAt);
    }

    public async Task SelectSupplierQuotationAsync(int quotationId, CancellationToken cancellationToken = default)
    {
        var sq = await _sqRepo.GetByIdAsync(quotationId, cancellationToken)
            ?? throw new DomainException("Supplier quotation not found", 404);

        var all = await _sqRepo.ListAsync(q => q.RequisitionId == sq.RequisitionId, cancellationToken);
        foreach (var item in all)
            item.Selected = item.Id == quotationId;

        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<LpoDto>> GetLposAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _lpoRepo.GetPagedAsync(query, null, cancellationToken, l => l.Supplier!);
        return new PagedResult<LpoDto>
        {
            Items = paged.Items.Select(l => new LpoDto(l.Id, l.Number, l.RequisitionId, l.SupplierId, l.SupplierName, l.Qty, l.UnitPrice, l.Total, l.Status, l.GrnNo, l.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<LpoDto> CreateLpoAsync(int requisitionId, CreateLpoRequest request, CancellationToken cancellationToken = default)
    {
        var req = await _reqRepo.GetByIdAsync(requisitionId, cancellationToken)
            ?? throw new DomainException("Requisition not found", 404);

        int supplierId;
        decimal unitPrice;

        if (request.SupplierId.HasValue && request.UnitPrice.HasValue)
        {
            supplierId = request.SupplierId.Value;
            unitPrice = request.UnitPrice.Value;
        }
        else
        {
            var quotes = await _sqRepo.ListAsync(q => q.RequisitionId == requisitionId && q.Selected, cancellationToken);
            var selected = quotes.FirstOrDefault()
                ?? throw new DomainException("No supplier selected for this requisition. Specify supplier and unit price or select a quote.");
            supplierId = selected.SupplierId;
            unitPrice = selected.UnitPrice;
        }

        var number = await _sequences.NextAsync("LPO-", cancellationToken);
        var lpo = new Lpo
        {
            Number = number,
            RequisitionId = requisitionId,
            SupplierId = supplierId,
            Qty = req.Qty,
            UnitPrice = unitPrice,
            Total = req.Qty * unitPrice,
            Status = LpoStatus.PENDING_APPROVAL
        };

        req.Status = RequisitionStatus.ORDERED;

        await _lpoRepo.AddAsync(lpo, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var sup = await _supplierRepo.GetByIdAsync(supplierId, cancellationToken);
        return new LpoDto(lpo.Id, lpo.Number, lpo.RequisitionId, lpo.SupplierId, sup?.Name, lpo.Qty, lpo.UnitPrice, lpo.Total, lpo.Status, lpo.GrnNo, lpo.CreatedAt);
    }

    public async Task ApproveLpoAsync(int id, CancellationToken cancellationToken = default)
    {
        var lpo = await _lpoRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("LPO not found", 404);
        lpo.Status = LpoStatus.ORDERED;
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<SupplierInvoiceDto>> GetSupplierInvoicesAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _supInvRepo.GetPagedAsync(query, null, cancellationToken, s => s.Lpo!, s => s.Lpo!.Supplier!);
        return new PagedResult<SupplierInvoiceDto>
        {
            Items = paged.Items.Select(s => new SupplierInvoiceDto(s.Id, s.Number, s.SupplierRef, s.LpoId, s.LpoNumber, s.SupplierName, s.Amount, s.Paid, s.Balance, s.DueDate, s.MatchesLpo, s.Status, s.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<SupplierInvoiceDto> RecordSupplierInvoiceAsync(CreateSupplierInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var lpo = await _lpoRepo.GetByIdAsync(request.LpoId, cancellationToken, l => l.Supplier!)
            ?? throw new DomainException("LPO not found", 404);

        var number = await _sequences.NextAsync("SINV-", cancellationToken);
        var sinv = new SupplierInvoice
        {
            Number = number,
            SupplierRef = request.SupplierRef,
            LpoId = request.LpoId,
            Amount = request.Amount,
            DueDate = request.DueDate,
            MatchesLpo = request.Amount == lpo.Total,
            Status = SupplierInvoiceStatus.RECORDED
        };
        await _supInvRepo.AddAsync(sinv, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        sinv.Lpo = lpo;
        return new SupplierInvoiceDto(sinv.Id, sinv.Number, sinv.SupplierRef, sinv.LpoId, lpo.Number, lpo.SupplierName, sinv.Amount, sinv.Paid, sinv.Balance, sinv.DueDate, sinv.MatchesLpo, sinv.Status, sinv.CreatedAt);
    }

    public async Task<PagedResult<VoucherDto>> GetVouchersAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _voucherRepo.GetPagedAsync(query, null, cancellationToken);
        return new PagedResult<VoucherDto>
        {
            Items = paged.Items.Select(v => new VoucherDto(v.Id, v.Number, v.Kind, v.Payee, v.Amount, v.Method, v.Status, v.LpoId, v.SupplierInvoiceId, v.InvoiceId, v.Note, v.PaidOn, v.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<VoucherDto> CreateVoucherAsync(CreateVoucherRequest request, CancellationToken cancellationToken = default)
    {
        var number = await _sequences.NextAsync("VCH-", cancellationToken);
        var voucher = new Voucher
        {
            Number = number,
            Kind = request.Kind,
            Payee = request.Payee,
            Amount = request.Amount,
            Method = request.Method,
            LpoId = request.LpoId,
            SupplierInvoiceId = request.SupplierInvoiceId,
            InvoiceId = request.InvoiceId,
            Note = request.Note,
            Status = VoucherStatus.PENDING_APPROVAL
        };
        await _voucherRepo.AddAsync(voucher, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new VoucherDto(voucher.Id, voucher.Number, voucher.Kind, voucher.Payee, voucher.Amount, voucher.Method, voucher.Status, voucher.LpoId, voucher.SupplierInvoiceId, voucher.InvoiceId, voucher.Note, voucher.PaidOn, voucher.CreatedAt);
    }

    public async Task ApproveVoucherAsync(int id, CancellationToken cancellationToken = default)
    {
        var voucher = await _voucherRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Voucher not found", 404);
        voucher.Status = VoucherStatus.APPROVED;
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task PayVoucherAsync(int id, CancellationToken cancellationToken = default)
    {
        var voucher = await _voucherRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Voucher not found", 404);

        voucher.Status = VoucherStatus.PAID;
        voucher.PaidOn = DateOnly.FromDateTime(DateTime.UtcNow);

        if (voucher.SupplierInvoiceId.HasValue)
        {
            var sinv = await _supInvRepo.GetByIdAsync(voucher.SupplierInvoiceId.Value, cancellationToken);
            if (sinv != null)
            {
                sinv.Paid += voucher.Amount;
                if (sinv.Paid >= sinv.Amount)
                    sinv.Status = SupplierInvoiceStatus.PAID;
                else
                    sinv.Status = SupplierInvoiceStatus.PARTIAL;
            }
        }

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
