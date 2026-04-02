namespace Ragnor_Fitness_Arena.Models
{
    public class UserMembershipViewModel
    {
        public string PlanName { get; set; }
        public decimal Price { get; set; }
        public string Duration { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; }
    }

}
