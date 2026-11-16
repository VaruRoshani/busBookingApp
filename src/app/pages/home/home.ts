import { Component, computed, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { BusSearchResult } from '../../models/api.models';
import { BusService } from '../../services/bus.service';

@Component({
  imports: [FormsModule, DecimalPipe],
  selector: 'app-home',
  styleUrl: './home.css',
  templateUrl: './home.html',
})
export class Home {
  readonly results = signal<BusSearchResult[]>([]);
  readonly loading = signal(false);
  readonly errorMessage = signal('');
  readonly hasSearched = signal(false);
  readonly cities = signal<string[]>([]);
  readonly activeSort = signal<'rating' | 'departure' | 'price'>('rating');
  readonly sortedResults = computed(() => {
    const items = [...this.results()];
    switch (this.activeSort()) {
      case 'departure': return items.sort((a, b) => a.departureTime.localeCompare(b.departureTime));
      case 'price': return items.sort((a, b) => a.pricePerSeat - b.pricePerSeat);
      default: return items.sort((a, b) => b.rating - a.rating);
    }
  });
  readonly showFromSuggestions = signal(false);
  readonly showToSuggestions = signal(false);

  from = 'Pune';
  to = 'Nagpur';
  travelDate = this.today();

  constructor(
    private readonly buses: BusService,
    private readonly router: Router,
  ) {
    this.buses.getCities().subscribe({ next: (cities) => this.cities.set(cities), error: () => undefined });
    this.searchBuses();
  }

  citySuggestions(value: string): string[] {
    const query = value.trim().toLocaleLowerCase();
    return this.cities().filter((city) => !query || city.toLocaleLowerCase().includes(query));
  }

  selectCity(which: 'from' | 'to', city: string): void {
    if (which === 'from') { this.from = city; this.showFromSuggestions.set(false); }
    else { this.to = city; this.showToSuggestions.set(false); }
  }

  sortBy(sort: 'rating' | 'departure' | 'price'): void {
    this.activeSort.set(sort);
  }

  searchBuses(): void {
    if (!this.from.trim() || !this.to.trim() || !this.travelDate) {
      this.errorMessage.set('Enter both locations and a travel date.');
      this.hasSearched.set(true);
      return;
    }

    this.hasSearched.set(true);
    this.loading.set(true);
    this.errorMessage.set('');
    this.results.set([]);

    this.buses.search(this.from.trim(), this.to.trim(), this.travelDate).pipe(
      finalize(() => this.loading.set(false)),
    ).subscribe({
      next: (results) => this.results.set(results),
      error: (error: unknown) => this.errorMessage.set(this.readError(error)),
    });
  }

  setTravelDate(daysFromToday: number): void {
    const date = new Date();
    date.setDate(date.getDate() + daysFromToday);
    this.travelDate = this.toDateInputValue(date);
  }

  openBooking(busId: number): void {
    void this.router.navigate(['/new-booking', busId], {
      queryParams: { travelDate: this.travelDate },
    });
  }

  formatTime(time: string): string {
    return time.slice(0, 5);
  }

  private today(): string {
    return this.toDateInputValue(new Date());
  }

  private toDateInputValue(date: Date): string {
    const localDate = new Date(date);
    localDate.setMinutes(localDate.getMinutes() - localDate.getTimezoneOffset());
    return localDate.toISOString().slice(0, 10);
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

    return 'Bus search failed. Please try again.';
  }
}
