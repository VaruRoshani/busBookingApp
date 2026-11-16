import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin, finalize } from 'rxjs';
import { Bus, PassengerRequest } from '../../models/api.models';
import { BookingService } from '../../services/booking.service';
import { BusService } from '../../services/bus.service';

@Component({
  imports: [FormsModule, DecimalPipe],
  selector: 'app-booking-form',
  styleUrl: './booking-form.css',
  templateUrl: './booking-form.html',
})
export class BookingForm {
  readonly seats = Array.from({ length: 40 }, (_, index) => index + 1);
  readonly bus = signal<Bus | null>(null);
  readonly bookedSeats = signal<number[]>([]);
  readonly selectedSeats = signal<number[]>([]);
  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');
  readonly totalAmount = computed(
    () => (this.bus()?.pricePerSeat ?? 0) * this.selectedSeats().length,
  );

  readonly busId: number;
  readonly travelDate: string;
  passengers: Record<number, PassengerDraft> = {};

  constructor(
    route: ActivatedRoute,
    private readonly buses: BusService,
    private readonly bookings: BookingService,
    private readonly router: Router,
  ) {
    this.busId = Number(route.snapshot.paramMap.get('busId'));
    this.travelDate = route.snapshot.queryParamMap.get('travelDate') || this.today();

    if (!Number.isInteger(this.busId) || this.busId < 1) {
      this.loading.set(false);
      this.errorMessage.set('The bus ID is invalid.');
      return;
    }

    forkJoin({
      bus: this.buses.getBus(this.busId),
      seats: this.buses.getBookedSeats(this.busId, this.travelDate),
    }).pipe(
      finalize(() => this.loading.set(false)),
    ).subscribe({
      next: ({ bus, seats }) => {
        this.bus.set(bus);
        this.bookedSeats.set(seats.bookedSeats);
      },
      error: (error: unknown) => this.errorMessage.set(this.readError(error)),
    });
  }

  toggleSeat(seatNo: number): void {
    if (this.bookedSeats().includes(seatNo) || this.loading()) {
      return;
    }

    const currentSeats = this.selectedSeats();
    if (currentSeats.includes(seatNo)) {
      this.selectedSeats.set(currentSeats.filter((seat) => seat !== seatNo));
      delete this.passengers[seatNo];
      return;
    }

    this.selectedSeats.set([...currentSeats, seatNo].sort((a, b) => a - b));
    this.passengers[seatNo] = { name: '', age: null, gender: '' };
  }

  seatColumn(seatNo: number): string {
    const positionInRow = (seatNo - 1) % 4;
    return String(positionInRow < 2 ? positionInRow + 1 : positionInRow + 2);
  }

  isSelected(seatNo: number): boolean {
    return this.selectedSeats().includes(seatNo);
  }

  isBooked(seatNo: number): boolean {
    return this.bookedSeats().includes(seatNo);
  }

  formatTime(time: string): string {
    return time.slice(0, 5);
  }

  formatDate(date: string): string {
    return new Date(`${date}T00:00:00`).toLocaleDateString();
  }

  bookNow(): void {
    const selected = this.selectedSeats();
    if (selected.length === 0) {
      this.errorMessage.set('Select at least one seat to continue.');
      return;
    }

    const passengers: PassengerRequest[] = [];
    for (const seatNo of selected) {
      const passenger = this.passengers[seatNo];
      if (
        !passenger ||
        !passenger.name.trim() ||
        passenger.age === null ||
        !Number.isInteger(passenger.age) ||
        passenger.age < 1 ||
        passenger.age > 120 ||
        !passenger.gender
      ) {
        this.errorMessage.set('Enter a name, age, and gender for every selected seat.');
        return;
      }

      passengers.push({
        name: passenger.name.trim(),
        age: passenger.age,
        gender: passenger.gender,
        seatNo,
      });
    }

    this.submitting.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');
    this.bookings.create({
      busId: this.busId,
      travelDate: this.travelDate,
      passengers,
    }).pipe(
      finalize(() => this.submitting.set(false)),
    ).subscribe({
      next: () => {
        this.successMessage.set('Booking confirmed. Redirecting to My Bookings…');
        window.setTimeout(() => void this.router.navigateByUrl('/my-bookings'), 1200);
      },
      error: (error: unknown) => {
        if (error instanceof HttpErrorResponse && Array.isArray(error.error?.bookedSeats)) {
          this.bookedSeats.set([...this.bookedSeats(), ...error.error.bookedSeats]);
          this.selectedSeats.set(selected.filter((seat) => !this.bookedSeats().includes(seat)));
        }
        this.errorMessage.set(this.readError(error));
      },
    });
  }

  private today(): string {
    const date = new Date();
    date.setMinutes(date.getMinutes() - date.getTimezoneOffset());
    return date.toISOString().slice(0, 10);
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

    return 'Unable to load or save this booking. Please try again.';
  }
}

interface PassengerDraft {
  name: string;
  age: number | null;
  gender: string;
}
