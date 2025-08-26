namespace tourism_api.Domain
{
    public enum MealType
    {
        Breakfast, // Doručak
        Lunch,     // Ručak
        Dinner     // Večera
    }

    public class Reservation
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public int TouristId { get; set; }
        public DateTime Date { get; set; }
        public MealType MealType { get; set; }
        public int NumberOfGuests { get; set; }
    }


}
