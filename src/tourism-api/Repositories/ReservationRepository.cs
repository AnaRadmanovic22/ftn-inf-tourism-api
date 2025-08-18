using System.Data;
using Microsoft.Data.Sqlite;
using tourism_api.Domain;

namespace tourism_api.Repositories;

public class ReservationRepository
{
    private readonly string _connectionString;

    public ReservationRepository(IConfiguration configuration)
    {
        _connectionString = configuration["ConnectionString:SQLiteConnection"];
    }


    public List<Reservation> GetByTourist(int touristId)
    {
        var result = new List<Reservation>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Reservation WHERE TouristId = @touristId";
        command.Parameters.AddWithValue("@touristId", touristId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(Map(reader));
        }
        return result;
    }



    public List<Reservation> GetByRestaurantDateAndMeal(int restaurantId, DateTime date, string mealType)
    {
        var result = new List<Reservation>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Reservation
            WHERE RestaurantId = @restaurantId AND Date = @date AND MealType = @mealType";
        command.Parameters.AddWithValue("@restaurantId", restaurantId);
        command.Parameters.AddWithValue("@date", date.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@mealType", mealType);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(Map(reader));
        }
        return result;
    }

    public Reservation Create(Reservation reservation)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Reservation (RestaurantId, TouristId, Date, MealType, NumberOfGuests)
            VALUES (@restaurantId, @touristId, @date, @mealType, @numberOfGuests);
            SELECT last_insert_rowid();
        ";
        command.Parameters.AddWithValue("@restaurantId", reservation.RestaurantId);
        command.Parameters.AddWithValue("@touristId", reservation.TouristId);
        command.Parameters.AddWithValue("@date", reservation.Date.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@mealType", reservation.MealType);
        command.Parameters.AddWithValue("@numberOfGuests", reservation.NumberOfGuests);

        reservation.Id = (int)(long)command.ExecuteScalar();
        return reservation;
    }

    public bool Delete(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Reservation WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        return command.ExecuteNonQuery() > 0;
    }

    public Reservation? GetById(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Reservation WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();
        if (reader.Read())
            return Map(reader);
        return null;
    }

    private Reservation Map(IDataReader reader)
    {
        return new Reservation
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            RestaurantId = reader.GetInt32(reader.GetOrdinal("RestaurantId")),
            TouristId = reader.GetInt32(reader.GetOrdinal("TouristId")),
            Date = DateTime.Parse(reader.GetString(reader.GetOrdinal("Date"))),
            MealType = Enum.Parse<MealType>(reader.GetString(reader.GetOrdinal("MealType"))),
            NumberOfGuests = reader.GetInt32(reader.GetOrdinal("NumberOfGuests"))
        };
    }
}

