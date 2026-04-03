using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Ragnor_Fitness_Arena.Models;

namespace Ragnor_Fitness_Arena.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;

        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}


        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // HOME


        // ABOUT
        public IActionResult About()
        {
            return View("about");
        }

        // BLOG
        public IActionResult Blog()
        {
            return View("blog");
        }

        // BLOG DETAILS
        public IActionResult BlogDetails()
        {
            return View("blogdetails");
        }

        // BMI CALCULATOR
        public IActionResult BmiCalculator()
        {
            return View("bmicalculator");
        }

        // CLASS TIMETABLE
        public IActionResult ClassTimetable()
        {
            return View("classtimetable");
        }

        // CLASS DETAILS
        public IActionResult ClassDetails()
        {
            return View("classdetails");
        }

        // CONTACT
        public IActionResult Contact()
        {
            return View("contact");
        }

        // GALLERY
        public IActionResult Gallery()
        {
            return View("gallery");
        }

        // SERVICE
       

        // TEAM
        public IActionResult Team()
        {
            return View("team");
        }

        // PRIVACY
        public IActionResult Privacy()
        {
            return View("Privacy");
        }

        // 404 PAGE
        public IActionResult Error404()
        {
            return View("Error404");
        }

        public IActionResult Error()
        {
            return Content("Something went wrong. Please try again later.");
        }
        // DEFAULT ERROR
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel
        //    {
        //        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        //    });
        //}

        //public IActionResult Index()
        //{
        //    List<MembershipPlan> plans = new List<MembershipPlan>();

        //    string conStr = _configuration.GetConnectionString("DefaultConnection");

        //    using (SqlConnection con = new SqlConnection(conStr))
        //    {
        //        string query = "SELECT PlanId, PlanName, Price, Duration, Features FROM MembershipPlans";
        //        SqlCommand cmd = new SqlCommand(query, con);

        //        con.Open();
        //        SqlDataReader reader = cmd.ExecuteReader();

        //        while (reader.Read())
        //        {
        //            plans.Add(new MembershipPlan
        //            {
        //                PlanId = (int)reader["PlanId"],
        //                PlanName = reader["PlanName"].ToString(),
        //                Price = (decimal)reader["Price"],
        //                Duration = reader["Duration"].ToString(),
        //                Features = reader["Features"].ToString()
        //            });
        //        }
        //    }

        //    return View(plans);
        //}
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult BookTrial(string FullName, string PhoneNumber, DateTime PreferredDate, string Message)
        {
            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection con = new SqlConnection(conStr))
            {
                string query = @"INSERT INTO TrialBookings 
                        (FullName, PhoneNumber, PreferredDate, Message) 
                        VALUES 
                        (@n, @p, @d, @m)";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@n", FullName);
                cmd.Parameters.AddWithValue("@p", PhoneNumber);
                cmd.Parameters.AddWithValue("@d", PreferredDate);
                cmd.Parameters.AddWithValue("@m", Message);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Trial Booking Submitted!";   
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult BookTrainerAppointment(TrainerAppointment model)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = @"INSERT INTO TrainerAppointments
                        (FullName,PhoneNumber,TrainerName,AppointmentDate,AppointmentTime,Message)
                        VALUES
                        (@FullName,@PhoneNumber,@TrainerName,@AppointmentDate,@AppointmentTime,@Message)";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@FullName", model.FullName);
                cmd.Parameters.AddWithValue("@PhoneNumber", model.PhoneNumber);
                cmd.Parameters.AddWithValue("@TrainerName", model.TrainerName);
                cmd.Parameters.AddWithValue("@AppointmentDate", model.AppointmentDate);
                cmd.Parameters.AddWithValue("@AppointmentTime", model.AppointmentTime);
                cmd.Parameters.AddWithValue("@Message", model.Message);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Appointment Booked Successfully";
            return RedirectToAction("Index");
        }
        public IActionResult Service()
        {
            List<MembershipPlan> plans = new List<MembershipPlan>();

            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection con = new SqlConnection(conStr))
            {
                string query = "SELECT * FROM MembershipPlans";
                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    plans.Add(new MembershipPlan
                    {
                        PlanId = (int)reader["PlanId"],
                        PlanName = reader["PlanName"].ToString(),
                        Price = (decimal)reader["Price"],
                        Duration = reader["Duration"].ToString(),
                        Features = reader["Features"].ToString()
                    });
                }
            }

            return View(plans);
        }

        [HttpPost]
        public IActionResult BookTrainer(string Name, string Phone, string Trainer, DateTime Date, string Time)
        {
            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection con = new SqlConnection(conStr))
            {
                string query = "INSERT INTO TrainerAppointments (Name, Phone, Trainer, Date, Time) VALUES (@n,@p,@t,@d,@ti)";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@n", Name);
                cmd.Parameters.AddWithValue("@p", Phone);
                cmd.Parameters.AddWithValue("@t", Trainer);
                cmd.Parameters.AddWithValue("@d", Date);
                cmd.Parameters.AddWithValue("@ti", Time);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Appointment Booked!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ContactSubmit(string Name, string Email, string Comment)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                TempData["Error"] = "Please login to send message!";
                return RedirectToAction("Contact");
            }

            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection con = new SqlConnection(conStr))
            {
                string query = @"INSERT INTO Contacts 
(Name, Email, Comment, CreatedAt, Status) 
VALUES 
(@n, @e, @c, @d, 'New')";
                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@n", Name);
                cmd.Parameters.AddWithValue("@e", Email);
                //cmd.Parameters.AddWithValue("@w", Website);
                cmd.Parameters.AddWithValue("@c", Comment);
                cmd.Parameters.AddWithValue("@d", DateTime.Now);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Message sent successfully!";
            return RedirectToAction("Contact");
        }
    }
}
