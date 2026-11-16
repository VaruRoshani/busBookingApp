using System.Data;
using System.Security.Claims;
using BusBookingApi.Data;
using BusBookingApi.DTOs;
using BusBookingApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingApi.Controllers;

[ApiController]
[Authorize]
[Route("api/bookings")]
public sealed class BookingsController(BusBookingDbContext db) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<BookingCreatedDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingCreatedDto>> Create(CreateBookingRequest request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized(new { message = "The access token does not contain a valid user ID." });
        }

        if (request.TravelDate is null)
        {
            return BadRequest(new { message = "A travel date is required." });
        }

        var travelDate = request.TravelDate.Value;
        if (travelDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return BadRequest(new { message = "Travel date cannot be in the past." });
        }

        if (request.Passengers.Select(passenger => passenger.SeatNo).Distinct().Count() != request.Passengers.Count)
        {
            return BadRequest(new { message = "Each passenger must have a different seat number." });
        }

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var bus = await db.Buses.SingleOrDefaultAsync(candidate => candidate.BusId == request.BusId);
        if (bus is null)
        {
            return NotFound(new { message = "Bus was not found." });
        }

        var invalidSeat = request.Passengers.FirstOrDefault(passenger =>
            passenger.SeatNo < 1 || passenger.SeatNo > bus.TotalSeats);
        if (invalidSeat is not null)
        {
            return BadRequest(new
            {
                message = $"Seat numbers must be between 1 and {bus.TotalSeats}.",
                seatNo = invalidSeat.SeatNo
            });
        }

        var requestedSeats = request.Passengers.Select(passenger => passenger.SeatNo).ToArray();
        var takenSeats = await db.BookingPassengers.AsNoTracking()
            .Where(passenger =>
                passenger.Booking.BusId == bus.BusId &&
                passenger.Booking.TravelDate == travelDate &&
                requestedSeats.Contains(passenger.SeatNo))
            .Select(passenger => passenger.SeatNo)
            .Distinct()
            .OrderBy(seatNo => seatNo)
            .ToListAsync();

        if (takenSeats.Count > 0)
        {
            return Conflict(new
            {
                message = "One or more selected seats have already been booked.",
                bookedSeats = takenSeats
            });
        }

        var booking = new Booking
        {
            UserId = userId,
            BusId = bus.BusId,
            TravelDate = travelDate,
            BookingDate = DateTime.UtcNow,
            TotalAmount = bus.PricePerSeat * request.Passengers.Count,
            Passengers = request.Passengers.Select(passenger => new BookingPassenger
            {
                PassengerName = passenger.Name.Trim(),
                Age = passenger.Age,
                Gender = passenger.Gender,
                SeatNo = passenger.SeatNo
            }).ToList()
        };

        db.Bookings.Add(booking);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        var response = new BookingCreatedDto(
            booking.BookingId,
            booking.BusId,
            booking.TravelDate,
            booking.TotalAmount,
            booking.Passengers.Select(passenger => new BookingPassengerDto(
                passenger.PassengerId,
                passenger.PassengerName,
                passenger.Age,
                passenger.Gender,
                passenger.SeatNo)).ToArray());

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpGet("my")]
    [ProducesResponseType<IReadOnlyList<MyBookingDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<MyBookingDto>>> GetMine()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized(new { message = "The access token does not contain a valid user ID." });
        }

        var bookings = await db.Bookings.AsNoTracking()
            .Where(booking => booking.UserId == userId)
            .Include(booking => booking.Bus)
            .Include(booking => booking.Passengers)
            .OrderByDescending(booking => booking.BookingDate)
            .ToListAsync();

        var response = bookings.Select(booking => new MyBookingDto(
            booking.BookingId,
            booking.BusId,
            booking.Bus.BusName,
            booking.Bus.BusNumber,
            booking.Bus.BusType,
            booking.Bus.FromLocation,
            booking.Bus.ToLocation,
            booking.Bus.DepartureTime,
            booking.Bus.ArrivalTime,
            booking.TravelDate,
            booking.Bus.PricePerSeat,
            booking.TotalAmount,
            booking.BookingDate,
            booking.Passengers
                .OrderBy(passenger => passenger.SeatNo)
                .Select(passenger => new BookingPassengerDto(
                    passenger.PassengerId,
                    passenger.PassengerName,
                    passenger.Age,
                    passenger.Gender,
                    passenger.SeatNo))
                .ToArray()))
            .ToArray();

        return Ok(response);
    }
}
