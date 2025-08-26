using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using tourism_api.Domain;
using tourism_api.Repositories;

namespace tourism_api.Controllers;

[Route("api/restaurants")]
[ApiController]
public class ReservationController : ControllerBase
{
    private readonly ReservationRepository _reservationRepo;
    private readonly RestaurantRepository _restaurantRepo;


    public ReservationController(IConfiguration configuration)
    {
        _reservationRepo = new ReservationRepository(configuration);
        _restaurantRepo = new RestaurantRepository(configuration);
    }

    // POST /api/restaurants/{restaurantId}/reservations
    [HttpPost("{restaurantId}/reservations")]
    public IActionResult CreateReservation(int restaurantId, [FromBody] ReservationDto dto)
    {        
        //osnovna validacija ulaza
        if (dto == null)
        {
            return BadRequest(new { message = "Nedostaju podaci o rezervaciji." });
        }
        //validacija podataka
        if (dto.TouristId <= 0 || dto.NumberOfGuests <= 0 || string.IsNullOrWhiteSpace(dto.Date) || string.IsNullOrWhiteSpace(dto.MealType))
            return BadRequest(new { message = "Nevalidni podaci: touristID, date, mealType i numberOfGuests su obavezni." });

        var restaurant = _restaurantRepo.GetById(restaurantId);
        if (restaurant == null)
           return NotFound(new
            {
                message = "Restoran nije pronadjen."
            });
        //parsiranje datuma bezbedno ("yyyy-MM-dd")
        if (!DateTime.TryParseExact(dto.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return BadRequest(new { message = "Nevalidan format datuma. Ocekivani format je yyyy-MM-dd." });
        //Parsiranje meal type bezbedno
        if (!Enum.TryParse<MealType>(dto.MealType, ignoreCase: true, out var mealTypeEnum))
            return BadRequest(new { message = "Nevalidan tip obroka. Dozvoljeno: Breakfast, Lunch, Dinner." });

        //sabiranje postojece rezervacije za taj dan i obrok
        var reservations = _reservationRepo.GetByRestaurantDateAndMeal(restaurantId, date, mealTypeEnum.ToString());
        int totalGuests = reservations.Sum(r => r.NumberOfGuests);
        //proveravanje kapaciteta
        var available = restaurant.Capacity - totalGuests;
        if (dto.NumberOfGuests > available)
        {
            // Nikad ne vraćaj negativan broj – max(0, available)
            var safeAvailable = Math.Max(0, available);
            return BadRequest(new { message = $"Nema dovoljno mesta. Maksimalno mozete rezervisati jos {safeAvailable} mesto/a za ovaj termin." });
        }

        var reservation = new Reservation
        {
            RestaurantId = restaurantId,
            TouristId = dto.TouristId,
            Date = date,
            MealType = mealTypeEnum,
            NumberOfGuests = dto.NumberOfGuests
        };

        try
        {
            var created = _reservationRepo.Create(reservation);
            return Ok(created);
        }
        catch
        {
            return Problem("Greska prilikom kreiranja rezervacije.");
        }
    }

    // GET /api/restaurants/reservations?touristId=123
    [HttpGet("reservations")]
    public IActionResult GetReservationsByTourist([FromQuery] int touristId)
    {
        if (touristId <= 0)
            return BadRequest(new { message = "touristId je obavezan i mora biti > 0." });

        try
        {
            var reservations = _reservationRepo.GetByTourist(touristId);
            return Ok(reservations);
        }
        catch
        {
            return Problem("Doslo je do greske pri dohvatanju rezervacija.");
        }
    }

    // DELETE /api/restaurants/reservations/{reservationId}
    [HttpDelete("reservations/{reservationId}")]
    public IActionResult CancelReservation(int reservationId)
    {
        var reservation = _reservationRepo.GetById(reservationId);
        if (reservation == null)
            return NotFound(new { message = "Rezervacija nije pronađena." });

        // Izracunavanje vremena rezervacije (datum + vreme po obroku)
        var hour = reservation.MealType switch
        {
            MealType.Breakfast => 8,
            MealType.Lunch => 13,
            MealType.Dinner => 18,
            _ => 0
        };

        var reservationDateTime = reservation.Date.Date.AddHours(hour);
        var now = DateTime.Now;

        // Pravila otkazivanja
        var minHoursBefore = reservation.MealType == MealType.Breakfast ? 12 : 4;

        // Ako je "sada" POSLE dozvoljenog prozora -> zabrana
        if (now > reservationDateTime.AddHours(-minHoursBefore))
        {
            var msg = reservation.MealType == MealType.Breakfast
                ? "Dorucak se može otkazati najkasnije 12 sati ranije."
                : "Rucak i večera se mogu otkazati najkasnije 4 sata ranije.";
            return BadRequest(new { message = msg });
        }

        var deleted = _reservationRepo.Delete(reservationId);
        if (!deleted)
            return Problem("Greska prilikom otkazivanja rezervacije.");

        return Ok(new { message = "Rezervacija uspesno otkazana." });
    }
}

public class ReservationDto
{
    public int TouristId { get; set; }
    public string Date { get; set; } // "2025-08-01"
    public string MealType { get; set; } // "Breakfast" | "Lunch" | "Dinner"
    public int NumberOfGuests { get; set; }
}

