using Microsoft.Data.Sqlite;
using System.Data;
using tourism_api.Domain;

namespace tourism_api.Repositories
{
    public class ReviewRepository
    {
        private readonly string _cs;
        public ReviewRepository(IConfiguration configuration)
        {
            _cs = configuration["ConnectionString:SQLiteConnection"];
        }

        public Review Create(Review review)
        {
            using var connection = new SqliteConnection(_cs);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"INSERT INTO Reviews (RestaurantId, TouristId, Stars, Comment, CreatedAt) VALUES (@rid, @tid, @stars, @comment, @created);
            SELECT last_insert_rowid();";
            command.Parameters.AddWithValue("@rid", review.RestaurantId);
            command.Parameters.AddWithValue("@tid", review.TouristId);
            command.Parameters.AddWithValue("@stars", review.Stars);
            command.Parameters.AddWithValue("@comment", (object?)review.Comment ?? DBNull.Value);
            command.Parameters.AddWithValue("@created", review.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"));

            review.Id = (int)(long)command.ExecuteScalar();
            return review;
        }

        public List<Review> GetByRestaurant(int restaurantId)
        {
            var list = new List<Review>();
            using var connection = new SqliteConnection(_cs);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM Reviews WHERE RestaurantId=@rid ORDER BY CreatedAt DESC";
            command.Parameters.AddWithValue("@rid", restaurantId);
            using var rd = command.ExecuteReader();
            while (rd.Read())
            {
                list.Add(Map(rd));
            }
            return list;
        }
        public double? GetAverageForRestaurant(int restaurantId)
        {
            using var connection = new SqliteConnection(_cs);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT AVG(Stars) FROM Reviews Where RestaurantId=@rid";
            command.Parameters.AddWithValue("@rid", restaurantId);
            var obj = command.ExecuteScalar();
            if (obj == DBNull.Value || obj == null) return null;
            return Convert.ToDouble(obj);
        }

        private Review Map(IDataRecord record) => new Review
        {
            Id = Convert.ToInt32(record["Id"]),
            RestaurantId = Convert.ToInt32(record["RestaurantId"]),
            TouristId = Convert.ToInt32(record["TouristId"]),
            Stars = Convert.ToInt32(record["Stars"]),
            Comment = record["Comment"] == DBNull.Value ? null : record["Comment"].ToString(),
            CreatedAt = DateTime.Parse(record["CreatedAt"].ToString()!)
        };
    }
}
