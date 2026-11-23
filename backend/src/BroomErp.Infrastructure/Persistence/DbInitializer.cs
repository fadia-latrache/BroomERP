using BroomErp.Application.Common.Interfaces;
using BroomErp.Domain.Entities;
using BroomErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BroomErp.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await context.Database.EnsureCreatedAsync();

        // 1. Users
        if (!await context.Users.AnyAsync())
        {
            var adminEmail = config["Admin:Email"] ?? "gm@broomservices.qa";
            var adminPassword = config["Admin:Password"] ?? "ChangeThisPasswordNow123!";

            var users = new List<User>
            {
                new User
                {
                    Email = adminEmail.ToLowerInvariant(),
                    PasswordHash = hasher.Hash(adminPassword),
                    Role = Role.GM,
                    Active = true
                },
                new User
                {
                    Email = "ops@broomservices.qa",
                    PasswordHash = hasher.Hash("BroomOps2026!"),
                    Role = Role.OPS,
                    Active = true
                },
                new User
                {
                    Email = "finance@broomservices.qa",
                    PasswordHash = hasher.Hash("BroomFinance2026!"),
                    Role = Role.FINANCE,
                    Active = true
                },
                new User
                {
                    Email = "store@broomservices.qa",
                    PasswordHash = hasher.Hash("BroomStore2026!"),
                    Role = Role.STORE,
                    Active = true
                }
            };
            context.Users.AddRange(users);
        }

        // 2. Company Profile
        if (!await context.CompanyProfiles.AnyAsync())
        {
            context.CompanyProfiles.Add(new CompanyProfile
            {
                Name = "CleanOps Facilities & Commercial Cleaning Co.",
                Address = "Building 42, Grand Hamad Avenue, Doha, Qatar",
                Phone = "+974 4488 9900",
                CrNumber = "104928/CR",
                BankDetails = "Qatar National Bank (QNB) - IBAN: QA99QNBA0000000010492801"
            });
        }

        // 3. Service Types
        if (!await context.ServiceTypes.AnyAsync())
        {
            context.ServiceTypes.AddRange(
                new ServiceTypeRef { Code = "DEEP_CLEANING", Name = "Deep Cleaning & Sanitization" },
                new ServiceTypeRef { Code = "COMMERCIAL", Name = "Commercial Office Daily Janitorial" },
                new ServiceTypeRef { Code = "MARBLE_POLISH", Name = "Marble Crystallization & Restoration" },
                new ServiceTypeRef { Code = "CARPET_CARE", Name = "Industrial Carpet Extraction & Shampoo" },
                new ServiceTypeRef { Code = "FACADE_CLEAN", Name = "Exterior Glass & High-Rise Facade" },
                new ServiceTypeRef { Code = "AC_DUCT", Name = "HVAC & AC Duct Disinfection" }
            );
        }

        // 4. Customers
        if (!await context.Customers.AnyAsync())
        {
            var customers = new List<Customer>
            {
                new Customer
                {
                    Number = "CUST-000101",
                    Name = "Al Rayyan Commercial Towers",
                    Phone = "+974 4455 1100",
                    Email = "facility@alrayyantowers.qa",
                    Address = "Building 12, West Bay Commercial District, Doha",
                    Notes = "Requires security gate pass before 7:30 AM dispatch.",
                    Active = true
                },
                new Customer
                {
                    Number = "CUST-000102",
                    Name = "Lusail Marina Suites & Residences",
                    Phone = "+974 4499 8811",
                    Email = "operations@lusailmarina.qa",
                    Address = "Marina Promenade, Tower B, Lusail City",
                    Notes = "Weekly marble polishing in main reception lobby.",
                    Active = true
                },
                new Customer
                {
                    Number = "CUST-000103",
                    Name = "The Pearl Luxury Compound",
                    Phone = "+974 5512 3456",
                    Email = "management@pearlvillas.qa",
                    Address = "Porto Arabia, Parcel 8, The Pearl, Doha",
                    Notes = "Private villa move-in and turnover deep cleaning.",
                    Active = true
                },
                new Customer
                {
                    Number = "CUST-000104",
                    Name = "Al Sadd Specialized Medical Clinic",
                    Phone = "+974 4422 7700",
                    Email = "admin@alsaddclinic.qa",
                    Address = "Al Sadd Street, Zone 38, Doha",
                    Notes = "Medical-grade hospital disinfectant protocol mandatory.",
                    Active = true
                }
            };
            context.Customers.AddRange(customers);
            await context.SaveChangesAsync();
        }

        // 5. Staff
        if (!await context.Staff.AnyAsync())
        {
            var staff = new List<Staff>
            {
                new Staff { Number = "STF-001", Name = "Tariq Mansoor", Phone = "+974 5500 1122", Position = "Operations Supervisor", Active = true },
                new Staff { Number = "STF-002", Name = "Kamal Hossain", Phone = "+974 5500 3344", Position = "Lead Fleet Driver", Active = true },
                new Staff { Number = "STF-003", Name = "Sunita Sharma", Phone = "+974 5500 5566", Position = "Senior Cleaning Specialist", Active = true },
                new Staff { Number = "STF-004", Name = "Maria Santos", Phone = "+974 5500 7788", Position = "Housekeeping Specialist", Active = true },
                new Staff { Number = "STF-005", Name = "Rajesh Kumar", Phone = "+974 5500 9900", Position = "Floor & Carpet Technician", Active = true }
            };
            context.Staff.AddRange(staff);
            await context.SaveChangesAsync();

            // Teams
            var team1 = new Team { Name = "Alpha Commercial Crew", SupervisorStaffId = staff[0].Id, Active = true };
            context.Teams.Add(team1);
            await context.SaveChangesAsync();

            context.TeamMembers.AddRange(
                new TeamMember { TeamId = team1.Id, StaffId = staff[1].Id },
                new TeamMember { TeamId = team1.Id, StaffId = staff[2].Id },
                new TeamMember { TeamId = team1.Id, StaffId = staff[3].Id }
            );
        }

        // 6. Quotations
        if (!await context.Quotations.AnyAsync())
        {
            var cust1 = await context.Customers.FirstAsync();
            var q1 = new Quotation
            {
                Number = "QT-2026-001",
                CustomerId = cust1.Id,
                ServiceAddress = cust1.Address,
                Basis = "DAILY",
                PaymentTerms = "Net 30 Days after invoice issuance",
                DiscountPct = 5,
                Total = 4750.00m,
                Status = QuotationStatus.APPROVED,
                CreatedBy = "gm@broomservices.qa",
                Notes = "Annual comprehensive tower lobby and public washroom deep cleaning.",
                Lines = new List<QuotationLine>
                {
                    new QuotationLine { Description = "Marble Lobby Diamond Grinding & Crystallization", Qty = 1, UnitPrice = 3000m, Days = 2, Location = "Main Atrium" },
                    new QuotationLine { Description = "High-Traffic Carpet Rotary Shampoo Treatment", Qty = 2, UnitPrice = 1000m, Days = 1, Location = "Mezzanine Offices" }
                }
            };

            var cust2 = await context.Customers.Skip(1).FirstAsync();
            var q2 = new Quotation
            {
                Number = "QT-2026-002",
                CustomerId = cust2.Id,
                ServiceAddress = cust2.Address,
                Basis = "DAILY",
                PaymentTerms = "Immediate upon completion",
                DiscountPct = 0,
                Total = 1850.00m,
                Status = QuotationStatus.ACCEPTED,
                CreatedBy = "gm@broomservices.qa",
                Notes = "Executive penthouse move-in full handover cleaning.",
                Lines = new List<QuotationLine>
                {
                    new QuotationLine { Description = "Deluxe Turnover Deep Cleaning & Glass Polish", Qty = 1, UnitPrice = 1850m, Days = 1, Location = "Penthouse 45" }
                }
            };

            context.Quotations.AddRange(q1, q2);
            await context.SaveChangesAsync();
        }

        // 7. Jobs / Work Orders
        if (!await context.Jobs.AnyAsync())
        {
            var cust2 = await context.Customers.Skip(1).FirstAsync();
            var cust3 = await context.Customers.Skip(2).FirstAsync();
            var staffList = await context.Staff.ToListAsync();

            var job1 = new Job
            {
                Number = "JOB-2026-081",
                CustomerId = cust2.Id,
                ServiceType = "DEEP_CLEANING",
                Address = "Lusail Marina Suites, Tower B - Penthouse 45",
                ScheduledFor = DateOnly.FromDateTime(DateTime.UtcNow),
                StartTime = "08:30",
                DurationHours = 6,
                Amount = 1850.00m,
                Status = JobStatus.IN_PROGRESS,
                Notes = "High priority executive suite turnover."
            };

            var job2 = new Job
            {
                Number = "JOB-2026-082",
                CustomerId = cust3.Id,
                ServiceType = "MARBLE_POLISH",
                Address = "The Pearl Luxury Compound, Villa 18",
                ScheduledFor = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                StartTime = "09:00",
                DurationHours = 4,
                Amount = 2400.00m,
                Status = JobStatus.PLANNED,
                Notes = "Italian Carrara marble restoration."
            };

            var job3 = new Job
            {
                Number = "JOB-2026-080",
                CustomerId = cust2.Id,
                ServiceType = "COMMERCIAL",
                Address = "Lusail Marina Suites, Reception Atrium",
                ScheduledFor = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                StartTime = "07:00",
                DurationHours = 5,
                Amount = 1500.00m,
                Status = JobStatus.COMPLETED,
                Notes = "Monthly entrance and glass sanitization."
            };

            context.Jobs.AddRange(job1, job2, job3);
            await context.SaveChangesAsync();

            // Assign staff to in-progress job
            context.JobAssignments.AddRange(
                new JobAssignment { JobId = job1.Id, StaffId = staffList[0].Id, Role = "SUPERVISOR", Active = true },
                new JobAssignment { JobId = job1.Id, StaffId = staffList[2].Id, Role = "CREW", Active = true },
                new JobAssignment { JobId = job1.Id, StaffId = staffList[3].Id, Role = "CREW", Active = true }
            );

            // Completion report for job3
            context.JobCompletions.Add(new JobCompletion
            {
                JobId = job3.Id,
                SubmittedBy = "Tariq Mansoor",
                WorkDone = "Full lobby scrubbing, marble buffing, and high glass polish completed according to SLA.",
                CompletionStatus = "FULL",
                Complaint = "None. Client signed off on satisfaction slip."
            });

            // Invoice for completed job
            var inv1 = new Invoice
            {
                Number = "INV-2026-0045",
                CustomerId = cust2.Id,
                JobId = job3.Id,
                Total = 1500.00m,
                Paid = 1500.00m,
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)),
                Status = InvoiceStatus.PAID
            };
            context.Invoices.Add(inv1);
            await context.SaveChangesAsync();

            context.Payments.Add(new Payment
            {
                ReceiptNo = "RCP-2026-0012",
                InvoiceId = inv1.Id,
                Amount = 1500.00m,
                Method = PaymentMethod.BANK_TRANSFER,
                Reference = "QNB-REF-884920",
                ReceivedBy = "finance@broomservices.qa"
            });
        }

        // 8. Inventory & Stock Items
        if (!await context.StockItems.AnyAsync())
        {
            context.StockItems.AddRange(
                new StockItem { Name = "Industrial Heavy-Duty Floor Degreaser (20L)", Category = "Chemicals", Unit = "drums", Qty = 14, ReorderLevel = 5 },
                new StockItem { Name = "Marble Crystallization Chemical Compound (5kg)", Category = "Chemicals", Unit = "cans", Qty = 8, ReorderLevel = 3 },
                new StockItem { Name = "Hospital-Grade Multi-Surface Disinfectant (5L)", Category = "Chemicals", Unit = "gallons", Qty = 22, ReorderLevel = 8 },
                new StockItem { Name = "Microfiber Color-Coded Cleaning Cloths (50 Pack)", Category = "Supplies", Unit = "packs", Qty = 4, ReorderLevel = 10 }, // Low stock trigger
                new StockItem { Name = "Commercial Rotary Floor Buffer Machine 17\"", Category = "Machinery", Unit = "units", Qty = 6, ReorderLevel = 2 },
                new StockItem { Name = "Industrial Wet & Dry Vacuum Cleaner 60L", Category = "Machinery", Unit = "units", Qty = 5, ReorderLevel = 2 }
            );
        }

        // 9. Suppliers & Requisitions
        if (!await context.Suppliers.AnyAsync())
        {
            var sup1 = new Supplier { Name = "Qatar Industrial Chemical Supplies W.L.L", Phone = "+974 4450 8800" };
            var sup2 = new Supplier { Name = "Gulf Clean Machinery & Spares Trading", Phone = "+974 4471 2233" };
            context.Suppliers.AddRange(sup1, sup2);
            await context.SaveChangesAsync();

            context.Requisitions.AddRange(
                new Requisition
                {
                    Number = "REQ-2026-018",
                    Description = "20x Microfiber Cloth Packs & 10x Nitrile Glove Boxes",
                    Qty = 30,
                    EstUnitPrice = 45m,
                    Department = "Operations Store",
                    Reason = "Warehouse stock depleted below critical buffer level.",
                    RequestedBy = "store@broomservices.qa",
                    Status = RequisitionStatus.APPROVED
                },
                new Requisition
                {
                    Number = "REQ-2026-019",
                    Description = "Replacement Squeegee Blades & Vacuum Hose Adapters",
                    Qty = 12,
                    EstUnitPrice = 80m,
                    Department = "Equipment Maintenance",
                    Reason = "Scheduled maintenance overhaul for fleet rotary machines.",
                    RequestedBy = "ops@broomservices.qa",
                    Status = RequisitionStatus.DRAFT
                }
            );
        }

        await context.SaveChangesAsync();
    }
}
