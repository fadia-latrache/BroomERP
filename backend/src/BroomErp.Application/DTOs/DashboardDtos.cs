namespace BroomErp.Application.DTOs;

public record DashboardSummaryDto(
    int ActiveJobs,
    int PendingQuotations,
    decimal MonthlyRevenue,
    decimal OutstandingInvoices,
    int LowStockItems,
    int PendingApprovals,
    List<JobDto> TodayJobs,
    List<QuotationDto> RecentQuotations
);

public record ReportSummaryDto(
    decimal TotalRevenue,
    decimal TotalCollected,
    decimal TotalOutstanding,
    decimal TotalExpenses,
    int TotalJobs,
    int CompletedJobs,
    int CancelledJobs,
    List<MonthlySalesDto> MonthlySales
);

public record MonthlySalesDto(string Month, decimal Total);
