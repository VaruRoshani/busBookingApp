namespace BusBookingApi.Models;

public sealed class Bus
{
    public int BusId { get; set; }
    public string BusName { get; set; } = string.Empty;
    public string BusNumber { get; set; } = string.Empty;
    public string BusType { get; set; } = string.Empty;
    public string FromLocation { get; set; } = string.Empty;
    public string ToLocation { get; set; } = string.Empty;
    public TimeOnly DepartureTime { get; set; }
    public TimeOnly ArrivalTime { get; set; }
    public decimal PricePerSeat { get; set; }
    public int TotalSeats { get; set; } = 40;
    public decimal Rating { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
