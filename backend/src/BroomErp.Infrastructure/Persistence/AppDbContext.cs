using BroomErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BroomErp.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Sequence> Sequences => Set<Sequence>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Attendance> Attendance => Set<Attendance>();
    public DbSet<RosterEntry> RosterEntries => Set<RosterEntry>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationLine> QuotationLines => Set<QuotationLine>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobAssignment> JobAssignments => Set<JobAssignment>();
    public DbSet<JobCompletion> JobCompletions => Set<JobCompletion>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<StockCount> StockCounts => Set<StockCount>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetAssignment> AssetAssignments => Set<AssetAssignment>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<RentalMachine> Rentals => Set<RentalMachine>();
    public DbSet<RentalTimesheet> RentalTimesheets => Set<RentalTimesheet>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Requisition> Requisitions => Set<Requisition>();
    public DbSet<SupplierQuotation> SupplierQuotations => Set<SupplierQuotation>();
    public DbSet<Lpo> Lpos => Set<Lpo>();
    public DbSet<SupplierInvoice> SupplierInvoices => Set<SupplierInvoice>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<CashClose> CashCloses => Set<CashClose>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Enquiry> Enquiries => Set<Enquiry>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AuditChange> AuditChanges => Set<AuditChange>();
    public DbSet<Approval> Approvals => Set<Approval>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<ServiceTypeRef> ServiceTypes => Set<ServiceTypeRef>();
    public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Sequence>().HasKey(s => s.Prefix);
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Customer>().HasIndex(c => c.Number).IsUnique();
        modelBuilder.Entity<Quotation>().HasIndex(q => q.Number).IsUnique();
        modelBuilder.Entity<Job>().HasIndex(j => j.Number).IsUnique();
        modelBuilder.Entity<Invoice>().HasIndex(i => i.Number).IsUnique();

        modelBuilder.Entity<Quotation>()
            .HasMany(q => q.Lines)
            .WithOne(l => l.Quotation)
            .HasForeignKey(l => l.QuotationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Team>()
            .HasMany(t => t.Members)
            .WithOne(m => m.Team)
            .HasForeignKey(m => m.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Job>()
            .HasMany(j => j.Assignments)
            .WithOne(a => a.Job)
            .HasForeignKey(a => a.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Job>()
            .HasOne(j => j.Completion)
            .WithOne(c => c.Job)
            .HasForeignKey<JobCompletion>(c => c.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
