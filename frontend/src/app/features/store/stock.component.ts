import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { StockItem } from '../../core/models/models';
import { IconComponent } from '../../shared/components/icon.component';
import { ModalComponent } from '../../shared/components/modal.component';

@Component({
  selector: 'app-stock',
  standalone: true,
  imports: [CommonModule, FormsModule, IconComponent, ModalComponent],
  template: `
    <div class="space-y-6 animate-fade-in">
      <!-- Header -->
      <div class="flex items-center justify-between">
        <div>
          <h2 class="text-2xl font-bold text-slate-900 tracking-tight">Inventory & Stock</h2>
          <p class="text-sm text-slate-500 mt-1">Cleaning chemicals, equipment accessories, and supplies tracking</p>
        </div>
        <button
          (click)="isCreateOpen.set(true)"
          class="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-semibold shadow-sm transition-colors"
        >
          <app-icon name="plus" [size]="16"></app-icon>
          <span>New Stock Item</span>
        </button>
      </div>

      <!-- Table -->
      <div class="bg-white rounded-xl border border-slate-200/80 shadow-sm overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full text-left text-sm">
            <thead class="bg-slate-50 text-slate-500 text-xs font-semibold uppercase tracking-wider border-b border-slate-100">
              <tr>
                <th class="px-6 py-3.5">Item Name</th>
                <th class="px-6 py-3.5">Category</th>
                <th class="px-6 py-3.5">Current Qty</th>
                <th class="px-6 py-3.5">Reorder Level</th>
                <th class="px-6 py-3.5">Status</th>
                <th class="px-6 py-3.5 text-right">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr *ngFor="let s of stock()" class="hover:bg-slate-50/50 transition-colors">
                <td class="px-6 py-4 font-semibold text-slate-900">{{ s.name }}</td>
                <td class="px-6 py-4 text-slate-600">{{ s.category }}</td>
                <td class="px-6 py-4 font-bold text-slate-900">{{ s.qty }} {{ s.unit }}</td>
                <td class="px-6 py-4 text-slate-500">{{ s.reorderLevel }} {{ s.unit }}</td>
                <td class="px-6 py-4">
                  <span *ngIf="s.low" class="px-2 py-0.5 rounded text-xs font-semibold bg-rose-100 text-rose-800 border border-rose-200">
                    Low Stock
                  </span>
                  <span *ngIf="!s.low" class="px-2 py-0.5 rounded text-xs font-semibold bg-emerald-100 text-emerald-800 border border-emerald-200">
                    In Stock
                  </span>
                </td>
                <td class="px-6 py-4 text-right">
                  <button (click)="openMovementModal(s)" class="px-2.5 py-1 text-xs font-semibold bg-slate-100 text-slate-700 rounded hover:bg-slate-200">
                    Record Movement
                  </button>
                </td>
              </tr>
              <tr *ngIf="!stock().length">
                <td colspan="6" class="px-6 py-12 text-center text-slate-400 text-sm">
                  No stock items found. Click "New Stock Item" to add supplies.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Create Item Modal -->
      <app-modal [isOpen]="isCreateOpen()" title="Add New Stock Item" (close)="isCreateOpen.set(false)">
        <form (ngSubmit)="saveItem()" class="space-y-4">
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Item Name *</label>
            <input type="text" [(ngModel)]="newItem.name" name="name" required class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm" placeholder="e.g. Disinfectant Floor Cleaner 5L" />
          </div>
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-xs font-semibold text-slate-700 mb-1">Category</label>
              <input type="text" [(ngModel)]="newItem.category" name="category" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm" placeholder="Chemicals" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-700 mb-1">Unit of Measure</label>
              <input type="text" [(ngModel)]="newItem.unit" name="unit" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm" placeholder="cans, pcs, liters" />
            </div>
          </div>
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Reorder Threshold Level</label>
            <input type="number" [(ngModel)]="newItem.reorderLevel" name="reorderLevel" min="1" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm" />
          </div>
          <div class="flex justify-end gap-3 pt-4 border-t border-slate-100">
            <button type="button" (click)="isCreateOpen.set(false)" class="px-4 py-2 text-sm text-slate-600 font-medium">Cancel</button>
            <button type="submit" class="px-5 py-2 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-semibold">Save Item</button>
          </div>
        </form>
      </app-modal>

      <!-- Record Movement Modal -->
      <app-modal [isOpen]="isMovementOpen()" title="Record Stock Movement" (close)="isMovementOpen.set(false)">
        <form (ngSubmit)="submitMovement()" class="space-y-4">
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Movement Type</label>
            <select [(ngModel)]="movementForm.type" name="type" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm">
              <option value="RECEIVE">Receive (Inbound Shipment)</option>
              <option value="ISSUE">Issue (Dispatch to Crew)</option>
              <option value="RETURN">Return (Crew Unused Items)</option>
              <option value="ADJUST">Inventory Count Adjustment</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Quantity</label>
            <input type="number" [(ngModel)]="movementForm.qty" name="qty" min="1" required class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm" />
          </div>
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Recipient / Custodian (for Issue)</label>
            <input type="text" [(ngModel)]="movementForm.recipient" name="recipient" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm" placeholder="e.g. Team Alpha Lead" />
          </div>
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Reason / Notes</label>
            <input type="text" [(ngModel)]="movementForm.reason" name="reason" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm" placeholder="Job dispatch, stock restock, etc." />
          </div>
          <div class="flex justify-end gap-3 pt-4 border-t border-slate-100">
            <button type="button" (click)="isMovementOpen.set(false)" class="px-4 py-2 text-sm text-slate-600 font-medium">Cancel</button>
            <button type="submit" class="px-5 py-2 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-semibold">Post Movement</button>
          </div>
        </form>
      </app-modal>
    </div>
  `
})
export class StockComponent implements OnInit {
  private api = inject(ApiService);
  stock = signal<StockItem[]>([]);

  isCreateOpen = signal(false);
  isMovementOpen = signal(false);
  selectedItem: StockItem | null = null;

  newItem = {
    name: '',
    category: 'Chemicals',
    unit: 'pcs',
    reorderLevel: 5
  };

  movementForm = {
    type: 'RECEIVE',
    qty: 10,
    recipient: '',
    reason: 'Initial warehouse stocking'
  };

  ngOnInit() {
    this.loadStock();
  }

  loadStock() {
    this.api.get<StockItem[]>('stock').subscribe({
      next: res => this.stock.set(res),
      error: err => console.error(err)
    });
  }

  saveItem() {
    this.api.post<StockItem>('stock', this.newItem).subscribe({
      next: () => {
        this.isCreateOpen.set(false);
        this.newItem = { name: '', category: 'Chemicals', unit: 'pcs', reorderLevel: 5 };
        this.loadStock();
      },
      error: err => alert(err.error?.detail || 'Failed to create stock item')
    });
  }

  openMovementModal(s: StockItem) {
    this.selectedItem = s;
    this.isMovementOpen.set(true);
  }

  submitMovement() {
    if (!this.selectedItem) return;
    this.api.post(`stock/${this.selectedItem.id}/movements`, this.movementForm).subscribe({
      next: () => {
        this.isMovementOpen.set(false);
        this.loadStock();
      },
      error: err => alert(err.error?.detail || 'Failed to record stock movement')
    });
  }
}
