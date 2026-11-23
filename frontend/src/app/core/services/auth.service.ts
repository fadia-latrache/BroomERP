import { Injectable, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from './api.service';
import { User, Role } from '../models/models';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private api = inject(ApiService);
  private router = inject(Router);

  private tokenSignal = signal<string | null>(localStorage.getItem('token'));
  currentUser = signal<User | null>(this.getStoredUser());

  isAuthenticated = computed(() => !!this.tokenSignal());
  isGM = computed(() => this.currentUser()?.role === 'GM');

  private getStoredUser(): User | null {
    const raw = localStorage.getItem('user');
    try {
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }

  getToken(): string | null {
    return this.tokenSignal();
  }

  login(email: string, password: string) {
    return this.api.post<{ token: string; role: Role; email: string; userId: number }>('auth/login', { email, password }).pipe(
      tap(res => {
        localStorage.setItem('token', res.token);
        this.tokenSignal.set(res.token);
        const user: User = { id: res.userId, email: res.email, role: res.role };
        localStorage.setItem('user', JSON.stringify(user));
        this.currentUser.set(user);
        this.loadProfile();
      })
    );
  }

  loadProfile() {
    if (!this.getToken()) return;
    this.api.get<User>('me').subscribe({
      next: user => {
        this.currentUser.set(user);
        localStorage.setItem('user', JSON.stringify(user));
      },
      error: () => this.logout()
    });
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.tokenSignal.set(null);
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }
}
