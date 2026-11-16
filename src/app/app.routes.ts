import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { BookingForm } from './pages/booking-form/booking-form';
import { MyBookings } from './pages/my-bookings/my-bookings';
import { Register } from './pages/register/register';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', component: Home, title: 'Search buses' },
  { path: 'login', component: Register, title: 'Login' },
  { path: 'register', component: Register, title: 'Register' },
  {
    path: 'new-booking/:busId',
    component: BookingForm,
    canActivate: [authGuard],
    title: 'Select seats',
  },
  {
    path: 'my-bookings',
    component: MyBookings,
    canActivate: [authGuard],
    title: 'My bookings',
  },
  { path: '**', redirectTo: 'home' },
];
