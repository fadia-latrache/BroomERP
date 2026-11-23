import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../core/services/api.service';
import { ReportSummary } from '../../core/models/models';
import { IconComponent } from '../../shared/components/icon.component';
import { MoneyPipe } from '../../shared/pipes/money.pipe';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, IconComponent, MoneyPipe],
  template: `
    <div class="space-y-8 animate-fade-in">
      <div>
        <h2 class="text-2xl font-bold text-slate-900 tracking-tight">Financial & Operations Reports</h2>
        <p class="text-sm text-slate-500 mt-1">Aggregated revenue, expense analysis, and job performance KPIs</p>
      </div>

      <!-- Financial Metric Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-6">
        <div class="bg-white p-6 rounded-xl border border-slate-200 shadow-sm">
          <p class="text-xs font-semibold text-slate-500 uppercase">Total Billed</p>
          <h3 class="text-2xl font-bold text-slate-900 mt-2">{{ report()?.totalRevenue | money }}</h3>
        </div>
        <div class="bg-white p-6 rounded-xl border border-slate-200 shadow-sm">
          <p class="text-xs font-semibold text-slate-500 uppercase">Total Collected</p>
          <h3 class="text-2xl font-bold text-emerald-600 mt-2">{{ report()?.totalCollected | money }}</h3>
        </div>
        <div class="bg-white p-6 rounded-xl border border-slate-200 shadow-sm">
          <p class="text-xs font-semibold text-slate-500 uppercase">Outstanding Balance</p>
          <h3 class="text-2xl font-bold text-rose-600 mt-2">{{ report()?.totalOutstanding | money }}</h3>
        </div>
        <div class="bg-white p-6 rounded-xl border border-slate-200 shadow-sm">
          <p class="text-xs font-semibold text-slate-500 uppercase">Operating Expenses</p>
          <h3 class="text-2xl font-bold text-amber-600 mt-2">{{ report()?.totalExpenses | money }}</h3>
        </div>
      </div>

      <!-- Operations KPI Summary -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div class="bg-white p-6 rounded-xl border border-slate-200 shadow-sm">
          <p class="text-xs font-semibold text-slate-500 uppercase">Total Completed Jobs</p>
          <h3 class="text-3xl font-extrabold text-slate-900 mt-2">{{ report()?.completedJobs ?? 0 }}</h3>
          <p class="text-xs text-slate-400 mt-1">out of {{ report()?.totalJobs ?? 0 }} scheduled</p>
        </div>
        <div class="bg-white p-6 rounded-xl border border-slate-200 shadow-sm">
          <p class="text-xs font-semibold text-slate-500 uppercase">Cancelled Jobs</p>
          <h3 class="text-3xl font-extrabold text-slate-900 mt-2">{{ report()?.cancelledJobs ?? 0 }}</h3>
          <p class="text-xs text-slate-400 mt-1">Customer / internal cancellations</p>
        </div>
        <div class="bg-white p-6 rounded-xl border border-slate-200 shadow-sm">
          <p class="text-xs font-semibold text-slate-500 uppercase">Completion Rate</p>
          <h3 class="text-3xl font-extrabold text-emerald-600 mt-2">
            {{ ((report()?.completedJobs || 0) / Math.max(1, report()?.totalJobs || 1) * 100).toFixed(0) }}%
          </h3>
          <p class="text-xs text-slate-400 mt-1">Service fulfillment efficiency</p>
        </div>
      </div>

      <!-- Monthly Sales Breakdown -->
      <div class="bg-white p-6 rounded-xl border border-slate-200 shadow-sm">
        <h3 class="text-base font-semibold text-slate-800 mb-4">Monthly Revenue Summary</h3>
        <div class="space-y-3">
          <div *ngFor="let m of report()?.monthlySales" class="flex items-center justify-between text-sm py-2 border-b border-slate-100">
            <span class="font-medium text-slate-700">{{ m.month }}</span>
            <span class="font-bold text-slate-900">{{ m.total | money }}</span>
          </div>
          <div *ngIf="!report()?.monthlySales?.length" class="text-center py-6 text-slate-400 text-xs">
            No sales data recorded yet.
          </div>
        </div>
      </div>
    </div>
  `
})
export class ReportsComponent implements OnInit {
  private api = inject(ApiService);
  report = signal<ReportSummary | null>(null);
  Math = Math;

  ngOnInit() {
    this.api.get<ReportSummary>('reports/summary').subscribe({
      next: res => this.report.set(res),
      error: err => console.error(err)
    });
  }
}
