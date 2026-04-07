    

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ragnor_Fitness_Arena.Models;

namespace Ragnor_Fitness_Arena.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;

        // ✅ SINGLE CONSTRUCTOR (FIXED)
        public HomeController(IConfiguration configuration, ILogger<HomeController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // ================= BASIC PAGES =================

        public IActionResult About() => View("about");
        public IActionResult Blog() => View("blog");
        public IActionResult BlogDetails() => View("blogdetails");
        public IActionResult BmiCalculator() => View("bmicalculator");
        public IActionResult ClassTimetable() => View("classtimetable");
        public IActionResult ClassDetails() => View("classdetails");
        public IActionResult Contact() => View("contact");
        public IActionResult Gallery() => View("gallery");
        public IActionResult Team() => View("team");
        public IActionResult Privacy() => View("Privacy");
        public IActionResult Error404() => View("Error404");

        public IActionResult Error()
        {
            return Content("Something went wrong. Please try again later.");
        }

        // ================= HOME INDEX =================

        public IActionResult Index()
        {
            List<MembershipPlan> plans = new List<MembershipPlan>();

            try
            {
                string conStr = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(conStr))
                {
                    string query = "SELECT PlanId, PlanName, Price, Duration, Features FROM MembershipPlans";
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DB Error in Index");
                plans = new List<MembershipPlan>(); // safe fallback
            }

            return View(plans);
        }

        // ================= TRIAL BOOKING =================

        [HttpPost]
        public IActionResult BookTrial(string FullName, string PhoneNumber, DateTime PreferredDate, string Message)
        {
            try
            {
                string conStr = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(conStr))
                {
                    string query = @"INSERT INTO TrialBookings 
                                     (FullName, PhoneNumber, PreferredDate, Message) 
                                     VALUES (@n, @p, @d, @m)";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@n", FullName);
                    cmd.Parameters.AddWithValue("@p", PhoneNumber);
                    cmd.Parameters.AddWithValue("@d", PreferredDate);
                    cmd.Parameters.AddWithValue("@m", Message);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["Success"] = "Trial Booking Submitted!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trial Booking Error");
                TempData["Error"] = "Something went wrong!";
            }

            return RedirectToAction("Index");
        }

        // ================= TRAINER APPOINTMENT =================

        //[HttpPost]
        //public IActionResult BookTrainerAppointment(TrainerAppointment model)
        //{
        //    try
        //    {
        //        using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        //        {
        //            string query = @"INSERT INTO TrainerAppointments
        //                (FullName,PhoneNumber,TrainerName,AppointmentDate,AppointmentTime,Message)
        //                VALUES (@FullName,@PhoneNumber,@TrainerName,@AppointmentDate,@AppointmentTime,@Message)";

        //            SqlCommand cmd = new SqlCommand(query, con);
        //            cmd.Parameters.AddWithValue("@FullName", model.FullName);
        //            cmd.Parameters.AddWithValue("@PhoneNumber", model.PhoneNumber);
        //            cmd.Parameters.AddWithValue("@TrainerName", model.TrainerName);
        //            cmd.Parameters.AddWithValue("@AppointmentDate", model.AppointmentDate);
        //            cmd.Parameters.AddWithValue("@AppointmentTime", model.AppointmentTime);
        //            cmd.Parameters.AddWithValue("@Message", model.Message);

        //            con.Open();
        //            cmd.ExecuteNonQuery();
        //        }

        //        TempData["Success"] = "Appointment Booked Successfully";
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Trainer Booking Error");
        //        TempData["Error"] = "Something went wrong!";
        //    }

        //    return RedirectToAction("Index");
        //}

        //[HttpPost]
        //public IActionResult BookTrainerAppointment(TrainerAppointment model)
        //{
        //    try
        //    {
        //         🔍 DEBUG MODEL CHECK
        //        if (model == null || string.IsNullOrEmpty(model.FullName))
        //        {
        //            return Content("Model NOT received properly ❌");
        //        }

        //        string conStr = _configuration.GetConnectionString("DefaultConnection");

        //        using (SqlConnection con = new SqlConnection(conStr))
        //        {
        //            string query = @"INSERT INTO TrainerAppointments
        //    (FullName,PhoneNumber,TrainerName,AppointmentDate,AppointmentTime,Message)
        //    VALUES (@FullName,@PhoneNumber,@TrainerName,@AppointmentDate,@AppointmentTime,@Message)";

        //            SqlCommand cmd = new SqlCommand(query, con);

        //            cmd.Parameters.AddWithValue("@FullName", model.FullName ?? "");
        //            cmd.Parameters.AddWithValue("@PhoneNumber", model.PhoneNumber ?? "");
        //            cmd.Parameters.AddWithValue("@TrainerName", model.TrainerName ?? "");
        //            cmd.Parameters.AddWithValue("@AppointmentDate", model.AppointmentDate);
        //            cmd.Parameters.AddWithValue("@AppointmentTime", model.AppointmentTime ?? "");
        //            cmd.Parameters.AddWithValue("@Message", model.Message ?? "");

        //            con.Open();

        //            int rows = cmd.ExecuteNonQuery();

        //             🔥 IMPORTANT DEBUG OUTPUT
        //            if (rows > 0)
        //            {
        //                return Content("✅ Insert SUCCESS");
        //            }
        //            else
        //            {
        //                return Content("❌ Insert FAILED");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content("🔥 ERROR: " + ex.Message);
        //    }
        //}

        [HttpPost]
        public IActionResult BookTrainerAppointment(TrainerAppointment model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.FullName))
                {
                    return Content("Model NOT received properly ❌");
                }

                string conStr = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(conStr))
                {
                    string query = @"INSERT INTO TrainerAppointments
            (FullName,PhoneNumber,TrainerName,AppointmentDate,AppointmentTime,Message)
            VALUES (@FullName,@PhoneNumber,@TrainerName,@AppointmentDate,@AppointmentTime,@Message)";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@FullName", model.FullName ?? "");
                    cmd.Parameters.AddWithValue("@PhoneNumber", model.PhoneNumber ?? "");
                    cmd.Parameters.AddWithValue("@TrainerName", model.TrainerName ?? "");
                    cmd.Parameters.AddWithValue("@AppointmentDate", model.AppointmentDate);
                    cmd.Parameters.AddWithValue("@AppointmentTime", model.AppointmentTime ?? "");
                    cmd.Parameters.AddWithValue("@Message", model.Message ?? "");

                    con.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                    {
                        return Content("SUCCESS");
                    }
                    else
                    {
                        return Content("FAILED");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content("ERROR: " + ex.Message);
            }
        }

        // ================= SERVICE PAGE =================

        public IActionResult Service()
        {
            List<MembershipPlan> plans = new List<MembershipPlan>();

            try
            {
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service Page Error");
            }

            return View(plans);
        }

        // ================= CONTACT =================

        [HttpPost]
        public IActionResult ContactSubmit(string Name, string Email, string Comment)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                TempData["Error"] = "Please login to send message!";
                return RedirectToAction("Contact");
            }

            try
            {
                string conStr = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(conStr))
                {
                    string query = @"INSERT INTO Contacts 
                        (Name, Email, Comment, CreatedAt, Status) 
                        VALUES (@n, @e, @c, @d, 'New')";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@n", Name);
                    cmd.Parameters.AddWithValue("@e", Email);
                    cmd.Parameters.AddWithValue("@c", Comment);
                    cmd.Parameters.AddWithValue("@d", DateTime.Now);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["Success"] = "Message sent successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Contact Error");
                TempData["Error"] = "Something went wrong!";
            }

            return RedirectToAction("Contact");
        }
    }
}
