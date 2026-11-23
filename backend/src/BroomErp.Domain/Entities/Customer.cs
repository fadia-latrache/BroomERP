using BroomErp.Domain.Common;

namespace BroomErp.Domain.Entities;

public class Customer : BaseEntity, IAggregateRoot
{
    public string? Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool Active { get; set; } = true;

    public List<CustomerAddress> Addresses { get; set; } = [];
}

public class CustomerAddress : BaseEntity
{
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
