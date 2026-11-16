export interface RegisterRequest {
  userName: string;
  emailId: string;
  fullName: string;
  password: string;
}

export interface LoginRequest {
  userName: string;
  password: string;
}

export interface AuthUser {
  userId: number;
  userName: string;
  fullName: string;
}

export interface LoginResponse extends AuthUser {
  token: string;
}

export interface Bus {
  busId: number;
  busName: string;
  busNumber: string;
  busType: string;
  fromLocation: string;
  toLocation: string;
  departureTime: string;
  arrivalTime: string;
  pricePerSeat: number;
  totalSeats: number;
  rating: number;
}

export interface BusSearchResult extends Bus {
  availableSeats: number;
}

export interface BookedSeatsResponse {
  busId: number;
  travelDate: string;
  bookedSeats: number[];
}

export interface PassengerRequest {
  name: string;
  age: number;
  gender: string;
  seatNo: number;
}

export interface CreateBookingRequest {
  busId: number;
  travelDate: string;
  passengers: PassengerRequest[];
}

export interface BookingPassenger {
  passengerId: number;
  passengerName: string;
  age: number;
  gender: string;
  seatNo: number;
}

export interface Booking {
  bookingId: number;
  busId: number;
  busName: string;
  busNumber: string;
  busType: string;
  fromLocation: string;
  toLocation: string;
  departureTime: string;
  arrivalTime: string;
  travelDate: string;
  pricePerSeat: number;
  totalAmount: number;
  bookingDate: string;
  passengers: BookingPassenger[];
}

export interface BookingCreated {
  bookingId: number;
  busId: number;
  travelDate: string;
  totalAmount: number;
  passengers: BookingPassenger[];
}
