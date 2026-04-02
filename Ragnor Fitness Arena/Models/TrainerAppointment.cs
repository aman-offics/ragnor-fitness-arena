namespace Ragnor_Fitness_Arena.Models
{
    public class TrainerAppointment
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string TrainerName { get; set; }

        public DateTime AppointmentDate { get; set; }

        public string AppointmentTime { get; set; }

        public string Message { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
