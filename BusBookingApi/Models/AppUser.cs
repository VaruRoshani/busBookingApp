namespace BusBookingApi.Models;

public sealed class AppUser
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string EmailId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
