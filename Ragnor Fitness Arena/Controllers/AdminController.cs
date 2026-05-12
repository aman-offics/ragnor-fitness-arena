using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Ragnor_Fitness_Arena.Models;
using Ragnor_Fitness_Arena.Models.ViewModels;


namespace Ragnor_Fitness_Arena.Controllers
{
    public class AdminController : Controller
    {

        private readonly IConfiguration _configuration;
        
        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("AdminLogin") != null)
            {
                return RedirectToAction("Dashboard");
            }

            return View();
        }



        //public IActionResult Dashboard()
        //{
        //    if (HttpContext.Session.GetString("AdminLogin") == null)
        //    {
        //        return RedirectToAction("Login", "Admin");
        //    }

        //    string conStr = _configuration.GetConnectionString("DefaultConnection");
        //    int userCount = 0;
        //    using (SqliteConnection conn = new SqliteConnection(conStr))
        //    {
        //        string query = "SELECT COUNT(*) FROM Users";
        //        SqliteCommand cmd = new SqliteCommand(query, conn);
        //        conn.Open();
        //        userCount = Convert.ToInt32(cmd.ExecuteScalar());
        //    }
        //    ViewBag.UserCount = userCount;
        //    return View();
        //}

        public IActionResult Dashboard()
        {
            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                con.Open();

                // Total Users
                SqliteCommand cmd1 = new SqliteCommand("SELECT COUNT(*) FROM Users", con);
                ViewBag.TotalUsers = Convert.ToInt32(cmd1.ExecuteScalar());

                // Total Plans
                SqliteCommand cmd2 = new SqliteCommand("SELECT COUNT(*) FROM MembershipPlans", con);
                //ViewBag.TotalPlans = (int)cmd2.ExecuteScalar();
                ViewBag.TotalPlans = Convert.ToInt32(cmd2.ExecuteScalar());

                // Total Contacts
                SqliteCommand cmd3 = new SqliteCommand("SELECT COUNT(*) FROM Contacts", con);
                //ViewBag.TotalContacts = (int)cmd3.ExecuteScalar();
                ViewBag.TotalContacts = Convert.ToInt32(cmd3.ExecuteScalar());

                // New Messages
                SqliteCommand cmd4 = new SqliteCommand("SELECT COUNT(*) FROM Contacts WHERE Status='New'", con);
                //ViewBag.NewMessages = (int)cmd4.ExecuteScalar();
                ViewBag.NewMessages = Convert.ToInt32(cmd4.ExecuteScalar());

                // Active Memberships
                SqliteCommand cmd5 = new SqliteCommand("SELECT COUNT(*) FROM UserMemberships WHERE ExpiryDate > datetime('now')", con);
                //ViewBag.ActiveMemberships = (int)cmd5.ExecuteScalar();
                ViewBag.ActiveMemberships = Convert.ToInt32(cmd5.ExecuteScalar());
                // 🔥 TOTAL TRIALS
                SqliteCommand cmd6 = new SqliteCommand("SELECT COUNT(*) FROM TrialBookings", con);
                //ViewBag.TotalTrials = (int)cmd6.ExecuteScalar();
                ViewBag.TotalTrials = Convert.ToInt32(cmd6.ExecuteScalar());

                // 🔥 PENDING TRIALS
                SqliteCommand cmd7 = new SqliteCommand("SELECT COUNT(*) FROM TrialBookings WHERE Status='Pending'", con);
                //ViewBag.PendingTrials = (int)cmd7.ExecuteScalar();
                ViewBag.PendingTrials = Convert.ToInt32(cmd7.ExecuteScalar());

                // 🔥 TOTAL APPOINTMENTS
                SqliteCommand cmd8 = new SqliteCommand("SELECT COUNT(*) FROM TrainerAppointments", con);
                //ViewBag.TotalAppointments = (int)cmd8.ExecuteScalar();
                ViewBag.TotalAppointments = Convert.ToInt32(cmd8.ExecuteScalar());

                // 🔥 PENDING APPOINTMENTS
                SqliteCommand cmd9 = new SqliteCommand("SELECT COUNT(*) FROM TrainerAppointments WHERE Status='Pending'", con);
                //ViewBag.PendingAppointments = (int)cmd9.ExecuteScalar();
                ViewBag.PendingAppointments = Convert.ToInt32(cmd9.ExecuteScalar());
            }

            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                string query = "SELECT COUNT(*) FROM Admins WHERE Username=@u AND Password=@p";
                SqliteCommand cmd = new SqliteCommand(query, con);

                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);

                con.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());

                if (count == 1)
                {
                    HttpContext.Session.SetString("AdminLogin", "true");
                    return RedirectToAction("Dashboard", "Admin");
                }
            }

            ViewBag.Error = "Invalid login";
            return View();
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Admin");
        }

       

        public IActionResult Users()
        {
            if (HttpContext.Session.GetString("AdminLogin") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            string conStr = _configuration.GetConnectionString("DefaultConnection");
            List<string[]> users = new List<string[]>();

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                string query = "SELECT UserId, FullName, Email, CreatedAt FROM Users";
                SqliteCommand cmd = new SqliteCommand(query, con);

                con.Open();
                SqliteDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    users.Add(new string[]
                    {
                dr["UserId"].ToString(),
                dr["FullName"].ToString(),
                dr["Email"].ToString(),
                dr["CreatedAt"].ToString()
                    });
                }
            }

            ViewBag.Users = users;
            return View();
        }


        public IActionResult AddUser()
        {
            if (HttpContext.Session.GetString("AdminLogin") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            return View();
        }

        [HttpPost]
        public IActionResult AddUser(String fullname, String password, String email)
        {
            if(HttpContext.Session.GetString("AdminLogin") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                string query = "INSERT INTO Users (FullName, Email, Password) VALUES (@f, @e, @p)";
                SqliteCommand cmd = new SqliteCommand(query, con);

                cmd.Parameters.AddWithValue("@f", fullname);
                cmd.Parameters.AddWithValue("@p", password);
                cmd.Parameters.AddWithValue("@e", email);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Users");
        }


        public IActionResult DeleteUser(int id)
        {
            if (HttpContext.Session.GetString("AdminLogin") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                string query = "DELETE FROM Users WHERE UserId = @id";
                SqliteCommand cmd = new SqliteCommand(query, con);

                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Users");
        }

        public IActionResult EditUser(int id)
        {
            if (HttpContext.Session.GetString("AdminLogin") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            string conStr = _configuration.GetConnectionString("DefaultConnection");

            string fullname = "";
            string email = "";

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                string query = "SELECT FullName, Email FROM Users WHERE UserId = @id";
                SqliteCommand cmd = new SqliteCommand(query, con);

                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                SqliteDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    fullname = dr["FullName"].ToString();
                    email = dr["Email"].ToString();
                }
            }

            ViewBag.UserId = id;
            ViewBag.FullName = fullname;
            ViewBag.Email = email;

            return View();
        }

        [HttpPost]
        public IActionResult EditUser(int userid, string fullname, string email)
        {
            if (HttpContext.Session.GetString("AdminLogin") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                string query = "UPDATE Users SET FullName=@f, Email=@e WHERE UserId=@id";
                SqliteCommand cmd = new SqliteCommand(query, con);

                cmd.Parameters.AddWithValue("@f", fullname);
                cmd.Parameters.AddWithValue("@e", email);
                cmd.Parameters.AddWithValue("@id", userid);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Users");
        }

        public IActionResult Plans()
        {

            if (HttpContext.Session.GetString("AdminLogin") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            List<MembershipPlan> plans = new List<MembershipPlan>();

            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                string query = "SELECT * FROM MembershipPlans";
                SqliteCommand cmd = new SqliteCommand(query, con);

                con.Open();
                SqliteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    plans.Add(new MembershipPlan
                    {
                        PlanId = Convert.ToInt32(reader["PlanId"]),
                        Price = Convert.ToDecimal(reader["Price"]),
                        //PlanId = (int)reader["PlanId"],
                        PlanName = reader["PlanName"].ToString(),
                        //Price = (decimal)reader["Price"],
                        Duration = reader["Duration"].ToString(),
                        Features = reader["Features"].ToString()
                    });
                }
            }

            return View(plans);
        }

        [HttpPost]
        public IActionResult AddPlan(MembershipPlan plan)
        {

            if (HttpContext.Session.GetString("AdminLogin") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                string query = "INSERT INTO MembershipPlans (PlanName, Price, Duration, Features) VALUES (@n, @p, @d, @f)";

                SqliteCommand cmd = new SqliteCommand(query, con);

                cmd.Parameters.AddWithValue("@n", plan.PlanName);
                cmd.Parameters.AddWithValue("@p", plan.Price);
                cmd.Parameters.AddWithValue("@d", plan.Duration);
                cmd.Parameters.AddWithValue("@f", plan.Features);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Plans");
        }

        public IActionResult MymbershipList(string search, string status)
        {
            if (HttpContext.Session.GetString("AdminLogin") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            List<AdminMembershipVM> list = new List<AdminMembershipVM>();
            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                string query = @"SELECT u.FullName, u.Email, 
                         p.PlanName, um.StartDate, um.ExpiryDate, um.Id
                         FROM UserMemberships um
                         JOIN Users u ON um.UserId = u.UserId
                         JOIN MembershipPlans p ON um.PlanId = p.PlanId
                         WHERE 1=1";

                if (!string.IsNullOrEmpty(search))
                {
                    query += " AND (u.FullName LIKE @search OR u.Email LIKE @search)";
                }

                if (!string.IsNullOrEmpty(status))
                {
                    if (status == "Active")
                        query += " AND um.ExpiryDate > datetime('now')";
                    else if (status == "Expired")
                        query += " AND um.ExpiryDate <= datetime('now')";
                }

                SqliteCommand cmd = new SqliteCommand(query, con);

                if (!string.IsNullOrEmpty(search))
                    cmd.Parameters.AddWithValue("@search", "%" + search + "%");

                con.Open();
                SqliteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DateTime expiry = Convert.ToDateTime(reader["ExpiryDate"]);

                    list.Add(new AdminMembershipVM
                    {
                        FullName = reader["FullName"].ToString(),
                        Email = reader["Email"].ToString(),
                        PlanName = reader["PlanName"].ToString(),
                        StartDate = Convert.ToDateTime(reader["StartDate"]),
                        ExpiryDate = expiry,
                        Status = expiry > DateTime.Now ? "Active" : "Expired",
                        MembershipId = Convert.ToInt32(reader["Id"])
                    });
                }
            }

            return View(list);
        }

        public IActionResult RemoveMembership(int id)
        {
            if (HttpContext.Session.GetString("AdminLogin") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                string query = "DELETE FROM UserMemberships WHERE Id = @id";
                SqliteCommand cmd = new SqliteCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("MembershipList");
        }

        public IActionResult TrialBookings()
        {
            if (HttpContext.Session.GetString("AdminLogin") == null)
            {
                return RedirectToAction("Login");
            }

            List<TrialBooking> bookings = new List<TrialBooking>();

            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                string query = "SELECT * FROM TrialBookings ORDER BY CreatedAt DESC";
                SqliteCommand cmd = new SqliteCommand(query, con);

                con.Open();
                SqliteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {

                    bookings.Add(new TrialBooking
                    {
                        //Id = (int)reader["Id"],
                        Id = Convert.ToInt32(reader["Id"]),
                        FullName = reader["FullName"].ToString(),
                        PhoneNumber = reader["PhoneNumber"].ToString(),
                        PreferredDate = Convert.ToDateTime(reader["PreferredDate"]),
                        Message = reader["Message"].ToString(),
                        //CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                        CreatedAt = reader["CreatedAt"] != DBNull.Value
    ? Convert.ToDateTime(reader["CreatedAt"])
    : DateTime.Now,
                        Status = reader["Status"].ToString()
                    });
                }

            }

            return View(bookings);
        }

        public IActionResult ToggleStatus(int id)
        {
            if (HttpContext.Session.GetString("AdminLogin") == null)
            {
                return RedirectToAction("Login");
            }

            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                con.Open();

                string query = @"
        UPDATE TrialBookings
        SET Status = CASE 
            WHEN Status = 'Pending' THEN 'Contacted'
            ELSE 'Pending'
        END
        WHERE Id = @id";

                SqliteCommand cmd = new SqliteCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("TrialBookings");
        }

        public IActionResult TrainerAppointments(string search)
        {
            if (HttpContext.Session.GetString("AdminLogin") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            string conStr = _configuration.GetConnectionString("DefaultConnection");

            List<string[]> appointments = new List<string[]>();

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                string query = "SELECT * FROM TrainerAppointments";

                if (!string.IsNullOrEmpty(search))
                {
                    query += " WHERE FullName LIKE @search OR PhoneNumber LIKE @search OR TrainerName LIKE @search";
                }

                query += " ORDER BY CreatedAt DESC";

                SqliteCommand cmd = new SqliteCommand(query, con);

                if (!string.IsNullOrEmpty(search))
                {
                    cmd.Parameters.AddWithValue("@search", "%" + search + "%");
                }

                con.Open();
                SqliteDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    appointments.Add(new string[]
                    {
                dr["Id"].ToString(),
                dr["FullName"].ToString(),
                dr["PhoneNumber"].ToString(),
                dr["TrainerName"].ToString(),
                dr["AppointmentDate"].ToString(),
                dr["AppointmentTime"].ToString(),
                dr["Status"].ToString()
                    });
                }
            }

            ViewBag.Appointments = appointments;
            ViewBag.Search = search;

            return View();
        }

        public IActionResult UpdateAppointmentStatus(int id, string status)
        {
            if (HttpContext.Session.GetString("AdminLogin") == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                string query = "UPDATE TrainerAppointments SET Status=@status WHERE Id=@id";

                SqliteCommand cmd = new SqliteCommand(query, con);

                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("TrainerAppointments");
        }

        public IActionResult DeletePlan(int id)
        {
            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                con.Open();

                SqliteTransaction tran = con.BeginTransaction();

                try
                {
                    // Step 1: Delete from UserMemberships
                    string q1 = "DELETE FROM UserMemberships WHERE PlanId=@id";
                    SqliteCommand cmd1 = new SqliteCommand(q1, con, tran);
                    cmd1.Parameters.AddWithValue("@id", id);
                    cmd1.ExecuteNonQuery();

                    // Step 2: Delete from MembershipPlans
                    string q2 = "DELETE FROM MembershipPlans WHERE PlanId=@id";
                    SqliteCommand cmd2 = new SqliteCommand(q2, con, tran);
                    cmd2.Parameters.AddWithValue("@id", id);
                    cmd2.ExecuteNonQuery();

                    tran.Commit();

                    TempData["Success"] = "Plan deleted successfully!";
                }
                catch (Exception)
                {
                    tran.Rollback();
                    TempData["Error"] = "Delete failed!";
                }
            }

            return RedirectToAction("Plans");
        }

        public IActionResult EditPlan(int id)
        {
            string conStr = _configuration.GetConnectionString("DefaultConnection");

            MembershipPlan plan = new MembershipPlan();

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                string query = "SELECT * FROM MembershipPlans WHERE PlanId=@id";

                SqliteCommand cmd = new SqliteCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                SqliteDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    plan.PlanId = Convert.ToInt32(dr["PlanId"]);
                    plan.Price = Convert.ToDecimal(dr["Price"]);
                    //plan.PlanId = (int)dr["PlanId"];
                    plan.PlanName = dr["PlanName"].ToString();
                    //plan.Price = (decimal)dr["Price"];
                    plan.Duration = dr["Duration"].ToString();
                    plan.Features = dr["Features"].ToString();
                }
            }

            return View(plan);
        }

        [HttpPost]
        public IActionResult EditPlan(MembershipPlan plan)
        {
            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                string query = @"UPDATE MembershipPlans 
                         SET PlanName=@n, Price=@p, Duration=@d, Features=@f
                         WHERE PlanId=@id";

                SqliteCommand cmd = new SqliteCommand(query, con);

                cmd.Parameters.AddWithValue("@n", plan.PlanName);
                cmd.Parameters.AddWithValue("@p", plan.Price);
                cmd.Parameters.AddWithValue("@d", plan.Duration);
                cmd.Parameters.AddWithValue("@f", plan.Features);
                cmd.Parameters.AddWithValue("@id", plan.PlanId);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Plans");
        }
        public IActionResult AddPlan()
        {
            return View();
        }
        public IActionResult ContactMessages(string search, string status, int page = 1)
        {
            int pageSize = 5;
            List<Contact> contacts = new List<Contact>();

            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                string query = "SELECT * FROM Contacts WHERE 1=1";

                if (!string.IsNullOrEmpty(search))
                    query += " AND (Name LIKE @search OR Email LIKE @search)";

                if (!string.IsNullOrEmpty(status))
                    query += " AND Status = @status";

                query += " ORDER BY CreatedAt DESC";

                SqliteCommand cmd = new SqliteCommand(query, con);

                if (!string.IsNullOrEmpty(search))
                    cmd.Parameters.AddWithValue("@search", "%" + search + "%");

                if (!string.IsNullOrEmpty(status))
                    cmd.Parameters.AddWithValue("@status", status);

                con.Open();
                SqliteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    contacts.Add(new Contact
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        //Id = (int)reader["Id"],
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                      
                        Comment = reader["Comment"].ToString(),
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                        Status = reader["Status"].ToString()
                    });
                }
            }

            // 🔥 PAGINATION LOGIC
            int totalItems = contacts.Count();

            var pagedData = contacts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(pagedData);
        }


        public IActionResult MarkAsSeen(int id)
        {
            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                string query = "UPDATE Contacts SET Status='Seen' WHERE Id=@id";

                SqliteCommand cmd = new SqliteCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("ContactMessages");
        }

        public IActionResult DeleteMessage(int id)
        {
            string conStr = _configuration.GetConnectionString("DefaultConnection");

            using (SqliteConnection con = new SqliteConnection(conStr))
            {
                string query = "DELETE FROM Contacts WHERE Id=@id";

                SqliteCommand cmd = new SqliteCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("ContactMessages");
        }
    }
}
