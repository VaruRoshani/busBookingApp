import { HttpErrorResponse } from '@angular/common/http';
import { DecimalPipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { Booking } from '../../models/api.models';
import { BookingService } from '../../services/booking.service';
import { finalize } from 'rxjs';

@Component({
  imports: [DecimalPipe],
  selector: 'app-my-bookings',
  styleUrl: './my-bookings.css',
  templateUrl: './my-bookings.html',
})
export class MyBookings implements OnInit {
  readonly bookings = signal<Booking[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal('');

  constructor(private readonly bookingService: BookingService) {}

  seatNumbers(booking: Booking): string {
    return booking.passengers.map((passenger) => String(passenger.seatNo)).join(', ');
  }

  passengerNames(booking: Booking): string {
    return booking.passengers.map((passenger) => passenger.passengerName).join(', ');
  }

  ngOnInit(): void {
    this.bookingService.getMine().pipe(
      finalize(() => this.loading.set(false)),
    ).subscribe({
      next: (bookings) => this.bookings.set(bookings),
      error: (error: unknown) => this.errorMessage.set(this.readError(error)),
    });
  }

  private readError(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const body: unknown = error.error;
      if (body && typeof body === 'object' && 'message' in body && typeof body.message === 'string') {
        return body.message;
      }
      if (error.status === 0) {
        return 'Unable to reach the booking service. Make sure the API is running.';
      }
    }

    return 'Could not load your bookings. Please try again.';
  }
}
