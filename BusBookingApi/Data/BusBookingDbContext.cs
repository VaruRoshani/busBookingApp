using BusBookingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BusBookingApi.Data;

public sealed class BusBookingDbContext(DbContextOptions<BusBookingDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Bus> Buses => Set<Bus>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingPassenger> BookingPassengers => Set<BookingPassenger>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.UserId);
            entity.Property(user => user.UserName).HasMaxLength(32).UseCollation("SQL_Latin1_General_CP1_CI_AS").IsRequired();
            entity.Property(user => user.EmailId).HasMaxLength(254).UseCollation("SQL_Latin1_General_CP1_CI_AS").IsRequired();
            entity.Property(user => user.FullName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.Role).HasMaxLength(30).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(100).IsRequired();
            entity.Property(user => user.CreatedDate).IsRequired();
            entity.HasIndex(user => user.UserName).IsUnique();
            entity.HasIndex(user => user.EmailId).IsUnique();
            entity.HasMany(user => user.Bookings)
                .WithOne(booking => booking.User)
                .HasForeignKey(booking => booking.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Bus>(entity =>
        {
            entity.ToTable("Buses");
            entity.HasKey(bus => bus.BusId);
            entity.Property(bus => bus.BusName).HasMaxLength(120).IsRequired();
            entity.Property(bus => bus.BusNumber).HasMaxLength(20).IsRequired();
            entity.Property(bus => bus.BusType).HasMaxLength(60).IsRequired();
            entity.Property(bus => bus.FromLocation).HasMaxLength(80).IsRequired();
            entity.Property(bus => bus.ToLocation).HasMaxLength(80).IsRequired();
            entity.Property(bus => bus.PricePerSeat).HasPrecision(10, 2).IsRequired();
            entity.Property(bus => bus.TotalSeats).HasDefaultValue(40);
            entity.Property(bus => bus.Rating).HasPrecision(2, 1).IsRequired();
            entity.HasIndex(bus => bus.BusNumber).IsUnique();
            entity.HasMany(bus => bus.Bookings)
                .WithOne(booking => booking.Bus)
                .HasForeignKey(booking => booking.BusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("Bookings");
            entity.HasKey(booking => booking.BookingId);
            entity.Property(booking => booking.TravelDate).IsRequired();
            entity.Property(booking => booking.TotalAmount).HasPrecision(12, 2).IsRequired();
            entity.Property(booking => booking.BookingDate).IsRequired();
            entity.HasIndex(booking => new { booking.BusId, booking.TravelDate });
            entity.HasIndex(booking => new { booking.UserId, booking.BookingDate });
            entity.HasMany(booking => booking.Passengers)
                .WithOne(passenger => passenger.Booking)
                .HasForeignKey(passenger => passenger.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookingPassenger>(entity =>
        {
            entity.ToTable("BookingPassengers");
            entity.HasKey(passenger => passenger.PassengerId);
            entity.Property(passenger => passenger.PassengerName).HasMaxLength(100).IsRequired();
            entity.Property(passenger => passenger.Gender).HasMaxLength(20).IsRequired();
            entity.HasIndex(passenger => new { passenger.BookingId, passenger.SeatNo }).IsUnique();
        });
    }
}
