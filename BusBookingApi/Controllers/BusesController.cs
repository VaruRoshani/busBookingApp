using BusBookingApi.Data;
using BusBookingApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingApi.Controllers;

[ApiController]
[Route("api/buses")]
public sealed class BusesController(BusBookingDbContext db) : ControllerBase
{
    [HttpGet("cities")]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetCities()
    {
        var fromCities = await db.Buses.AsNoTracking().Select(bus => bus.FromLocation).Distinct().ToListAsync();
        var toCities = await db.Buses.AsNoTracking().Select(bus => bus.ToLocation).Distinct().ToListAsync();
        var cities = fromCities.Concat(toCities).Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(city => city, StringComparer.OrdinalIgnoreCase).ToArray();
        return Ok(cities);
    }

    [HttpGet("search")]
    [ProducesResponseType<IReadOnlyList<BusSearchResultDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<BusSearchResultDto>>> Search(
        [FromQuery(Name = "from")] string? from,
        [FromQuery(Name = "to")] string? to,
        [FromQuery] DateOnly? date)
    {
        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
        {
            return BadRequest(new { message = "Both 'from' and 'to' locations are required." });
        }

        if (date is null)
        {
            return BadRequest(new { message = "A travel date is required in yyyy-MM-dd format." });
        }

        var buses = await db.Buses.AsNoTracking()
            .Where(bus =>
                bus.FromLocation.ToLower() == from.Trim().ToLower() &&
                bus.ToLocation.ToLower() == to.Trim().ToLower())
            .OrderBy(bus => bus.DepartureTime)
            .ToListAsync();

        var busIds = buses.Select(bus => bus.BusId).ToArray();
        var bookedCounts = await db.BookingPassengers.AsNoTracking()
            .Where(passenger =>
                passenger.Booking.TravelDate == date.Value &&
                busIds.Contains(passenger.Booking.BusId))
            .GroupBy(passenger => passenger.Booking.BusId)
            .Select(group => new { BusId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(group => group.BusId, group => group.Count);

        var response = buses.Select(bus =>
        {
            var booked = bookedCounts.GetValueOrDefault(bus.BusId);
            return new BusSearchResultDto(
                bus.BusId,
                bus.BusName,
                bus.BusNumber,
                bus.BusType,
                bus.FromLocation,
                bus.ToLocation,
                bus.DepartureTime,
                bus.ArrivalTime,
                bus.PricePerSeat,
                bus.TotalSeats,
                bus.Rating,
                Math.Max(0, bus.TotalSeats - booked));
        }).ToArray();

        return Ok(response);
    }

    [HttpGet("{busId:int}")]
    [ProducesResponseType<BusDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BusDto>> GetBus(int busId)
    {
        var bus = await db.Buses.AsNoTracking()
            .Where(candidate => candidate.BusId == busId)
            .Select(candidate => new BusDto(
                candidate.BusId,
                candidate.BusName,
                candidate.BusNumber,
                candidate.BusType,
                candidate.FromLocation,
                candidate.ToLocation,
                candidate.DepartureTime,
                candidate.ArrivalTime,
                candidate.PricePerSeat,
                candidate.TotalSeats,
                candidate.Rating))
            .SingleOrDefaultAsync();

        return bus is null ? NotFound(new { message = "Bus was not found." }) : Ok(bus);
    }

    [HttpGet("{busId:int}/booked-seats")]
    [ProducesResponseType<BookedSeatsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookedSeatsDto>> GetBookedSeats(int busId, [FromQuery] DateOnly? travelDate)
    {
        if (travelDate is null)
        {
            return BadRequest(new { message = "A travel date is required in yyyy-MM-dd format." });
        }

        var busExists = await db.Buses.AnyAsync(bus => bus.BusId == busId);
        if (!busExists)
        {
            return NotFound(new { message = "Bus was not found." });
        }

        var seatNumbers = await db.BookingPassengers.AsNoTracking()
            .Where(passenger =>
                passenger.Booking.BusId == busId &&
                passenger.Booking.TravelDate == travelDate.Value)
            .OrderBy(passenger => passenger.SeatNo)
            .Select(passenger => passenger.SeatNo)
            .ToListAsync();

        return Ok(new BookedSeatsDto(busId, travelDate.Value, seatNumbers));
    }
}
