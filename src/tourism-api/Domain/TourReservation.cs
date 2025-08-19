using System.Xml.Linq;

namespace tourism_api.Domain
{
    public class TourReservation
    {
        public int Id { get; set;}
        public int TourId { get; set; }
        public int UserId { get; set; }
        public int NumberOfPeople { get; set; }

        public bool IsValid()
        {
            return NumberOfPeople > 0 && TourId > 0 && UserId> 0;
        }
    }
}
