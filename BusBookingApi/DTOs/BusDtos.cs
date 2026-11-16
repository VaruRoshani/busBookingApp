namespace BusBookingApi.DTOs;

public sealed record BusDto(
    int BusId,
    string BusName,
    string BusNumber,
    string BusType,
    string FromLocation,
    string ToLocation,
    TimeOnly DepartureTime,
    TimeOnly ArrivalTime,
    decimal PricePerSeat,
    int TotalSeats,
    decimal Rating);

public sealed record BusSearchResultDto(
    int BusId,
    string BusName,
    string BusNumber,
    string BusType,
    string FromLocation,
    string ToLocation,
    TimeOnly DepartureTime,
    TimeOnly ArrivalTime,
    decimal PricePerSeat,
    int TotalSeats,
    decimal Rating,
    int AvailableSeats);

public sealed record BookedSeatsDto(int BusId, DateOnly TravelDate, IReadOnlyList<int> BookedSeats);
