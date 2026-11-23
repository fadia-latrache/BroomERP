import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { Quotation, Customer, QuotationLine } from '../../core/models/models';
import { IconComponent } from '../../shared/components/icon.component';
import { BadgeComponent } from '../../shared/components/badge.component';
import { ModalComponent } from '../../shared/components/modal.component';
import { MoneyPipe } from '../../shared/pipes/money.pipe';

@Component({
  selector: 'app-quotations',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent, BadgeComponent, ModalComponent, MoneyPipe],
  template: `
    <div class="space-y-6 animate-fade-in">
      <!-- Header -->
      <div class="flex items-center justify-between">
        <div>
          <h2 class="text-2xl font-bold text-slate-900 tracking-tight">Quotations</h2>
          <p class="text-sm text-slate-500 mt-1">Manage price estimates, approvals, and contract proposals</p>
        </div>
        <button
          (click)="openCreateModal()"
          class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-semibold shadow-sm transition-colors"
        >
          <app-icon name="plus" [size]="16"></app-icon>
          <span>New Quotation</span>
        </button>
      </div>

      <!-- Table -->
      <div class="bg-white rounded-xl border border-slate-200/80 shadow-sm overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full text-left text-sm">
            <thead class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wider border-b border-slate-100">
              <tr>
                <th class="px-6 py-3.5">Quotation #</th>
                <th class="px-6 py-3.5">Customer</th>
                <th class="px-6 py-3.5">Total Amount</th>
                <th class="px-6 py-3.5">Status</th>
                <th class="px-6 py-3.5">Created</th>
                <th class="px-6 py-3.5 text-right">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr *ngFor="let q of quotations()" class="hover:bg-slate-50/50 transition-colors">
                <td class="px-6 py-4 font-mono font-medium text-xs text-slate-900">{{ q.number }}</td>
                <td class="px-6 py-4 font-semibold text-slate-900">{{ q.customerName || '—' }}</td>
                <td class="px-6 py-4 font-bold text-slate-900">{{ q.total | money }}</td>
                <td class="px-6 py-4"><app-badge [label]="q.status"></app-badge></td>
                <td class="px-6 py-4 text-xs text-slate-500">{{ q.createdAt | date:'mediumDate' }}</td>
                <td class="px-6 py-4 text-right space-x-2">
                  <button *ngIf="q.status === 'PENDING_APPROVAL'" (click)="approve(q.id)" class="px-2.5 py-1 text-xs font-semibold bg-emerald-50 text-emerald-700 rounded border border-emerald-200 hover:bg-emerald-100">
                    Approve
                  </button>
                  <button *ngIf="q.status === 'APPROVED'" (click)="openAcceptModal(q)" class="px-2.5 py-1 text-xs font-semibold bg-blue-50 text-blue-700 rounded border border-blue-200 hover:bg-blue-100">
                    Convert to Job
                  </button>
                </td>
              </tr>
              <tr *ngIf="!quotations().length">
                <td colspan="6" class="px-6 py-12 text-center text-slate-400 text-sm">
                  No quotations found.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Create Quotation Modal -->
      <app-modal [isOpen]="isCreateOpen()" title="Create New Quotation" (close)="isCreateOpen.set(false)">
        <form (ngSubmit)="saveQuotation()" class="space-y-4">
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Customer *</label>
            <select [(ngModel)]="newQuotation.customerId" name="customerId" required class="w-full px-3 py-2 rounded-lg border border-slate-300 text-sm">
              <option value="0" disabled>Select Customer...</option>
              <option *ngFor="let c of customers()" [value]="c.id">{{ c.name }}</option>
            </select>
          </div>

          <!-- Line Items -->
          <div>
            <div class="flex items-center justify-between mb-2">
              <label class="block text-xs font-semibold text-slate-700">Line Items</label>
              <button type="button" (click)="addLine()" class="text-xs text-emerald-600 hover:text-emerald-700 font-semibold">+ Add Service Line</button>
            </div>
            <div class="space-y-2 max-h-48 overflow-y-auto pr-1">
              <div *ngFor="let line of newQuotation.lines; let i = index" class="grid grid-cols-12 gap-2 items-center bg-slate-50 p-2 rounded-lg border border-slate-200">
                <div class="col-span-5">
                  <input type="text" [(ngModel)]="line.description" [name]="'desc_' + i" placeholder="Service description" class="w-full px-2 py-1 text-xs border border-slate-300 rounded" required />
                </div>
                <div class="col-span-2">
                  <input type="number" [(ngModel)]="line.qty" [name]="'qty_' + i" placeholder="Qty" min="1" class="w-full px-2 py-1 text-xs border border-slate-300 rounded" required />
                </div>
                <div class="col-span-3">
                  <input type="number" [(ngModel)]="line.unitPrice" [name]="'price_' + i" placeholder="Rate (QAR)" min="0" class="w-full px-2 py-1 text-xs border border-slate-300 rounded" required />
                </div>
                <div class="col-span-2 text-right">
                  <button type="button" (click)="removeLine(i)" class="text-rose-500 hover:text-rose-700 p-1">
                    <app-icon name="x" [size]="14"></app-icon>
                  </button>
                </div>
              </div>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-xs font-semibold text-slate-700 mb-1">Discount (%)</label>
              <input type="number" [(ngModel)]="newQuotation.discountPct" name="discountPct" min="0" max="100" class="w-full px-3 py-2 rounded-lg border border-slate-300 text-sm" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-700 mb-1">Billing Basis</label>
              <select [(ngModel)]="newQuotation.basis" name="basis" class="w-full px-3 py-2 rounded-lg border border-slate-300 text-sm">
                <option value="DAILY">Daily</option>
                <option value="HOURLY">Hourly</option>
              </select>
            </div>
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Service Address / Location</label>
            <input type="text" [(ngModel)]="newQuotation.serviceAddress" name="serviceAddress" placeholder="Optional specific site address" class="w-full px-3 py-2 rounded-lg border border-slate-300 text-sm" />
          </div>

          <div class="flex justify-end gap-3 pt-4 border-t border-slate-100">
            <button type="button" (click)="isCreateOpen.set(false)" class="px-4 py-2 text-sm text-slate-600 font-medium">Cancel</button>
            <button type="submit" class="px-5 py-2 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-semibold">Generate Quotation</button>
          </div>
        </form>
      </app-modal>

      <!-- Convert to Job Modal -->
      <app-modal [isOpen]="isAcceptOpen()" title="Convert Quotation to Confirmed Job" (close)="isAcceptOpen.set(false)">
        <form (ngSubmit)="confirmAccept()" class="space-y-4">
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Service Type *</label>
            <input type="text" [(ngModel)]="acceptForm.serviceType" name="serviceType" required class="w-full px-3 py-2 rounded-lg border border-slate-300 text-sm" placeholder="DEEP_CLEANING" />
          </div>
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Scheduled Date *</label>
            <input type="date" [(ngModel)]="acceptForm.scheduledFor" name="scheduledFor" required class="w-full px-3 py-2 rounded-lg border border-slate-300 text-sm" />
          </div>
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Start Time</label>
            <input type="time" [(ngModel)]="acceptForm.startTime" name="startTime" class="w-full px-3 py-2 rounded-lg border border-slate-300 text-sm" />
          </div>
          <div class="flex justify-end gap-3 pt-4 border-t border-slate-100">
            <button type="button" (click)="isAcceptOpen.set(false)" class="px-4 py-2 text-sm text-slate-600 font-medium">Cancel</button>
            <button type="submit" class="px-5 py-2 rounded-lg bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold">Confirm Job</button>
          </div>
        </form>
      </app-modal>
    </div>
  `
})
export class QuotationsComponent implements OnInit {
  private api = inject(ApiService);
  quotations = signal<Quotation[]>([]);
  customers = signal<Customer[]>([]);

