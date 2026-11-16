using BusBookingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BusBookingApi.Data;

public static class DbInitializer
{
    private static readonly (string Name, double Latitude, double Longitude)[] Cities =
    [
        ("Pune", 18.5204, 73.8567), ("Mumbai", 19.0760, 72.8777), ("Nagpur", 21.1458, 79.0882),
        ("Surat", 21.1702, 72.8311), ("Ahmedabad", 23.0225, 72.5714), ("Bhavnagar", 21.7645, 72.1519),
        ("Rajkot", 22.3039, 70.8022), ("Vadodara", 22.3072, 73.1812), ("Nashik", 19.9975, 73.7898),
        ("Indore", 22.7196, 75.8577), ("Bengaluru", 12.9716, 77.5946), ("Hyderabad", 17.3850, 78.4867),
        ("Chennai", 13.0827, 80.2707), ("Delhi", 28.6139, 77.2090), ("Jaipur", 26.9124, 75.7873)
    ];

    private static readonly string[] Operators =
    ["National Travels", "Silverline Coaches", "CityLink Express", "Royal Roadways", "Sunrise Travels", "GreenLine Bus"];

    private static readonly string[] Types = ["AC Sleeper (2+1)", "AC Seater (2+2)", "Non-AC Seater"];

    public static async Task SeedBusesAsync(BusBookingDbContext db)
    {
        var existingNumbers = await db.Buses.Select(bus => bus.BusNumber).ToListAsync();
        var knownNumbers = existingNumbers.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var additions = new List<Bus>();
        var seedIndex = 1;

        for (var fromIndex = 0; fromIndex < Cities.Length; fromIndex++)
        for (var toIndex = 0; toIndex < Cities.Length; toIndex++)
        {
            if (fromIndex == toIndex) continue;

            var distance = RoadDistance(Cities[fromIndex], Cities[toIndex]);
            for (var busIndex = 0; busIndex < 3; busIndex++)
            {
                var number = $"SEED{seedIndex:D5}";
                seedIndex++;
                if (!knownNumbers.Add(number)) continue;

                var departureMinutes = (5 * 60 + (fromIndex * 97 + toIndex * 43 + busIndex * 390) % (18 * 60));
                var durationMinutes = Math.Max(90, (int)(distance / (busIndex == 0 ? 58 : 65) * 60));
                var arrivalMinutes = (departureMinutes + durationMinutes) % (24 * 60);
                var multiplier = busIndex switch { 0 => 1.8m, 1 => 1.35m, _ => 0.95m };
                var price = Math.Round(Math.Max(250m, (decimal)distance * multiplier) / 50m, MidpointRounding.AwayFromZero) * 50m;
                var rating = 3.5m + ((fromIndex * 7 + toIndex * 3 + busIndex * 2) % 14) / 10m;

                additions.Add(new Bus
                {
                    BusName = Operators[(fromIndex * 3 + toIndex + busIndex) % Operators.Length],
                    BusNumber = number,
                    BusType = Types[busIndex],
                    FromLocation = Cities[fromIndex].Name,
                    ToLocation = Cities[toIndex].Name,
                    DepartureTime = new TimeOnly(departureMinutes / 60, departureMinutes % 60),
                    ArrivalTime = new TimeOnly(arrivalMinutes / 60, arrivalMinutes % 60),
                    PricePerSeat = price,
                    TotalSeats = 40,
                    Rating = rating
                });
            }
        }

        if (additions.Count == 0) return;
        db.Buses.AddRange(additions);
        await db.SaveChangesAsync();
    }

    private static double RoadDistance((string Name, double Latitude, double Longitude) from,
        (string Name, double Latitude, double Longitude) to)
    {
        const double radius = 6371;
        var latitudeDelta = Degrees(to.Latitude - from.Latitude);
        var longitudeDelta = Degrees(to.Longitude - from.Longitude);
        var a = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2) +
                Math.Cos(Degrees(from.Latitude)) * Math.Cos(Degrees(to.Latitude)) *
                Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2);
        return radius * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)) * 1.35;
    }

    private static double Degrees(double value) => value * Math.PI / 180;
}
