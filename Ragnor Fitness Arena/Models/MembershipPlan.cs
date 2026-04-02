using System;

namespace Ragnor_Fitness_Arena.Models
{
    public class MembershipPlan
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; }
        public decimal Price { get; set; }
        public string Duration { get; set; }
        public string Features { get; set; }

    }
}
