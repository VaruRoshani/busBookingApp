import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.constants';
import { Booking, BookingCreated, CreateBookingRequest } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class BookingService {
  private readonly http = inject(HttpClient);

  create(request: CreateBookingRequest): Observable<BookingCreated> {
    return this.http.post<BookingCreated>(`${API_BASE_URL}/bookings`, request);
  }

  getMine(): Observable<Booking[]> {
    return this.http.get<Booking[]>(`${API_BASE_URL}/bookings/my`);
  }
}
