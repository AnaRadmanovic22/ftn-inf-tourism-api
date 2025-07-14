using Microsoft.AspNetCore.Mvc;
using tourism_api.Domain;
using tourism_api.Repositories;

namespace tourism_api.Controllers;

[Route("api/restaurants/{restaurantId}/meals")]
[ApiController]
public class MealController : ControllerBase
{
    private readonly RestaurantRepository _restaurantRepo;
    private readonly MealRepository _mealRepo;

    public MealController(IConfiguration configuration)
    {
        _restaurantRepo = new RestaurantRepository(configuration);
        _mealRepo = new MealRepository(configuration);
    }

    [HttpGet]
    public ActionResult<List<Meal>> GetByRestaurant(int restaurantId)
    {
        try
        {
            Restaurant restaurant = _restaurantRepo.GetById(restaurantId);
            if (restaurant == null)
            {
                return NotFound($"Restaurant with ID {restaurantId} not found.");
            }

            List<Meal> meals = _mealRepo.GetByRestaurantId(restaurantId);
            return Ok(meals);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while retrieving meals.");
        }
    }


    [HttpPost]
    public ActionResult<Meal> Create(int restaurantId, [FromBody] Meal newMeal)
    {
        if (!newMeal.IsValid())
        {
            return BadRequest("Invalid meal data.");
        }

        try
        {
            Restaurant restaurant = _restaurantRepo.GetById(restaurantId);
            if (restaurant == null)
            {
                return NotFound($"Restaurant with ID {restaurantId} not found.");
            }

            newMeal.RestaurantId = restaurantId;
            Meal createdMeal = _mealRepo.Create(newMeal);
            return Ok(createdMeal);
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while creating the meal.");
        }
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int restaurantId, int id)
    {
        try
        {
            Restaurant restaurant = _restaurantRepo.GetById(restaurantId);
            if (restaurant == null)
            {
                return NotFound($"Restaurant with ID {restaurantId} not found.");
            }

            bool isDeleted = _mealRepo.Delete(id);
            if (isDeleted)
            {
                return NoContent();
            }
            return NotFound($"Meal with ID {id} not found.");
        }
        catch (Exception ex)
        {
            return Problem("An error occurred while deleting the meal.");
        }
    }
}
