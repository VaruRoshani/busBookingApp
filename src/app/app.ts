import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  imports: [RouterOutlet, RouterLink],
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App {
  protected readonly title = signal('busBookingApp');
  protected readonly auth = inject(AuthService);

  private readonly router = inject(Router);

  firstName(fullName: string): string {
    return fullName.trim().split(/\s+/)[0] || fullName;
  }

  logout(): void {
    this.auth.logout();
    void this.router.navigateByUrl('/home');
  }
}
