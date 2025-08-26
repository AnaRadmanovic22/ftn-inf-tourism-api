namespace tourism_api.Domain
{
    public class Review
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public int TouristId { get; set; }
        public int Stars {  get; set; } //1..5
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsValid()
        {
            return RestaurantId > 0 && TouristId > 0 && Stars >= 1 && Stars <= 5; 
        }
    }
}
