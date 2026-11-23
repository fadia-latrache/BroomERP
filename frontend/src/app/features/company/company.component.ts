import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { CompanyProfile } from '../../core/models/models';

@Component({
  selector: 'app-company',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="max-w-3xl space-y-6 animate-fade-in">
      <div>
        <h2 class="text-2xl font-bold text-slate-900 tracking-tight">Company Profile</h2>
        <p class="text-sm text-slate-500 mt-1">Official business registration details printed on quotations and tax invoices</p>
      </div>

      <div class="bg-white rounded-xl border border-slate-200/80 shadow-sm p-6">
        <form (ngSubmit)="saveProfile()" class="space-y-4">
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Company Trade Name *</label>
            <input type="text" [(ngModel)]="profile.name" name="name" required class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm" />
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-xs font-semibold text-slate-700 mb-1">Commercial Registration (CR) #</label>
              <input type="text" [(ngModel)]="profile.crNumber" name="crNumber" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-700 mb-1">Official Phone</label>
              <input type="text" [(ngModel)]="profile.phone" name="phone" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm" />
            </div>
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Registered Address / Office Location</label>
            <textarea [(ngModel)]="profile.address" name="address" rows="2" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm"></textarea>
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">Bank Account & IBAN Details</label>
            <textarea [(ngModel)]="profile.bankDetails" name="bankDetails" rows="2" class="w-full px-3 py-2 border border-slate-300 rounded-lg text-sm" placeholder="Bank Name, IBAN, SWIFT code for invoice footers"></textarea>
          </div>

          <div class="pt-4 border-t border-slate-100 flex justify-end">
            <button type="submit" [disabled]="isSaving()" class="px-5 py-2.5 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-semibold shadow-sm transition-colors disabled:opacity-50">
              {{ isSaving() ? 'Saving...' : 'Update Profile' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `
})
export class CompanyComponent implements OnInit {
  private api = inject(ApiService);
  isSaving = signal(false);

  profile: CompanyProfile = {
    id: 0,
    name: 'Broom Services Co.',
    address: 'Doha, Qatar',
    phone: '+974 4400 1234',
    crNumber: '12345/CR',
    bankDetails: 'Qatar National Bank (QNB) - IBAN: QA99QNBA0000000012345678'
  };

  ngOnInit() {
    this.api.get<CompanyProfile>('company').subscribe(res => {
      if (res) this.profile = res;
    });
  }

  saveProfile() {
    this.isSaving.set(true);
    this.api.patch<CompanyProfile>('company', this.profile).subscribe({
      next: res => {
        this.isSaving.set(false);
        this.profile = res;
        alert('Company profile updated successfully.');
      },
      error: err => {
        this.isSaving.set(false);
        alert(err.error?.detail || 'Failed to save profile');
      }
    });
  }
}
