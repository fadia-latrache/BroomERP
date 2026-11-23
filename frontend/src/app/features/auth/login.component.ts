import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-slate-900 p-4">
      <div class="w-full max-w-md bg-white rounded-2xl shadow-xl p-8 border border-slate-100">
        <!-- Brand Header -->
        <div class="text-center mb-8">
          <div class="w-12 h-12 rounded-xl bg-emerald-500 text-white font-black text-2xl flex items-center justify-center mx-auto mb-4 shadow-lg shadow-emerald-500/20">
            C
          </div>
          <h2 class="text-2xl font-bold text-slate-900 tracking-tight">CleanOps ERP</h2>
          <p class="text-sm text-slate-500 mt-1">Sign in to your staff portal</p>
        </div>

        <!-- Error Alert -->
        <div *ngIf="errorMessage()" class="mb-5 p-3 rounded-lg bg-rose-50 border border-rose-200 text-rose-700 text-xs font-medium">
          {{ errorMessage() }}
        </div>

        <!-- Form -->
        <form (ngSubmit)="onLogin()" class="space-y-4">
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5 uppercase tracking-wider">Email</label>
            <input
              type="email"
              [(ngModel)]="email"
              name="email"
              required
              class="w-full px-3.5 py-2.5 rounded-lg border border-slate-300 text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-colors"
              placeholder="name@company.com"
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5 uppercase tracking-wider">Password</label>
            <input
              type="password"
              [(ngModel)]="password"
              name="password"
              required
              class="w-full px-3.5 py-2.5 rounded-lg border border-slate-300 text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-colors"
              placeholder="••••••••••••"
            />
          </div>

          <button
            type="submit"
            [disabled]="isLoading()"
            class="w-full py-2.5 px-4 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white text-sm font-semibold shadow-md shadow-emerald-600/20 transition-all disabled:opacity-50"
          >
            {{ isLoading() ? 'Signing in...' : 'Sign In' }}
          </button>
        </form>

        <!-- Quick Demo Fill -->
        <div class="mt-8 pt-5 border-t border-slate-100 flex items-center justify-between text-xs">
          <span class="text-slate-400">Demo Account</span>
          <button (click)="fillDemo()" type="button" class="font-semibold text-emerald-600 hover:text-emerald-700">
            Fill Demo Credentials
          </button>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  email = 'gm@broomservices.qa';
  password = 'ChangeThisPasswordNow123!';
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  fillDemo() {
    this.email = 'gm@broomservices.qa';
    this.password = 'ChangeThisPasswordNow123!';
  }

  onLogin() {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.auth.login(this.email, this.password).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/dashboard']);
      },
      error: err => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.detail || 'Invalid email or password');
      }
    });
  }
}
