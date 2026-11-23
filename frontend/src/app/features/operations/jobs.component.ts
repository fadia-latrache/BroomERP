import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { Job } from '../../core/models/models';
import { IconComponent } from '../../shared/components/icon.component';
import { BadgeComponent } from '../../shared/components/badge.component';
import { ModalComponent } from '../../shared/components/modal.component';
import { MoneyPipe } from '../../shared/pipes/money.pipe';

@Component({
  selector: 'app-jobs',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent, BadgeComponent, ModalComponent, MoneyPipe],
  template: `
    <div class="space-y-6 animate-fade-in">
      <!-- Header -->
      <div class="flex items-center justify-between">
        <div>
          <h2 class="text-2xl font-bold text-slate-900 tracking-tight">Operations & Dispatch</h2>
          <p class="text-sm text-slate-500 mt-1">Field job scheduling, live status execution, and supervisor sign-offs</p>
        </div>
      </div>

      <!-- Jobs Table -->
      <div class="bg-white rounded-xl border border-slate-200/80 shadow-sm overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full text-left text-sm">
            <thead class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wider border-b border-slate-100">
              <tr>
                <th class="px-6 py-3.5">Job #</th>
                <th class="px-6 py-3.5">Client & Location</th>
                <th class="px-6 py-3.5">Service Type</th>
                <th class="px-6 py-3.5">Scheduled Date & Time</th>
                <th class="px-6 py-3.5">Amount</th>
                <th class="px-6 py-3.5">Status</th>
                <th class="px-6 py-3.5 text-right">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr *ngFor="let j of jobs()" class="hover:bg-slate-50/50 transition-colors">
                <td class="px-6 py-4 font-mono font-medium text-xs text-slate-900">{{ j.number }}</td>
                <td class="px-6 py-4">
                  <div class="font-semibold text-slate-900">{{ j.customerName || '—' }}</div>
                  <div class="text-xs text-slate-500 truncate max-w-xs mt-0.5">{{ j.address }}</div>
                </td>
                <td class="px-6 py-4 text-slate-700 font-medium text-xs">{{ j.serviceType }}</td>
                <td class="px-6 py-4 text-xs text-slate-600">
                  <span class="font-medium text-slate-800">{{ j.scheduledFor }}</span>
                  <span *ngIf="j.startTime" class="text-slate-500"> · {{ j.startTime }}</span>
                </td>
                <td class="px-6 py-4 font-bold text-slate-900">{{ j.amount | money }}</td>
                <td class="px-6 py-4"><app-badge [label]="j.status"></app-badge></td>
                <td class="px-6 py-4 text-right space-x-2">
                  <button *ngIf="j.status === 'PLANNED'" (click)="startJob(j.id)" class="px-3 py-1.5 text-xs font-semibold bg-blue-50 text-blue-700 rounded-lg border border-blue-200 hover:bg-blue-100 transition-colors">
                    Start Shift
                  </button>
                  <button *ngIf="j.status === 'IN_PROGRESS'" (click)="openCompleteModal(j)" class="px-3 py-1.5 text-xs font-semibold bg-emerald-50 text-emerald-700 rounded-lg border border-emerald-200 hover:bg-emerald-100 transition-colors">
                    Mark Completed
                  </button>
                  <button *ngIf="j.status !== 'COMPLETED' && j.status !== 'CANCELLED'" (click)="cancelJob(j.id)" class="px-2 py-1 text-xs text-rose-500 hover:text-rose-700">
                    Cancel
                  </button>
                </td>
              </tr>
              <tr *ngIf="!jobs().length">
                <td colspan="7" class="px-6 py-12 text-center text-slate-400 text-sm">
                  No work orders found. Accepted quotations automatically appear here as planned jobs.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Completion Report Modal -->
      <app-modal [isOpen]="isCompleteOpen()" title="Service Completion & Sign-off" (close)="isCompleteOpen.set(false)">
        <form (ngSubmit)="submitCompletion()" class="space-y-4">
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Work Done Summary *</label>
            <textarea [(ngModel)]="completionForm.workDone" name="workDone" required rows="3" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:outline-none focus:border-emerald-500" placeholder="Summary of areas cleaned, polished, or treated..."></textarea>
          </div>
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Service Completion Level</label>
            <select [(ngModel)]="completionForm.completionStatus" name="completionStatus" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm">
              <option value="FULL">100% Full Completion (Client Inspected & Signed)</option>
              <option value="PARTIAL">Partial Completion (Pending Follow-up Visit)</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Client Feedback / Any Issues</label>
            <input type="text" [(ngModel)]="completionForm.complaint" name="complaint" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:outline-none focus:border-emerald-500" placeholder="e.g. None / Satisfied sign-off received" />
          </div>
          <div class="flex justify-end gap-3 pt-4 border-t border-slate-100">
            <button type="button" (click)="isCompleteOpen.set(false)" class="px-4 py-2 text-sm text-slate-600 font-medium">Cancel</button>
            <button type="submit" class="px-5 py-2 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-semibold shadow-sm">Complete Job & Issue Invoice</button>
          </div>
        </form>
      </app-modal>
    </div>
  `
})
export class JobsComponent implements OnInit {
  private api = inject(ApiService);
  jobs = signal<Job[]>([]);

  isCompleteOpen = signal(false);
  selectedJobId = 0;

  completionForm = {
    workDone: 'All scheduled deep cleaning, floor scrubbing, and surface disinfection completed to satisfaction.',
    completionStatus: 'FULL',
    complaint: ''
  };

  ngOnInit() {
    this.loadJobs();
  }

  loadJobs() {
    this.api.get<Job[]>('jobs').subscribe({
      next: res => this.jobs.set(res),
      error: err => console.error(err)
    });
  }

  startJob(id: number) {
    this.api.post(`jobs/${id}/status`, { status: 'IN_PROGRESS' }).subscribe({
      next: () => this.loadJobs(),
      error: err => alert(err.error?.detail || 'Failed to start job')
    });
  }

  openCompleteModal(j: Job) {
    this.selectedJobId = j.id;
    this.isCompleteOpen.set(true);
  }

  submitCompletion() {
    this.api.post(`jobs/${this.selectedJobId}/complete`, this.completionForm).subscribe({
      next: () => {
        this.isCompleteOpen.set(false);
        this.loadJobs();
      },
      error: err => alert(err.error?.detail || 'Failed to complete job')
    });
  }

  cancelJob(id: number) {
    if (confirm('Cancel this work order?')) {
      this.api.post(`jobs/${id}/cancel`, { reason: 'Cancelled by operations supervisor' }).subscribe({
        next: () => this.loadJobs(),
        error: err => alert(err.error?.detail || 'Failed to cancel job')
      });
    }
  }
}
