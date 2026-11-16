import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { API_BASE_URL } from '../config/api.constants';
import { AuthUser, LoginRequest, LoginResponse, RegisterRequest } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly storage = typeof localStorage === 'undefined' ? null : localStorage;
  private readonly storedUser = this.readStoredUser();

  readonly currentUser = signal<AuthUser | null>(this.storedUser);
  readonly isAuthenticated = computed(() => Boolean(this.token && this.currentUser()));

  get token(): string | null {
    return this.storage?.getItem('token') ?? null;
  }

  register(request: RegisterRequest): Observable<AuthUser> {
    return this.http.post<AuthUser>(`${API_BASE_URL}/auth/register`, request);
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${API_BASE_URL}/auth/login`, request).pipe(
      tap((response) => {
        this.storage?.setItem('token', response.token);
        this.storage?.setItem('userId', String(response.userId));
        this.storage?.setItem('userName', response.userName);
        this.storage?.setItem('fullName', response.fullName);
        this.currentUser.set({
          userId: response.userId,
          userName: response.userName,
          fullName: response.fullName,
        });
      }),
    );
  }

  logout(): void {
    this.storage?.removeItem('token');
    this.storage?.removeItem('userId');
    this.storage?.removeItem('userName');
    this.storage?.removeItem('fullName');
    this.currentUser.set(null);
  }

  private readStoredUser(): AuthUser | null {
    if (!this.storage?.getItem('token')) {
      return null;
    }

    const userId = Number(this.storage.getItem('userId'));
    const userName = this.storage.getItem('userName');
    const fullName = this.storage.getItem('fullName');
    if (!Number.isInteger(userId) || !userName || !fullName) {
      return null;
    }

    return { userId, userName, fullName };
  }
}
