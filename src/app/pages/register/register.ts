import { HttpErrorResponse } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../services/auth.service';

@Component({
  imports: [FormsModule],
  selector: 'app-register',
  styleUrl: './register.css',
  templateUrl: './register.html',
})
export class Register {
  readonly mode = signal<'login' | 'register'>('register');
  readonly loading = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');
  readonly fieldErrors = signal<Record<string, string>>({});

  readonly loginForm = { userName: '', password: '' };
  readonly registerForm = { userName: '', emailId: '', fullName: '', password: '' };

  constructor(
    private readonly auth: AuthService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
  ) {
    this.route.url.subscribe((segments) => {
      this.mode.set(segments[0]?.path === 'login' ? 'login' : 'register');
    });
  }

  setMode(mode: 'login' | 'register'): void {
    this.mode.set(mode);
    this.errorMessage.set('');
    this.successMessage.set('');
    this.fieldErrors.set({});
    void this.router.navigateByUrl(`/${mode}`);
  }

  submitLogin(): void {
    this.loading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.auth.login(this.loginForm).pipe(
      finalize(() => this.loading.set(false)),
    ).subscribe({
      next: () => void this.router.navigateByUrl('/home'),
      error: (error: unknown) => this.errorMessage.set(error instanceof HttpErrorResponse && error.status === 0
        ? 'Cannot connect to server'
        : 'Invalid username or password'),
    });
  }

  submitRegister(): void {
    const errors = this.validateRegister();
    this.fieldErrors.set(errors);
    if (Object.keys(errors).length > 0) return;
    this.loading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.auth.register(this.registerForm).pipe(
      finalize(() => this.loading.set(false)),
    ).subscribe({
      next: () => {
        this.registerForm.password = '';
        this.mode.set('login');
        this.successMessage.set('Your account is ready. Please log in.');
        void this.router.navigateByUrl('/login');
      },
      error: (error: unknown) => this.errorMessage.set(this.readError(error)),
    });
  }

  validateField(field: string): void {
    this.fieldErrors.update((current) => {
      const next = { ...current };
      const errors = this.validateRegister();
      if (errors[field]) next[field] = errors[field];
      else delete next[field];
      return next;
    });
  }

  private validateRegister(): Record<string, string> {
    const errors: Record<string, string> = {};
    if (this.registerForm.fullName.trim().length < 3) errors['fullName'] = 'Full name must be at least 3 characters.';
    if (this.registerForm.userName.trim().length < 4) errors['userName'] = 'Username must be at least 4 characters.';
    else if (/\s/.test(this.registerForm.userName)) errors['userName'] = 'Username cannot contain spaces.';
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.registerForm.emailId.trim())) errors['emailId'] = 'Enter a valid email address.';
    if (this.registerForm.password.length < 8 || !/[A-Z]/.test(this.registerForm.password) ||
        !/[a-z]/.test(this.registerForm.password) || !/\d/.test(this.registerForm.password) ||
        !/[^A-Za-z0-9\s]/.test(this.registerForm.password)) {
      errors['password'] = 'Minimum 8 characters with uppercase, lowercase, number and special character';
    }
    return errors;
  }

  private readError(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const body: unknown = error.error;
      if (body && typeof body === 'object') {
        const details = body as { message?: unknown; detail?: unknown; title?: unknown; errors?: unknown };
        if (details.errors && typeof details.errors === 'object') {
          const mapped: Record<string, string> = {};
          for (const [key, messages] of Object.entries(details.errors as Record<string, unknown>)) {
            const field = key.charAt(0).toLowerCase() + key.slice(1);
            if (['fullName', 'userName', 'emailId', 'password'].includes(field)) {
              mapped[field] = Array.isArray(messages) ? String(messages[0] ?? '') : String(messages);
            }
          }
          if (Object.keys(mapped).length) {
            this.fieldErrors.set(mapped);
            return '';
          }
        }
        if (typeof details.message === 'string') {
          return details.message;
        }
        if (typeof details.detail === 'string') {
          return details.detail;
        }
        if (typeof details.title === 'string') {
          return details.title;
        }
      }

      if (error.status === 0) {
        return 'Unable to reach the booking service. Please try again later.';
      }
    }

    return 'Something went wrong. Please check your details and try again.';
  }
}
