using Microsoft.AspNetCore.Mvc;
using tourism_api.Domain;
using tourism_api.Repositories;

namespace tourism_api.Controllers
{
    [Route("api/restaurants")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly ReviewRepository reviewRepository;
        private readonly RestaurantRepository restaurantRepository;
        private readonly ReservationRepository reservationRepository;

        public ReviewController(IConfiguration configuration)
        {
            reviewRepository = new ReviewRepository(configuration);
            restaurantRepository = new RestaurantRepository(configuration);
            reservationRepository = new ReservationRepository(configuration);
        }
        //POST /api/restaurants/{restaurantId}/reviews
        [HttpPost("{restaurantId}/reviews")]
        public IActionResult CreateReview(int restaurantId, [FromBody] ReviewDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Nedostaju podaci o oceni. " });

            if (dto.TouristId <= 0 || dto.Stars < 1 || dto.Stars > 5)
                return BadRequest(new { message = "Nevalidni podaci (touristId i/ili broj zvezdica 1-5)." });

            var restaurant = restaurantRepository.GetById(restaurantId);
            if (restaurant == null)
                return NotFound(new { message = "Restoran nije pronadjen." });

            //Provera da li je dozvoljeno ocenjivanje (>=1h posle termina i <=3 dana)

            var allowed = reservationRepository.TouristCanReviewRestaurant(dto.TouristId, restaurantId, DateTime.Now);
            if (!allowed)
                return StatusCode(403, new { message = "Ocenjivanje je dozvoljeno od 1h nakon termina do 3 dana posle posete." });

            var review = new Review
            {
                RestaurantId = restaurantId,
                TouristId = dto.TouristId,
                Stars = dto.Stars,
                Comment = dto.Comment,
                CreatedAt = DateTime.Now,
            };

            try
            {
                var created = reviewRepository.Create(review);
                return Ok(created);
            }
            catch
            {
                return Problem("Greska prilikom cuvanja ocene");
            }
        }
        // GET /api/restaurants/{restaurantID}/reviews
        [HttpGet("{restaurantId}/reviews")]
        public IActionResult GetReviewsForRestaurant(int restaurantId)
        {
            var restaurant = restaurantRepository.GetById(restaurantId);
            if (restaurant == null)
                return NotFound(new { message = "Restoran nije pronadjen." });

            var list = reviewRepository.GetByRestaurant(restaurantId);
            return Ok(list);
        }
    }

    public class ReviewDto
    {
        public int TouristId { get; set; }
        public int Stars { get; set; }

        public string? Comment { get; set; }    
    }
}
