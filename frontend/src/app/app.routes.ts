import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { LoginComponent } from './features/auth/login.component';
import { LayoutComponent } from './features/layout/layout.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { CustomersComponent } from './features/sales/customers.component';
import { QuotationsComponent } from './features/sales/quotations.component';
import { JobsComponent } from './features/operations/jobs.component';
import { InvoicesComponent } from './features/finance/invoices.component';
import { StockComponent } from './features/store/stock.component';
import { PurchasingComponent } from './features/purchasing/purchasing.component';
import { ReportsComponent } from './features/reports/reports.component';
import { CompanyComponent } from './features/company/company.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'customers', component: CustomersComponent },
      { path: 'quotations', component: QuotationsComponent },
      { path: 'jobs', component: JobsComponent },
      { path: 'invoices', component: InvoicesComponent },
      { path: 'stock', component: StockComponent },
      { path: 'purchasing', component: PurchasingComponent },
      { path: 'reports', component: ReportsComponent },
      { path: 'company', component: CompanyComponent },
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
