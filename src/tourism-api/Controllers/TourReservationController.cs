using Microsoft.AspNetCore.Mvc;
using tourism_api.Domain;
using tourism_api.Repositories;

namespace tourism_api.Controllers;

[Route("api/reservations")]
[ApiController]
public class TourReservationController : ControllerBase
{
    private readonly TourReservationRepository _reservationRepo;
    private readonly TourRepository _tourRepo;
    private readonly UserRepository _userRepo;

    public TourReservationController(IConfiguration configuration)
    {
        _reservationRepo = new TourReservationRepository(configuration);
        _tourRepo = new TourRepository(configuration);
        _userRepo = new UserRepository(configuration);
    }

    [HttpGet]
    public ActionResult<List<TourReservation>> Get([FromQuery] int? tourId, [FromQuery] int? userId)
    {
        try
        {
            if (tourId.HasValue)
            {
                List<TourReservation> reservations = _reservationRepo.GetByTourId(tourId.Value);
                return Ok(reservations);
            }
            else if (userId.HasValue)
            {
                List<TourReservation> reservations = _reservationRepo.GetByUserId(userId.Value);
                return Ok(reservations);
            }
            else
            {
                // Ako nijedan filter nije poslat, vrati sve (ili 400 BadRequest)
                return BadRequest("You must provide either tourId or userId.");
            }
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while fetching the reservations.");
        }
    }


    [HttpPost]
    public ActionResult<TourReservation> Create([FromBody] TourReservation reservation)
    {
        if (reservation.NumberOfPeople <= 0)
        {
            return BadRequest("Number of people must be greater than zero.");
        }

        try
        {
            Tour tour = _tourRepo.GetById(reservation.TourId);
            if (tour == null)
            {
                return NotFound($"Tour with ID {reservation.TourId} not found.");
            }

            User user = _userRepo.GetById(reservation.UserId);
            if (user == null)
            {
                return NotFound($"User with ID {reservation.UserId} not found.");
            }

            List<TourReservation> existingReservations = _reservationRepo.GetByTourId(reservation.TourId);
            int occupied = existingReservations.Sum(r => r.NumberOfPeople);
            int available = tour.MaxGuests - occupied;

            if (reservation.NumberOfPeople > available)
            {
                return BadRequest($"Only {available} spots are available for this tour.");
            }

            TourReservation created = _reservationRepo.Create(reservation);
            return Ok(created);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while creating the reservation.");
        }
    }

    [HttpDelete("{reservationId}")]
    public ActionResult Delete(int reservationId)
    {
        try
        {
            TourReservation reservation = _reservationRepo.GetById(reservationId);
            if (reservation == null)
                return NotFound($"Reservation with ID {reservationId} not found.");

            Tour tour = _tourRepo.GetById(reservation.TourId);
            if (tour == null)
                return NotFound($"Tour with ID {reservation.TourId} not found.");

            DateTime nowUtc = DateTime.UtcNow;
            DateTime tourDateUtc = tour.DateTime.ToUniversalTime();
            DateTime deadline = tourDateUtc.AddHours(-24);

            Console.WriteLine($"Now UTC: {nowUtc}");
            Console.WriteLine($"Tour UTC: {tourDateUtc}");
            Console.WriteLine($"Deadline (24h before tour): {deadline}");

            // Dozvoli brisanje ako je trenutno vreme pre 24h pre ture
            // ili ako je tura prošla (sada >= tourDateUtc)
            if (nowUtc < deadline || nowUtc >= tourDateUtc)
            {
                bool isDeleted = _reservationRepo.Delete(reservationId);
                if (isDeleted)
                    return Ok();
                else
                    return NotFound("Failed to delete the reservation.");
            }

            return BadRequest("Cannot cancel reservation less than 24 hours before the tour.");
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while deleting the reservation.");
        }
    }


}
