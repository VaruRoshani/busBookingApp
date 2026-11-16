using System.ComponentModel.DataAnnotations;

namespace BusBookingApi.DTOs;

public sealed class CreateBookingRequest
{
    [Range(1, int.MaxValue)]
    public int BusId { get; set; }

    [Required]
    public DateOnly? TravelDate { get; set; }

    [Required, MinLength(1), MaxLength(40)]
    public List<CreatePassengerRequest> Passengers { get; set; } = [];
}

public sealed class CreatePassengerRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 120)]
    public int Age { get; set; }

    [Required, RegularExpression("^(?i:male|female|other)$", ErrorMessage = "Gender must be Male, Female, or Other.")]
    public string Gender { get; set; } = string.Empty;

    [Range(1, 40)]
    public int SeatNo { get; set; }
}

public sealed record BookingPassengerDto(int PassengerId, string PassengerName, int Age, string Gender, int SeatNo);

public sealed record MyBookingDto(
    int BookingId,
    int BusId,
    string BusName,
    string BusNumber,
    string BusType,
    string FromLocation,
    string ToLocation,
    TimeOnly DepartureTime,
    TimeOnly ArrivalTime,
    DateOnly TravelDate,
    decimal PricePerSeat,
    decimal TotalAmount,
    DateTime BookingDate,
    IReadOnlyList<BookingPassengerDto> Passengers);

public sealed record BookingCreatedDto(
    int BookingId,
    int BusId,
    DateOnly TravelDate,
    decimal TotalAmount,
    IReadOnlyList<BookingPassengerDto> Passengers);
