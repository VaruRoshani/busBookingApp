import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.constants';
import { BookedSeatsResponse, Bus, BusSearchResult } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class BusService {
  private readonly http = inject(HttpClient);

  search(from: string, to: string, date: string): Observable<BusSearchResult[]> {
    const params = new HttpParams().set('from', from).set('to', to).set('date', date);
    return this.http.get<BusSearchResult[]>(`${API_BASE_URL}/buses/search`, { params });
  }

  getCities(): Observable<string[]> {
    return this.http.get<string[]>(`${API_BASE_URL}/buses/cities`);
  }

  getBus(busId: number): Observable<Bus> {
    return this.http.get<Bus>(`${API_BASE_URL}/buses/${busId}`);
  }

  getBookedSeats(busId: number, travelDate: string): Observable<BookedSeatsResponse> {
    const params = new HttpParams().set('travelDate', travelDate);
    return this.http.get<BookedSeatsResponse>(
      `${API_BASE_URL}/buses/${busId}/booked-seats`,
      { params },
    );
  }
}
