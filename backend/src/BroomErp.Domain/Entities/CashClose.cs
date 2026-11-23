using BroomErp.Domain.Common;
using BroomErp.Domain.Enums;

namespace BroomErp.Domain.Entities;

public class CashClose : BaseEntity, IAggregateRoot
{
    public DateOnly CloseDate { get; set; }
    public decimal OpeningFloat { get; set; }
    public decimal Expected { get; set; }
    public decimal CashReceived { get; set; } = 0;
    public decimal CashPaidOut { get; set; } = 0;
    public decimal HandedOver { get; set; } = 0;
    public decimal Counted { get; set; }
    public decimal Difference { get; set; }
    public CloseStatus Status { get; set; }
}
