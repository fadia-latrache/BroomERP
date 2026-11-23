using BroomErp.Application.Interfaces.Services;
using BroomErp.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BroomErp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<IQuotationService, QuotationService>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<IPurchasingService, PurchasingService>();
        services.AddScoped<ICashCloseService, CashCloseService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
