import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { Supplier, Requisition, Lpo } from '../../core/models/models';
import { IconComponent } from '../../shared/components/icon.component';
import { BadgeComponent } from '../../shared/components/badge.component';
import { ModalComponent } from '../../shared/components/modal.component';
import { MoneyPipe } from '../../shared/pipes/money.pipe';

@Component({
  selector: 'app-purchasing',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent, BadgeComponent, ModalComponent, MoneyPipe],
  template: `
    <div class="space-y-6 animate-fade-in">
      <!-- Header -->
      <div class="flex items-center justify-between">
        <div>
          <h2 class="text-2xl font-bold text-slate-900 tracking-tight">Purchasing & LPOs</h2>
          <p class="text-sm text-slate-500 mt-1">Vendor requisitions, Local Purchase Orders, and supplier contracts</p>
        </div>
        <div class="flex gap-2">
          <button (click)="isSupplierOpen.set(true)" class="px-3.5 py-2 rounded-lg border border-slate-300 text-sm font-semibold text-slate-700 bg-white hover:bg-slate-50">
            + New Supplier
          </button>
          <button (click)="isReqOpen.set(true)" class="px-4 py-2 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-semibold shadow-sm">
            + New Requisition
          </button>
        </div>
      </div>

      <!-- Requisitions Table -->
      <div class="bg-white rounded-xl border border-slate-200/80 shadow-sm overflow-hidden">
        <div class="px-6 py-4 border-b border-slate-100">
          <h3 class="text-base font-semibold text-slate-800">Material Requisitions</h3>
        </div>
        <div class="overflow-x-auto">
          <table class="w-full text-left text-sm">
            <thead class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wider border-b border-slate-100">
              <tr>
                <th class="px-6 py-3.5">Req #</th>
                <th class="px-6 py-3.5">Description</th>
                <th class="px-6 py-3.5">Quantity</th>
                <th class="px-6 py-3.5">Department</th>
                <th class="px-6 py-3.5">Status</th>
                <th class="px-6 py-3.5 text-right">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr *ngFor="let r of requisitions()" class="hover:bg-slate-50/50 transition-colors">
                <td class="px-6 py-4 font-mono font-medium text-xs text-slate-900">{{ r.number }}</td>
                <td class="px-6 py-4 font-semibold text-slate-900">{{ r.description }}</td>
                <td class="px-6 py-4 text-slate-600">{{ r.qty }}</td>
                <td class="px-6 py-4 text-slate-600">{{ r.department || 'Operations' }}</td>
                <td class="px-6 py-4"><app-badge [label]="r.status"></app-badge></td>
                <td class="px-6 py-4 text-right">
                  <button *ngIf="r.status === 'DRAFT'" (click)="approveReq(r.id)" class="px-2.5 py-1 text-xs font-semibold bg-blue-50 text-blue-700 rounded border border-blue-200 hover:bg-blue-100">
                    Approve
                  </button>
                </td>
              </tr>
              <tr *ngIf="!requisitions().length">
                <td colspan="6" class="px-6 py-8 text-center text-slate-400 text-sm">
                  No purchase requisitions yet.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Add Supplier Modal -->
      <app-modal [isOpen]="isSupplierOpen()" title="Register New Supplier" (close)="isSupplierOpen.set(false)">
        <form (ngSubmit)="saveSupplier()" class="space-y-4">
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Supplier / Vendor Name *</label>
            <input type="text" [(ngModel)]="newSupplier.name" name="name" required class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm" placeholder="e.g. Qatar Chemical Supplies Co." />
          </div>
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Phone Number</label>
            <input type="text" [(ngModel)]="newSupplier.phone" name="phone" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm" placeholder="+974 4433 2211" />
          </div>
          <div class="flex justify-end gap-3 pt-4 border-t border-slate-100">
            <button type="button" (click)="isSupplierOpen.set(false)" class="px-4 py-2 text-sm text-slate-600 font-medium">Cancel</button>
            <button type="submit" class="px-5 py-2 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-semibold">Save Supplier</button>
          </div>
        </form>
      </app-modal>

      <!-- Add Requisition Modal -->
      <app-modal [isOpen]="isReqOpen()" title="New Material Requisition" (close)="isReqOpen.set(false)">
        <form (ngSubmit)="saveRequisition()" class="space-y-4">
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Material / Supply Description *</label>
            <input type="text" [(ngModel)]="newReq.description" name="description" required class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm" placeholder="e.g. 50x Heavy Duty Microfiber Towels" />
          </div>
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-xs font-semibold text-slate-700 mb-1">Quantity Needed</label>
              <input type="number" [(ngModel)]="newReq.qty" name="qty" min="1" required class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-700 mb-1">Department</label>
              <input type="text" [(ngModel)]="newReq.department" name="department" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm" />
            </div>
          </div>
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Reason for Request</label>
            <input type="text" [(ngModel)]="newReq.reason" name="reason" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm" placeholder="Depleted stock for villa contracts" />
          </div>
          <div class="flex justify-end gap-3 pt-4 border-t border-slate-100">
            <button type="button" (click)="isReqOpen.set(false)" class="px-4 py-2 text-sm text-slate-600 font-medium">Cancel</button>
            <button type="submit" class="px-5 py-2 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-semibold">Submit Requisition</button>
          </div>
        </form>
      </app-modal>
    </div>
  `
})
export class PurchasingComponent implements OnInit {
  private api = inject(ApiService);
  suppliers = signal<Supplier[]>([]);
  requisitions = signal<Requisition[]>([]);

  isSupplierOpen = signal(false);
  isReqOpen = signal(false);

  newSupplier = { name: '', phone: '' };
  newReq = { description: '', qty: 10, department: 'Operations', reason: '' };

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.api.get<Supplier[]>('suppliers').subscribe(res => this.suppliers.set(res));
    this.api.get<Requisition[]>('requisitions').subscribe(res => this.requisitions.set(res));
  }

  saveSupplier() {
    this.api.post<Supplier>('suppliers', this.newSupplier).subscribe({
      next: () => {
        this.isSupplierOpen.set(false);
        this.newSupplier = { name: '', phone: '' };
        this.loadData();
      },
      error: err => alert(err.error?.detail || 'Failed to save supplier')
    });
  }

  saveRequisition() {
    this.api.post<Requisition>('requisitions', this.newReq).subscribe({
      next: () => {
        this.isReqOpen.set(false);
        this.newReq = { description: '', qty: 10, department: 'Operations', reason: '' };
        this.loadData();
      },
      error: err => alert(err.error?.detail || 'Failed to create requisition')
    });
  }

  approveReq(id: number) {
    this.api.post(`requisitions/${id}/approve`, {}).subscribe({
      next: () => this.loadData(),
      error: err => alert(err.error?.detail || 'Failed to approve requisition')
    });
  }
}
