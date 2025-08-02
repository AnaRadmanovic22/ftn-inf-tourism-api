using Microsoft.Data.Sqlite;
using tourism_api.Domain;

namespace tourism_api.Repositories
{
    public class TourReservationRepository
    {
        private readonly string _connectionString;

        public TourReservationRepository(IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionString:SQLiteConnection"];
        }

        public TourReservation Create(TourReservation reservation)
        {
            try {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = @"INSERT INTO TourReservations (TourId, UserId, NumberOfPeople)
                                VALUES (@TourId,@UserId,@NumberOfPeople);
                                SELECT LAST_INSERT_ROWID();";
                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@TourId", reservation.TourId );
                command.Parameters.AddWithValue("@UserId", reservation.UserId);
                command.Parameters.AddWithValue("@NumberOfPeople", reservation.NumberOfPeople);

                reservation.Id = Convert.ToInt32(command.ExecuteScalar());

                return reservation;

            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Greška u konverziji podataka iz baze: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Konekcija nije otvorena ili je otvorena više puta: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }

        public bool Delete(int id)
        {
            try{
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = "DELETE FROM TourReservations WHERE Id = @Id";

                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                return command.ExecuteNonQuery() > 0;
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Greška u konverziji podataka iz baze: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Konekcija nije otvorena ili je otvorena više puta: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }

        public TourReservation GetById(int id)
        {
            TourReservation reservation = null;
            try
            { 
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = "SELECT * FROM TourReservations WHERE Id = @Id";
                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                using SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    return new TourReservation
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        TourId = Convert.ToInt32(reader["TourId"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        NumberOfPeople = Convert.ToInt32(reader["NumberOfPeople"])
                    };
                }
                return reservation;
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Greška u konverziji podataka iz baze: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Konekcija nije otvorena ili je otvorena više puta: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }

        public List<TourReservation> GetByTourId(int tourId)
        {
            List<TourReservation> reservations = new List<TourReservation>();

            try{

                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = $@"SELECT * FROM TourReservations WHERE TourId = @TourId";

                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@TourId", tourId);

                using SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    reservations.Add(new TourReservation
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        TourId = Convert.ToInt32(reader["TourId"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        NumberOfPeople = Convert.ToInt32(reader["NumberOfPeople"])
                    });
                }
                return reservations;
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Greška u konverziji podataka iz baze: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Konekcija nije otvorena ili je otvorena više puta: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }

        public List<TourReservation> GetByUserId(int userId)
        {
            List<TourReservation> reservations = new List<TourReservation>();

            try
            {
                using SqliteConnection connection = new SqliteConnection(_connectionString);
                connection.Open();

                string query = $@"SELECT * FROM TourReservations WHERE UserId = @UserId";

                using SqliteCommand command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    reservations.Add(new TourReservation
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        TourId = Convert.ToInt32(reader["TourId"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        NumberOfPeople = Convert.ToInt32(reader["NumberOfPeople"])
                    });
                }
                return reservations;
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"Greška pri konekciji ili izvršavanju neispravnih SQL upita: {ex.Message}");
                throw;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Greška u konverziji podataka iz baze: {ex.Message}");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Konekcija nije otvorena ili je otvorena više puta: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                throw;
            }
        }
    }
}

