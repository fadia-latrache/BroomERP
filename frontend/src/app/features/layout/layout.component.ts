import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { IconComponent } from '../../shared/components/icon.component';
import { BadgeComponent } from '../../shared/components/badge.component';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, IconComponent, BadgeComponent],
  template: `
    <div class="flex h-screen bg-slate-50 font-sans antialiased text-slate-800">
      <!-- Sidebar -->
      <aside class="w-64 bg-slate-900 border-r border-slate-800 flex flex-col shrink-0">
        <!-- Brand -->
        <div class="h-16 flex items-center px-6 gap-3 border-b border-slate-800">
          <div class="w-9 h-9 rounded-xl bg-emerald-500 flex items-center justify-center text-white font-black text-xl shadow-md shadow-emerald-500/20">
            C
          </div>
          <div>
            <h1 class="text-sm font-bold text-white tracking-wide">CleanOps ERP</h1>
            <p class="text-[11px] text-slate-400 font-medium">Facilities Management</p>
          </div>
        </div>

        <!-- Navigation Menu -->
        <nav class="flex-1 px-3 py-4 space-y-6 overflow-y-auto">
          <!-- Overview -->
          <div>
            <div class="px-3 text-[11px] font-bold tracking-wider text-slate-400 uppercase mb-1.5">Overview</div>
            <div class="space-y-1">
              <a routerLink="/dashboard" routerLinkActive="bg-emerald-500/10 text-emerald-400 font-semibold" class="flex items-center gap-3 px-3 py-2 rounded-lg text-sm text-slate-300 hover:bg-slate-800 hover:text-white transition-colors">
                <app-icon name="dashboard" [size]="18"></app-icon>
                <span>Dashboard</span>
              </a>
              <a routerLink="/reports" routerLinkActive="bg-emerald-500/10 text-emerald-400 font-semibold" class="flex items-center gap-3 px-3 py-2 rounded-lg text-sm text-slate-300 hover:bg-slate-800 hover:text-white transition-colors">
                <app-icon name="bar-chart" [size]="18"></app-icon>
                <span>Reports & KPIs</span>
              </a>
            </div>
          </div>

          <!-- Sales & Clients -->
          <div>
            <div class="px-3 text-[11px] font-bold tracking-wider text-slate-400 uppercase mb-1.5">Commercial</div>
            <div class="space-y-1">
              <a routerLink="/customers" routerLinkActive="bg-emerald-500/10 text-emerald-400 font-semibold" class="flex items-center gap-3 px-3 py-2 rounded-lg text-sm text-slate-300 hover:bg-slate-800 hover:text-white transition-colors">
                <app-icon name="users" [size]="18"></app-icon>
                <span>Customers</span>
              </a>
              <a routerLink="/quotations" routerLinkActive="bg-emerald-500/10 text-emerald-400 font-semibold" class="flex items-center gap-3 px-3 py-2 rounded-lg text-sm text-slate-300 hover:bg-slate-800 hover:text-white transition-colors">
                <app-icon name="file-text" [size]="18"></app-icon>
                <span>Quotations</span>
              </a>
            </div>
          </div>

          <!-- Daily Operations -->
          <div>
            <div class="px-3 text-[11px] font-bold tracking-wider text-slate-400 uppercase mb-1.5">Operations</div>
            <div class="space-y-1">
              <a routerLink="/jobs" routerLinkActive="bg-emerald-500/10 text-emerald-400 font-semibold" class="flex items-center gap-3 px-3 py-2 rounded-lg text-sm text-slate-300 hover:bg-slate-800 hover:text-white transition-colors">
                <app-icon name="briefcase" [size]="18"></app-icon>
                <span>Dispatch & Jobs</span>
              </a>
            </div>
          </div>

          <!-- Billing & Cashier -->
          <div>
            <div class="px-3 text-[11px] font-bold tracking-wider text-slate-400 uppercase mb-1.5">Finance</div>
            <div class="space-y-1">
              <a routerLink="/invoices" routerLinkActive="bg-emerald-500/10 text-emerald-400 font-semibold" class="flex items-center gap-3 px-3 py-2 rounded-lg text-sm text-slate-300 hover:bg-slate-800 hover:text-white transition-colors">
                <app-icon name="dollar-sign" [size]="18"></app-icon>
                <span>Invoices & Payments</span>
              </a>
            </div>
          </div>

          <!-- Store & Stock -->
          <div>
            <div class="px-3 text-[11px] font-bold tracking-wider text-slate-400 uppercase mb-1.5">Storekeeper</div>
            <div class="space-y-1">
              <a routerLink="/stock" routerLinkActive="bg-emerald-500/10 text-emerald-400 font-semibold" class="flex items-center gap-3 px-3 py-2 rounded-lg text-sm text-slate-300 hover:bg-slate-800 hover:text-white transition-colors">
                <app-icon name="box" [size]="18"></app-icon>
                <span>Inventory & Materials</span>
              </a>
              <a routerLink="/purchasing" routerLinkActive="bg-emerald-500/10 text-emerald-400 font-semibold" class="flex items-center gap-3 px-3 py-2 rounded-lg text-sm text-slate-300 hover:bg-slate-800 hover:text-white transition-colors">
                <app-icon name="shopping-cart" [size]="18"></app-icon>
                <span>Suppliers & Orders</span>
              </a>
            </div>
          </div>

          <!-- Administration -->
          <div>
            <div class="px-3 text-[11px] font-bold tracking-wider text-slate-400 uppercase mb-1.5">Settings</div>
            <div class="space-y-1">
              <a routerLink="/company" routerLinkActive="bg-emerald-500/10 text-emerald-400 font-semibold" class="flex items-center gap-3 px-3 py-2 rounded-lg text-sm text-slate-300 hover:bg-slate-800 hover:text-white transition-colors">
                <app-icon name="settings" [size]="18"></app-icon>
                <span>Company Details</span>
              </a>
            </div>
          </div>
        </nav>

        <!-- Current User Profile Footer -->
        <div class="p-3.5 border-t border-slate-800 bg-slate-900/50 flex items-center justify-between">
          <div class="flex items-center gap-2.5 truncate mr-2">
            <div class="w-8 h-8 rounded-full bg-slate-800 border border-slate-700 flex items-center justify-center text-xs font-bold text-slate-300 shrink-0">
              {{ (auth.currentUser()?.email || 'U').charAt(0).toUpperCase() }}
            </div>
            <div class="truncate">
              <div class="text-xs font-semibold text-white truncate">{{ auth.currentUser()?.email || 'User' }}</div>
              <span class="text-[10px] font-medium text-emerald-400">General Manager</span>
            </div>
          </div>
          <button (click)="auth.logout()" title="Sign Out" class="p-1.5 text-slate-400 hover:text-rose-400 hover:bg-slate-800 rounded-lg transition-colors">
            <app-icon name="log-out" [size]="16"></app-icon>
          </button>
        </div>
      </aside>

      <!-- Main Container -->
      <div class="flex-1 flex flex-col overflow-hidden">
        <!-- Top App Header -->
        <header class="h-16 bg-white border-b border-slate-200/80 flex items-center justify-between px-8 shadow-sm shrink-0">
          <div class="flex items-center gap-3">
            <span class="text-sm font-semibold text-slate-700">Qatar Central Operations</span>
            <span class="text-xs text-slate-400 font-normal">|</span>
            <span class="text-xs font-medium text-slate-500">{{ today | date:'fullDate' }}</span>
          </div>

          <div class="flex items-center gap-3">
            <span class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200">
              <span class="w-2 h-2 rounded-full bg-emerald-500"></span>
              System Active
            </span>
          </div>
        </header>

        <!-- Dynamic Content Body -->
        <main class="flex-1 overflow-y-auto p-8 bg-slate-50">
          <router-outlet></router-outlet>
        </main>
      </div>
    </div>
  `
})
export class LayoutComponent {
  auth = inject(AuthService);
  today = new Date();
}
