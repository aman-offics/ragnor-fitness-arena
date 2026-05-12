using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Ragnor_Fitness_Arena.Models;


public class UserController : Controller
{

    private readonly IConfiguration _configuration;

    public UserController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string Email, string Password)
    {
        string conStr = _configuration.GetConnectionString("DefaultConnection");
        //return Content(conStr);

        using (SqliteConnection con = new SqliteConnection(conStr))
        {
            string query = "SELECT UserId, FullName FROM Users WHERE Email=@e AND Password=@p";

            SqliteCommand cmd = new SqliteCommand(query, con);

            cmd.Parameters.AddWithValue("@e", Email);
            cmd.Parameters.AddWithValue("@p", Password);

            con.Open();

            SqliteDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                //int userIdFromDB = (int)reader["UserId"];
                int userIdFromDB = Convert.ToInt32(reader["UserId"]);

                string fullName = reader["FullName"] == DBNull.Value
                                    ? ""
                                    : reader["FullName"].ToString();

                // ✅ CLOSE READER FIRST
                reader.Close();

                // ✅ STORE SESSION
                HttpContext.Session.SetInt32("UserId", userIdFromDB);
                HttpContext.Session.SetString("UserName", fullName);

                // ✅ GET ACTIVE MEMBERSHIP COUNT
                string countQuery = @"SELECT COUNT(*) 
                                  FROM UserMemberships 
                                  WHERE UserId = @UserId 
                                  AND ExpiryDate > datetime('now')";

                SqliteCommand countCmd = new SqliteCommand(countQuery, con);

                countCmd.Parameters.AddWithValue("@UserId", userIdFromDB);

                //int membershipCount = (int)countCmd.ExecuteScalar();
                int membershipCount = Convert.ToInt32(countCmd.ExecuteScalar());

                // ✅ STORE MEMBERSHIP COUNT SESSION
                HttpContext.Session.SetInt32("MembershipCount", membershipCount);

                //TempData["Success"] = "Login Successful!";

                return RedirectToAction("Index", "Home");
            }

            reader.Close();
        }

     
        TempData["Error"] = "Invalid Email or Password!";
        return RedirectToAction("Index", "Home");

    }



    public IActionResult Signup()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Signup(string FullName, string Email, string Password)
    {
        string conStr = _configuration.GetConnectionString("DefaultConnection");

        using (SqliteConnection con = new SqliteConnection(conStr))
        {
            string query = "INSERT INTO Users (FullName, Email, Password) VALUES (@n, @e, @p)";

            SqliteCommand cmd = new SqliteCommand(query, con);

            cmd.Parameters.AddWithValue("@n", FullName);
            cmd.Parameters.AddWithValue("@e", Email);
            cmd.Parameters.AddWithValue("@p", Password);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        return RedirectToAction("Index", "Home");
    }


    public IActionResult Dashboard()
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
            return RedirectToAction("Login");

        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }


    [HttpPost]
    public IActionResult JoinPlan(int id)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");

        // 🔥 CHECK LOGIN
        if (userId == null)
        {
            TempData["Message"] = "Please login first!";
            return RedirectToAction("Index", "Home");
        }

        string conStr = _configuration.GetConnectionString("DefaultConnection");

        using (SqliteConnection con = new SqliteConnection(conStr))
        {
            con.Open();

            // 🔥 CHECK ACTIVE PLAN COUNT
            SqliteCommand checkCmd = new SqliteCommand(
                @"SELECT COUNT(*) 
              FROM UserMemberships 
              WHERE UserId = @UserId 
              AND ExpiryDate > datetime('now')",
                con);

            checkCmd.Parameters.AddWithValue("@UserId", userId.Value);

            //int activePlans = (int)checkCmd.ExecuteScalar();
            int activePlans = Convert.ToInt32(checkCmd.ExecuteScalar());

            // 🔥 MAX 3 ACTIVE PLANS
            if (activePlans >= 3)
            {
                TempData["Message"] = "You can only have 3 active plans at the same time!";
                return RedirectToAction("MyMembership", "User");
            }

            // 🔥 CHECK SAME PLAN ALREADY ACTIVE
            SqliteCommand duplicateCmd = new SqliteCommand(
                @"SELECT COUNT(*) 
              FROM UserMemberships 
              WHERE UserId = @UserId 
              AND PlanId = @PlanId
              AND ExpiryDate > datetime('now')",
                con);

            duplicateCmd.Parameters.AddWithValue("@UserId", userId.Value);
            duplicateCmd.Parameters.AddWithValue("@PlanId", id);

            //int samePlan = (int)duplicateCmd.ExecuteScalar();
            int samePlan = Convert.ToInt32(duplicateCmd.ExecuteScalar());

            if (samePlan > 0)
            {
                TempData["Message"] = "You already joined this plan!";
                return RedirectToAction("MyMembership", "User");
            }

            // 🔥 GET PLAN DURATION
            SqliteCommand cmdPlan = new SqliteCommand(
                "SELECT Duration FROM MembershipPlans WHERE PlanId = @PlanId",
                con);

            cmdPlan.Parameters.AddWithValue("@PlanId", id);

            object result = cmdPlan.ExecuteScalar();

            if (result == null)
            {
                TempData["Message"] = "Invalid Plan!";
                return RedirectToAction("Index", "Home");
            }

            int durationMonths = Convert.ToInt32(result);

            DateTime startDate = DateTime.Now;
            DateTime expiryDate = startDate.AddMonths(durationMonths);

            // 🔥 INSERT MEMBERSHIP
            SqliteCommand cmdInsert = new SqliteCommand(
                @"INSERT INTO UserMemberships 
              (UserId, PlanId, StartDate, ExpiryDate) 
              VALUES 
              (@UserId, @PlanId, @StartDate, @ExpiryDate)",
                con);

            cmdInsert.Parameters.AddWithValue("@UserId", userId.Value);
            cmdInsert.Parameters.AddWithValue("@PlanId", id);
            cmdInsert.Parameters.AddWithValue("@StartDate", startDate);
            cmdInsert.Parameters.AddWithValue("@ExpiryDate", expiryDate);

            cmdInsert.ExecuteNonQuery();
        }

        TempData["Message"] = "Plan Joined Successfully!";
        return RedirectToAction("MyMembership", "User");
    }




    [HttpPost]
    public IActionResult CancelMembership()
    {
        int? userId =HttpContext.Session.GetInt32("UserId");

        if (userId == null)
            return RedirectToAction("Index", "Home");

        string conStr = _configuration.GetConnectionString("DefaultConnection");
        using (SqliteConnection con = new SqliteConnection(conStr))
        {
            string deleteQuery = "DELETE FROM UserMemberships WHERE UserId=@uid";
            SqliteCommand cmd = new SqliteCommand(deleteQuery, con);
            cmd.Parameters.AddWithValue("@uid", userId);

            con.Open();
            cmd.ExecuteNonQuery();
        }
        TempData["Message"] = "Membership Cancelled Successfully!";
        return RedirectToAction("MyMembership");


    }




    public IActionResult MyMembership()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
            return RedirectToAction("Index", "Home");

        List<UserMembershipViewModel> memberships = new List<UserMembershipViewModel>();

        string conStr = _configuration.GetConnectionString("DefaultConnection");

        using (SqliteConnection con = new SqliteConnection(conStr))
        {
            string query = @"SELECT m.PlanName,
                                m.Price,
                                m.Duration,
                                um.StartDate,
                                um.ExpiryDate
                         FROM UserMemberships um
                         JOIN MembershipPlans m ON um.PlanId = m.PlanId
                         WHERE um.UserId = @uid";

            SqliteCommand cmd = new SqliteCommand(query, con);
            cmd.Parameters.AddWithValue("@uid", userId.Value);

            con.Open();
            SqliteDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                memberships.Add(new UserMembershipViewModel
                {
                    PlanName = reader["PlanName"].ToString(),
                    Price = Convert.ToDecimal(reader["Price"]),
                    Duration = reader["Duration"].ToString(),
                    StartDate = Convert.ToDateTime(reader["StartDate"]),
                    ExpiryDate = Convert.ToDateTime(reader["ExpiryDate"])
                });
            }
        }

        return View("MyMembership", memberships);
    }



}
