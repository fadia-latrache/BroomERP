import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { DashboardSummary } from '../../core/models/models';
import { IconComponent } from '../../shared/components/icon.component';
import { BadgeComponent } from '../../shared/components/badge.component';
import { MoneyPipe } from '../../shared/pipes/money.pipe';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, IconComponent, BadgeComponent, MoneyPipe],
  template: `
    <div class="space-y-8 animate-fade-in">
      <!-- Title -->
      <div>
        <h2 class="text-2xl font-bold text-slate-900 tracking-tight">Executive Dashboard</h2>
        <p class="text-sm text-slate-500 mt-1">Overview of daily operations, ongoing work orders, and financial summaries</p>
      </div>

      <!-- 4 Top KPI Cards -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <!-- Card 1: Active Jobs -->
        <div class="bg-white rounded-xl p-6 border border-slate-200/80 shadow-sm flex items-center justify-between">
          <div>
            <p class="text-xs font-semibold text-slate-500 uppercase tracking-wider">Active Jobs</p>
            <h3 class="text-3xl font-extrabold text-slate-900 mt-2">{{ summary()?.activeJobs ?? 0 }}</h3>
            <p class="text-xs text-blue-600 font-medium mt-1">Scheduled & In Progress</p>
          </div>
          <div class="w-12 h-12 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center">
            <app-icon name="briefcase" [size]="22"></app-icon>
          </div>
        </div>

        <!-- Card 2: Pending Quotations -->
        <div class="bg-white rounded-xl p-6 border border-slate-200/80 shadow-sm flex items-center justify-between">
          <div>
            <p class="text-xs font-semibold text-slate-500 uppercase tracking-wider">Pending Quotations</p>
            <h3 class="text-3xl font-extrabold text-slate-900 mt-2">{{ summary()?.pendingQuotations ?? 0 }}</h3>
            <p class="text-xs text-amber-600 font-medium mt-1">Proposals awaiting approval</p>
          </div>
          <div class="w-12 h-12 rounded-xl bg-amber-50 text-amber-600 flex items-center justify-center">
            <app-icon name="file-text" [size]="22"></app-icon>
          </div>
        </div>

        <!-- Card 3: Monthly Revenue -->
        <div class="bg-white rounded-xl p-6 border border-slate-200/80 shadow-sm flex items-center justify-between">
          <div>
            <p class="text-xs font-semibold text-slate-500 uppercase tracking-wider">Collected Revenue</p>
            <h3 class="text-2xl font-extrabold text-slate-900 mt-2">{{ summary()?.monthlyRevenue | money }}</h3>
            <p class="text-xs text-emerald-600 font-medium mt-1">Paid this month</p>
          </div>
          <div class="w-12 h-12 rounded-xl bg-emerald-50 text-emerald-600 flex items-center justify-center">
            <app-icon name="dollar-sign" [size]="22"></app-icon>
          </div>
        </div>

        <!-- Card 4: Outstanding Receivables -->
        <div class="bg-white rounded-xl p-6 border border-slate-200/80 shadow-sm flex items-center justify-between">
          <div>
            <p class="text-xs font-semibold text-slate-500 uppercase tracking-wider">Unpaid Invoices</p>
            <h3 class="text-2xl font-extrabold text-slate-900 mt-2">{{ summary()?.outstandingInvoices | money }}</h3>
            <p class="text-xs text-rose-600 font-medium mt-1">Pending customer settlement</p>
          </div>
          <div class="w-12 h-12 rounded-xl bg-rose-50 text-rose-600 flex items-center justify-center">
            <app-icon name="alert-circle" [size]="22"></app-icon>
          </div>
        </div>
      </div>

      <!-- Tables Grid -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <!-- Today's Operations Schedule -->
        <div class="bg-white rounded-xl border border-slate-200/80 shadow-sm overflow-hidden">
          <div class="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
            <div>
              <h3 class="text-base font-semibold text-slate-800">Today's Dispatch Schedule</h3>
              <p class="text-xs text-slate-400">Assigned field teams and on-site locations</p>
            </div>
            <a routerLink="/jobs" class="text-xs font-semibold text-emerald-600 hover:text-emerald-700">View All Jobs &rarr;</a>
          </div>
          <div class="overflow-x-auto">
            <table class="w-full text-left text-sm">
              <thead class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wider border-b border-slate-100">
                <tr>
                  <th class="px-6 py-3">Order #</th>
                  <th class="px-6 py-3">Client</th>
                  <th class="px-6 py-3">Service</th>
                  <th class="px-6 py-3">Status</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100">
                <tr *ngFor="let job of summary()?.todayJobs" class="hover:bg-slate-50/50 transition-colors">
                  <td class="px-6 py-3.5 font-medium text-slate-900 font-mono text-xs">{{ job.number }}</td>
                  <td class="px-6 py-3.5 text-slate-700 font-medium">{{ job.customerName || '—' }}</td>
                  <td class="px-6 py-3.5 text-slate-500 text-xs">{{ job.serviceType }}</td>
                  <td class="px-6 py-3.5"><app-badge [label]="job.status"></app-badge></td>
                </tr>
                <tr *ngIf="!summary()?.todayJobs?.length">
                  <td colspan="4" class="px-6 py-8 text-center text-slate-400 text-xs">No dispatch assignments scheduled for today</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Recent Quotations -->
        <div class="bg-white rounded-xl border border-slate-200/80 shadow-sm overflow-hidden">
          <div class="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
            <div>
              <h3 class="text-base font-semibold text-slate-800">Recent Price Proposals</h3>
              <p class="text-xs text-slate-400">Latest proposals sent to commercial clients</p>
            </div>
            <a routerLink="/quotations" class="text-xs font-semibold text-emerald-600 hover:text-emerald-700">View All &rarr;</a>
          </div>
          <div class="overflow-x-auto">
            <table class="w-full text-left text-sm">
              <thead class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wider border-b border-slate-100">
                <tr>
                  <th class="px-6 py-3">Quote #</th>
                  <th class="px-6 py-3">Client</th>
                  <th class="px-6 py-3">Amount</th>
                  <th class="px-6 py-3">Status</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100">
                <tr *ngFor="let q of summary()?.recentQuotations" class="hover:bg-slate-50/50 transition-colors">
                  <td class="px-6 py-3.5 font-medium text-slate-900 font-mono text-xs">{{ q.number }}</td>
                  <td class="px-6 py-3.5 text-slate-700 font-medium">{{ q.customerName || '—' }}</td>
                  <td class="px-6 py-3.5 font-bold text-slate-900">{{ q.total | money }}</td>
                  <td class="px-6 py-3.5"><app-badge [label]="q.status"></app-badge></td>
                </tr>
                <tr *ngIf="!summary()?.recentQuotations?.length">
                  <td colspan="4" class="px-6 py-8 text-center text-slate-400 text-xs">No recent quotations</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  `
})
export class DashboardComponent implements OnInit {
  private api = inject(ApiService);
  summary = signal<DashboardSummary | null>(null);

  ngOnInit() {
    this.api.get<DashboardSummary>('dashboard').subscribe({
      next: res => this.summary.set(res),
      error: err => console.error('Failed to load dashboard summary', err)
    });
  }
}
