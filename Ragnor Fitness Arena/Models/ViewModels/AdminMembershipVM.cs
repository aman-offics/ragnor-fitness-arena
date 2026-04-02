namespace Ragnor_Fitness_Arena.Models.ViewModels
{
    public class AdminMembershipVM
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PlanName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; }
        public int MembershipId { get; set; }
    }
}
