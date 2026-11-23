namespace BroomErp.Application.DTOs;

public record CustomerDto(int Id, string? Number, string Name, string Phone, string Email, string Address, string Notes, bool Active, DateTime CreatedAt);
public record CreateCustomerRequest(string Name, string Phone = "", string Email = "", string Address = "", string Notes = "");
public record UpdateCustomerRequest(string? Name = null, string? Phone = null, string? Email = null, string? Address = null, string? Notes = null, bool? Active = null);

public record AddressDto(int Id, int CustomerId, string Label, string Address);
public record CreateAddressRequest(string Label, string Address);

public record CustomerHistoryDto(
    List<AddressDto> Addresses,
    List<EnquiryDto> Enquiries,
    List<QuotationDto> Quotations,
    List<JobDto> Jobs,
    List<InvoiceDto> Invoices,
    List<PaymentDto> Payments
);