  isCreateOpen = signal(false);
  isAcceptOpen = signal(false);
  selectedQuoteId = 0;

  newQuotation = {
    customerId: 0,
    lines: [{ description: 'Deep Cleaning Service', qty: 1, unitPrice: 350, days: 1, location: '' }],
    discountPct: 0,
    basis: 'DAILY',
    serviceAddress: ''
  };

  acceptForm = {
    serviceType: 'DEEP_CLEANING',
    scheduledFor: new Date().toISOString().slice(0, 10),
    startTime: '08:00'
  };

  ngOnInit() {
    this.loadQuotations();
    this.api.get<Customer[]>('customers').subscribe(res => this.customers.set(res));
  }

  loadQuotations() {
    this.api.get<Quotation[]>('quotations').subscribe({
      next: res => this.quotations.set(res),
      error: err => console.error(err)
    });
  }

  openCreateModal() {
    if (this.customers().length > 0 && this.newQuotation.customerId === 0) {
      this.newQuotation.customerId = this.customers()[0].id;
    }
    this.isCreateOpen.set(true);
  }

  addLine() {
    this.newQuotation.lines.push({ description: '', qty: 1, unitPrice: 0, days: 1, location: '' });
  }

  removeLine(index: number) {
    if (this.newQuotation.lines.length > 1) {
      this.newQuotation.lines.splice(index, 1);
    }
  }

  saveQuotation() {
    this.api.post<Quotation>('quotations', this.newQuotation).subscribe({
      next: () => {
        this.isCreateOpen.set(false);
        this.loadQuotations();
      },
      error: err => alert(err.error?.detail || 'Failed to create quotation')
    });
  }

  approve(id: number) {
    this.api.post(`quotations/${id}/approve`, {}).subscribe({
      next: () => this.loadQuotations(),
      error: err => alert(err.error?.detail || 'Failed to approve quotation')
    });
  }

  openAcceptModal(q: Quotation) {
    this.selectedQuoteId = q.id;
    this.isAcceptOpen.set(true);
  }

  confirmAccept() {
    this.api.post(`quotations/${this.selectedQuoteId}/accept`, this.acceptForm).subscribe({
      next: () => {
        this.isAcceptOpen.set(false);
        this.loadQuotations();
      },
      error: err => alert(err.error?.detail || 'Failed to convert quotation')
    });
  }
}
