# Broom ERP — .NET 10 + Angular 18 Enterprise Architecture

A modern rewrite of the Broom ERP system built using **Clean Architecture**, **SOLID Principles**, **ASP.NET Core Web API (.NET 10)**, and **Angular 18**.

---

## 🏛️ Architecture Overview

The solution resides in `broom-dotnet-angular/` and is divided into two separate, decoupled applications:

```
broom-dotnet-angular/
├── backend/                  # .NET 10 Clean Architecture Solution
│   ├── BroomErp.slnx
│   ├── src/
│   │   ├── BroomErp.Domain/          # Core Domain Entities, Enums, Exceptions (Pure C#)
│   │   ├── BroomErp.Application/     # CQRS/Services, DTOs, Business Logic, Interfaces
│   │   ├── BroomErp.Infrastructure/  # EF Core, SQLite, JWT Provider, BCrypt, PDF
│   │   └── BroomErp.Api/             # ASP.NET Core Controllers, Swagger, Middlewares
│   └── Dockerfile
│
├── frontend/                 # Angular 18 Single-Page Application
│   ├── src/
│   │   └── app/
│   │       ├── core/                 # Models, ApiService, AuthService, AuthInterceptor, AuthGuard
│   │       ├── shared/               # IconComponent, BadgeComponent, ModalComponent, MoneyPipe
│   │       └── features/             # Dashboard, Sales, Operations, Finance, Store, Purchasing, etc.
│   ├── Dockerfile
│   └── nginx.conf
│
└── docker-compose.yml        # Orchestration for both services
```

---

## 📐 SOLID Principles Implementation

| Principle | .NET 10 Backend | Angular 18 Frontend |
|---|---|---|
| **Single Responsibility (SRP)** | Every service (`CustomerService`, `JobService`, `InvoiceService`) encapsulates one business domain. | Components have dedicated roles (e.g. `ModalComponent` handles dialogs, `BadgeComponent` handles status pill styling). |
| **Open/Closed (OCP)** | Behavior is extended via interfaces (`IPdfGenerator`, `ISequenceGenerator`, `IRepository<T>`) without modifying existing code. | Feature routes and navigation dynamically map without altering the root shell. |
| **Liskov Substitution (LSP)** | Generic `Repository<T>` implements `IRepository<T>`, allowing any storage engine (SQLite, PostgreSQL, SQL Server) to be swapped transparently. | Services adhere to strongly typed TypeScript interfaces. |
| **Interface Segregation (ISP)** | Focused, domain-specific service contracts (`ICustomerService`, `IJobService`, `IStockService`) rather than one bloated API interface. | Granular models and typed observables. |
| **Dependency Inversion (DIP)** | `BroomErp.Application` defines abstractions (`IRepository<T>`, `IUnitOfWork`, `IJwtProvider`); `BroomErp.Infrastructure` implements them. Controller dependencies are resolved through .NET IoC. | Angular `inject()` and provider tokens decouple HTTP transport and authentication state. |

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Node.js 20+](https://nodejs.org/) & npm
- [Angular CLI 18](https://angular.dev/)

---

### Running the Backend (.NET 10)

```powershell
cd broom-dotnet-angular/backend
dotnet run --project src/BroomErp.Api/BroomErp.Api.csproj --urls "http://localhost:5000"
```

- **Swagger UI**: Visit [http://localhost:5000/swagger](http://localhost:5000/swagger)
- **Health Check**: [http://localhost:5000/api/health](http://localhost:5000/api/health)
- **Database**: Automatically created and initialized with SQLite (`broom_erp.db`) on first startup.

#### Default Admin Credentials
- **Email**: `gm@broomservices.qa`
- **Password**: `ChangeThisPasswordNow123!`

---

### Running the Frontend (Angular 18)

```powershell
cd broom-dotnet-angular/frontend
npm start
```

- **Web App**: Visit [http://localhost:4200](http://localhost:4200)
- Auto-configured proxy and CORS for `http://localhost:5000`

---

### Running with Docker Compose

To start both services with a single command:

```powershell
cd broom-dotnet-angular
docker compose up --build
```

- **Frontend App**: `http://localhost:4200`
- **Backend API**: `http://localhost:5000`

---

## 📦 Feature Coverage

1. **Authentication & RBAC**: JWT Bearer token generation, auto-locking on repeated failed logins, role-based permissions (`GM`, `FIELD`, `SALES`, `OPS`, `FINANCE`, `STORE`, `SUPERVISOR`).
2. **Sales & CRM**: Customer directory, address books, enquiries, and price quotations with multi-line calculation and discount logic.
3. **Quotation Workflow**: Draft &rarr; Request Approval &rarr; GM Approve &rarr; Accept (automatically provisions a confirmed Job with unique document number).
4. **Operations & Field Jobs**: Job lifecycle tracking (`PLANNED` &rarr; `IN_PROGRESS` &rarr; `COMPLETED`), staff crew assignments, supervisor completion forms.
5. **Invoicing & Finance**: Automated invoice generation upon job completion, partial/full payment recording, balance calculations, PDF invoice and quotation download.
6. **Store & Inventory**: Item master catalog, real-time quantity monitoring, low-stock alerts, stock movements (`RECEIVE`, `ISSUE`, `RETURN`, `ADJUST`), physical stock counts.
7. **Purchasing & LPOs**: Vendor directory, material requisitions, quotation selection, and Local Purchase Orders (LPOs).
8. **Executive Dashboard & Reports**: Real-time KPI summaries, today's schedule, recent quotations, financial totals, and monthly revenue breakdown.
