import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { Invoice } from '../../core/models/models';
import { IconComponent } from '../../shared/components/icon.component';
import { BadgeComponent } from '../../shared/components/badge.component';
import { ModalComponent } from '../../shared/components/modal.component';
import { MoneyPipe } from '../../shared/pipes/money.pipe';

@Component({
  selector: 'app-invoices',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent, BadgeComponent, ModalComponent, MoneyPipe],
  template: `
    <div class="space-y-6 animate-fade-in">
      <!-- Header -->
      <div class="flex items-center justify-between">
        <div>
          <h2 class="text-2xl font-bold text-slate-900 tracking-tight">Billing & Receipts</h2>
          <p class="text-sm text-slate-500 mt-1">Customer tax invoices, payment receipt records, and account balances</p>
        </div>
      </div>

      <!-- Invoices Table -->
      <div class="bg-white rounded-xl border border-slate-200/80 shadow-sm overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full text-left text-sm">
            <thead class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wider border-b border-slate-100">
              <tr>
                <th class="px-6 py-3.5">Invoice #</th>
                <th class="px-6 py-3.5">Client</th>
                <th class="px-6 py-3.5">Total Billed</th>
                <th class="px-6 py-3.5">Amount Paid</th>
                <th class="px-6 py-3.5">Remaining Balance</th>
                <th class="px-6 py-3.5">Status</th>
                <th class="px-6 py-3.5 text-right">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr *ngFor="let inv of invoices()" class="hover:bg-slate-50/50 transition-colors">
                <td class="px-6 py-4 font-mono font-medium text-xs text-slate-900">{{ inv.number }}</td>
                <td class="px-6 py-4 font-semibold text-slate-900">{{ inv.customerName || '—' }}</td>
                <td class="px-6 py-4 font-bold text-slate-900">{{ inv.total | money }}</td>
                <td class="px-6 py-4 text-emerald-600 font-semibold">{{ inv.paid | money }}</td>
                <td class="px-6 py-4 font-bold" [ngClass]="inv.balance > 0 ? 'text-rose-600' : 'text-slate-400'">
                  {{ inv.balance | money }}
                </td>
                <td class="px-6 py-4"><app-badge [label]="inv.status"></app-badge></td>
                <td class="px-6 py-4 text-right space-x-2">
                  <button *ngIf="inv.balance > 0" (click)="openPaymentModal(inv)" class="px-3 py-1.5 text-xs font-semibold bg-emerald-50 text-emerald-700 rounded-lg border border-emerald-200 hover:bg-emerald-100 transition-colors">
                    Record Payment
                  </button>
                  <button (click)="downloadPdf(inv.id)" class="px-2.5 py-1.5 text-xs font-medium text-slate-600 hover:text-slate-900 border border-slate-200 rounded-lg bg-white hover:bg-slate-50 transition-colors">
                    Download PDF
                  </button>
                </td>
              </tr>
              <tr *ngIf="!invoices().length">
                <td colspan="7" class="px-6 py-12 text-center text-slate-400 text-sm">
                  No invoices recorded yet. Completed jobs automatically generate an invoice here.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Record Payment Modal -->
      <app-modal [isOpen]="isPaymentOpen()" title="Record Customer Settlement" (close)="isPaymentOpen.set(false)">
        <form (ngSubmit)="submitPayment()" class="space-y-4">
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Payment Received (QAR) *</label>
            <input type="number" [(ngModel)]="paymentForm.amount" name="amount" required min="1" [max]="selectedInvoice?.balance || 99999" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:outline-none focus:border-emerald-500" />
            <p class="text-[11px] text-slate-500 mt-1">Pending balance: {{ selectedInvoice?.balance | money }}</p>
          </div>
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Settlement Method</label>
            <select [(ngModel)]="paymentForm.method" name="method" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:outline-none focus:border-emerald-500">
              <option value="BANK_TRANSFER">Bank Wire / Electronic Transfer</option>
              <option value="CASH">Cash Drawer</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Bank Reference / Cheque #</label>
            <input type="text" [(ngModel)]="paymentForm.reference" name="reference" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm focus:outline-none focus:border-emerald-500" placeholder="e.g. QNB-FT-991823" />
          </div>
          <div class="flex justify-end gap-3 pt-4 border-t border-slate-100">
            <button type="button" (click)="isPaymentOpen.set(false)" class="px-4 py-2 text-sm text-slate-600 font-medium">Cancel</button>
            <button type="submit" class="px-5 py-2 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-semibold shadow-sm">Save & Issue Receipt</button>
          </div>
        </form>
      </app-modal>
    </div>
  `
})
export class InvoicesComponent implements OnInit {
  private api = inject(ApiService);
  invoices = signal<Invoice[]>([]);

  isPaymentOpen = signal(false);
  selectedInvoice: Invoice | null = null;

  paymentForm = {
    amount: 0,
    method: 'BANK_TRANSFER' as const,
    reference: ''
  };

  ngOnInit() {
    this.loadInvoices();
  }

  loadInvoices() {
    this.api.get<Invoice[]>('invoices').subscribe({
      next: res => this.invoices.set(res),
      error: err => console.error(err)
    });
  }

  openPaymentModal(inv: Invoice) {
    this.selectedInvoice = inv;
    this.paymentForm.amount = inv.balance;
    this.isPaymentOpen.set(true);
  }

  submitPayment() {
    if (!this.selectedInvoice) return;
    this.api.post(`invoices/${this.selectedInvoice.id}/payments`, this.paymentForm).subscribe({
      next: () => {
        this.isPaymentOpen.set(false);
        this.loadInvoices();
      },
      error: err => alert(err.error?.detail || 'Failed to record payment')
    });
  }

  downloadPdf(id: number) {
    this.api.downloadBlob(`documents/invoice/${id}/pdf`).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Invoice-${id}.pdf`;
      a.click();
    });
  }
}
