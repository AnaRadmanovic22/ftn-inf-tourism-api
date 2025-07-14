using Microsoft.AspNetCore.Mvc;
using tourism_api.Domain;
using tourism_api.Repositories;

namespace tourism_api.Controllers;

[Route("api/restaurants")]
[ApiController]
public class RestaurantController : ControllerBase
{
    private readonly RestaurantRepository _restaurantRepo;
    private readonly UserRepository _userRepo;

    public RestaurantController(IConfiguration configuration)
    {
        _restaurantRepo = new RestaurantRepository(configuration);
        _userRepo = new UserRepository(configuration);
    }

    [HttpGet]
    public ActionResult<List<Restaurant>> GetPaged([FromQuery] int ownerId = 0, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string orderBy = "Name", [FromQuery] string orderDirection = "ASC")
    {
        if(ownerId > 0)
        {
            return Ok(_restaurantRepo.GetByOwner(ownerId));
        }

        // Validacija za orderBy i orderDirection
        List<string> validOrderByColumns = new List<string> { "Name", "Description", "Capacity" }; // Lista dozvoljenih kolona za sortiranje
        if (!validOrderByColumns.Contains(orderBy))
        {
            orderBy = "Name"; // Default vrednost
        }

        List<string> validOrderDirections = new List<string> { "ASC", "DESC" }; // Lista dozvoljenih smerova
        if (!validOrderDirections.Contains(orderDirection))
        {
            orderDirection = "ASC"; // Default vrednost
        }

        try
        {
            List<Restaurant> restaurants = _restaurantRepo.GetPaged(page, pageSize, orderBy, orderDirection);
            int totalCount = _restaurantRepo.CountAll();
            Object result = new
            {
                Data = restaurants,
                TotalCount = totalCount
            };
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while fetching the restaurants.");
        }
    }

    [HttpGet("{id}")]
    public ActionResult<Restaurant> GetById(int id)
    {
        try
        {
            Restaurant restaurant = _restaurantRepo.GetById(id);
            if (restaurant == null)
            {
                return NotFound($"Restaurant with ID {id} not found.");
            }
            return Ok(restaurant);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while fetching the restaurant.");
        }
    }

    [HttpPost]
    public ActionResult<Restaurant> Create([FromBody] Restaurant newRestaurant)
    {
        if (!newRestaurant.IsValid())
        {
            return BadRequest("Invalid restaurant data.");
        }

        try
        {
            User user = _userRepo.GetById(newRestaurant.OwnerId);
            if (user == null)
            {
                return NotFound($"User with ID {newRestaurant.OwnerId} not found.");
            }

            Restaurant createdRestaurant = _restaurantRepo.Create(newRestaurant);
            return Ok(createdRestaurant);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while creating the restaurant.");
        }
    }

    [HttpPut("{id}")]
    public ActionResult<Restaurant> Update(int id, [FromBody] Restaurant restaurant)
    {

        //if (!restaurant.IsValid())
        //{
        //    return BadRequest("Invalid restaurant data.");
        //}

        try
        {
            restaurant.Id = id;

            
            if (restaurant.Status == "objavljen")
            {

                if (string.IsNullOrWhiteSpace(restaurant.ImageUrl) || restaurant.Meals == null || restaurant.Meals.Count < 5)

                {
                    return BadRequest("Restoran ne može biti objavljen bez slike i najmanje 5 jela.");
                }
            }
            else
            {
                
                restaurant.Status = (!string.IsNullOrWhiteSpace(restaurant.ImageUrl) && restaurant.Meals.Count >= 5)
                    ? "otvoren"
                    : "u pripremi";
            }

            Restaurant updatedRestaurant = _restaurantRepo.Update(restaurant);
            if (updatedRestaurant == null)
            {
                return NotFound($"Restaurant with ID {id} not found.");
            }
            return Ok(updatedRestaurant);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while updating the restaurant.");
        }
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        try
        {
            bool isDeleted = _restaurantRepo.Delete(id);
            if (isDeleted)
            {
                return NoContent();
            }
            return NotFound($"Restaurant with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while deleting the restaurant.");
        }
    }
}

