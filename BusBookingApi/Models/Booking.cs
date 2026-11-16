namespace BusBookingApi.Models;

public sealed class Booking
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int BusId { get; set; }
    public DateOnly TravelDate { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime BookingDate { get; set; }

    public AppUser User { get; set; } = null!;
    public Bus Bus { get; set; } = null!;
    public ICollection<BookingPassenger> Passengers { get; set; } = new List<BookingPassenger>();
}
