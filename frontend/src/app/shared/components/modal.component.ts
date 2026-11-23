import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconComponent } from './icon.component';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <div *ngIf="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-sm animate-fade-in">
      <div class="relative w-full max-w-xl bg-white rounded-xl shadow-2xl overflow-hidden border border-slate-200">
        <!-- Header -->
        <div class="flex items-center justify-between px-6 py-4 border-b border-slate-100 bg-slate-50/50">
          <h3 class="text-base font-semibold text-slate-800">{{ title }}</h3>
          <button (click)="close.emit()" class="text-slate-400 hover:text-slate-600 rounded-lg p-1 transition-colors">
            <app-icon name="x" [size]="18"></app-icon>
          </button>
        </div>

        <!-- Body -->
        <div class="p-6 max-h-[75vh] overflow-y-auto">
          <ng-content></ng-content>
        </div>
      </div>
    </div>
  `
})
export class ModalComponent {
  @Input() isOpen = false;
  @Input() title = '';
  @Output() close = new EventEmitter<void>();
}
