namespace Ragnor_Fitness_Arena.Models
{
    public class TrialBooking
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime PreferredDate { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
    }
}
