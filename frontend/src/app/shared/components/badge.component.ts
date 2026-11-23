import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span [ngClass]="badgeClass" class="inline-flex items-center px-2 py-0.5 rounded text-xs font-semibold uppercase tracking-wider">
      {{ label }}
    </span>
  `
})
export class BadgeComponent {
  @Input() label: string = '';
  @Input() status?: string;

  get badgeClass(): string {
    const s = (this.status || this.label).toUpperCase();
    switch (s) {
      case 'COMPLETED':
      case 'PAID':
      case 'APPROVED':
      case 'ACCEPTED':
      case 'AVAILABLE':
        return 'bg-emerald-100 text-emerald-800 border border-emerald-200';
      case 'IN_PROGRESS':
      case 'ORDERED':
      case 'ISSUED':
      case 'PARTIAL':
        return 'bg-blue-100 text-blue-800 border border-blue-200';
      case 'PLANNED':
      case 'PENDING_APPROVAL':
      case 'DRAFT':
      case 'NEW':
      case 'REPORTED':
        return 'bg-amber-100 text-amber-800 border border-amber-200';
      case 'CANCELLED':
      case 'REJECTED':
      case 'PROBLEM':
      case 'OVERDUE':
      case 'DAMAGED':
        return 'bg-rose-100 text-rose-800 border border-rose-200';
      default:
        return 'bg-slate-100 text-slate-800 border border-slate-200';
    }
  }
}
