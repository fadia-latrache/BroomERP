import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { Customer } from '../../core/models/models';
import { IconComponent } from '../../shared/components/icon.component';
import { ModalComponent } from '../../shared/components/modal.component';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent, ModalComponent],
  template: `
    <div class="space-y-6 animate-fade-in">
      <!-- Header -->
      <div class="flex items-center justify-between">
        <div>
          <h2 class="text-2xl font-bold text-slate-900 tracking-tight">Customer Accounts</h2>
          <p class="text-sm text-slate-500 mt-1">Directory of commercial clients, residential compounds, and contact persons</p>
        </div>
        <button
          (click)="isCreateOpen.set(true)"
          class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-semibold shadow-sm transition-colors"
        >
          <app-icon name="plus" [size]="16"></app-icon>
          <span>Add Customer</span>
        </button>
      </div>

      <!-- Search Bar -->
      <div class="bg-white p-3.5 rounded-xl border border-slate-200/80 shadow-sm flex items-center gap-3">
        <span class="text-slate-400 pl-2">
          <app-icon name="search" [size]="16"></app-icon>
        </span>
        <input
          type="text"
          [(ngModel)]="searchQuery"
          (ngModelChange)="loadCustomers()"
          placeholder="Filter by customer name, phone, or location..."
          class="w-full text-sm text-slate-800 focus:outline-none placeholder-slate-400"
        />
      </div>

      <!-- Customers Table -->
      <div class="bg-white rounded-xl border border-slate-200/80 shadow-sm overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full text-left text-sm">
            <thead class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wider border-b border-slate-100">
              <tr>
                <th class="px-6 py-3.5">Account #</th>
                <th class="px-6 py-3.5">Customer / Company</th>
                <th class="px-6 py-3.5">Phone</th>
                <th class="px-6 py-3.5">Email</th>
                <th class="px-6 py-3.5">Address / Location</th>
                <th class="px-6 py-3.5">Status</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr *ngFor="let c of customers()" class="hover:bg-slate-50/50 transition-colors">
                <td class="px-6 py-4 font-mono font-medium text-xs text-slate-500">{{ c.number || '#' + c.id }}</td>
                <td class="px-6 py-4 font-semibold text-slate-900">{{ c.name }}</td>
                <td class="px-6 py-4 text-slate-600 text-xs font-mono">{{ c.phone || '—' }}</td>
                <td class="px-6 py-4 text-slate-600 text-xs">{{ c.email || '—' }}</td>
                <td class="px-6 py-4 text-slate-600 text-xs truncate max-w-xs">{{ c.address || '—' }}</td>
                <td class="px-6 py-4">
                  <span [ngClass]="c.active ? 'bg-emerald-50 text-emerald-700 border-emerald-200' : 'bg-slate-100 text-slate-600 border-slate-200'" class="px-2.5 py-0.5 rounded text-xs font-semibold border">
                    {{ c.active ? 'Active' : 'Inactive' }}
                  </span>
                </td>
              </tr>
              <tr *ngIf="!customers().length">
                <td colspan="6" class="px-6 py-12 text-center text-slate-400 text-sm">
                  No customers found. Click "Add Customer" to register a new account.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Add Customer Modal -->
      <app-modal [isOpen]="isCreateOpen()" title="Register New Customer Account" (close)="isCreateOpen.set(false)">
        <form (ngSubmit)="saveCustomer()" class="space-y-4">
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Company / Customer Name *</label>
            <input type="text" [(ngModel)]="newCustomer.name" name="name" required class="w-full px-3 py-2 rounded-lg border border-slate-300 text-sm focus:outline-none focus:border-emerald-500" placeholder="e.g. Lusail Commercial Tower" />
          </div>
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-xs font-semibold text-slate-700 mb-1">Primary Phone</label>
              <input type="text" [(ngModel)]="newCustomer.phone" name="phone" class="w-full px-3 py-2 rounded-lg border border-slate-300 text-sm focus:outline-none focus:border-emerald-500" placeholder="+974 4455 6677" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-700 mb-1">Billing Email</label>
              <input type="email" [(ngModel)]="newCustomer.email" name="email" class="w-full px-3 py-2 rounded-lg border border-slate-300 text-sm focus:outline-none focus:border-emerald-500" placeholder="accounts@company.qa" />
            </div>
          </div>
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Site Location / Address</label>
            <textarea [(ngModel)]="newCustomer.address" name="address" rows="2" class="w-full px-3 py-2 rounded-lg border border-slate-300 text-sm focus:outline-none focus:border-emerald-500" placeholder="Street, Building, Zone, City"></textarea>
          </div>
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Special Notes / Access Gate Instructions</label>
            <textarea [(ngModel)]="newCustomer.notes" name="notes" rows="2" class="w-full px-3 py-2 rounded-lg border border-slate-300 text-sm focus:outline-none focus:border-emerald-500" placeholder="Key pickup, security contact, or gate code..."></textarea>
          </div>
          <div class="flex justify-end gap-3 pt-4 border-t border-slate-100">
            <button type="button" (click)="isCreateOpen.set(false)" class="px-4 py-2 rounded-lg text-sm text-slate-600 hover:bg-slate-100 font-medium">Cancel</button>
            <button type="submit" class="px-5 py-2 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-semibold shadow-sm">Save Account</button>
          </div>
        </form>
      </app-modal>
    </div>
  `
})
export class CustomersComponent implements OnInit {
  private api = inject(ApiService);
  customers = signal<Customer[]>([]);
  searchQuery = '';
  isCreateOpen = signal(false);

  newCustomer = {
    name: '',
    phone: '',
    email: '',
    address: '',
    notes: ''
  };

  ngOnInit() {
    this.loadCustomers();
  }

  loadCustomers() {
    this.api.get<Customer[]>('customers', { search: this.searchQuery }).subscribe({
      next: res => this.customers.set(res),
      error: err => console.error(err)
    });
  }

  saveCustomer() {
    this.api.post<Customer>('customers', this.newCustomer).subscribe({
      next: () => {
        this.isCreateOpen.set(false);
        this.newCustomer = { name: '', phone: '', email: '', address: '', notes: '' };
        this.loadCustomers();
      },
      error: err => alert(err.error?.detail || 'Failed to create customer')
    });
  }
}
