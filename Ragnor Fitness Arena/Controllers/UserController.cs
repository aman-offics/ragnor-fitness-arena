using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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

        using (SqlConnection con = new SqlConnection(conStr))
        {
            string query = "SELECT UserId, FullName FROM Users WHERE Email=@e AND Password=@p";
            SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@e", Email);
            cmd.Parameters.AddWithValue("@p", Password);

            con.Open();

            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                int userIdFromDB = (int)reader["UserId"];
                string fullName = reader["FullName"] == DBNull.Value ? "" : reader["FullName"].ToString();


                // 🔥 STORE SESSION HERE
                HttpContext.Session.SetInt32("UserId", userIdFromDB);
                HttpContext.Session.SetString("UserName", fullName);

                return RedirectToAction("Index", "Home");
            }
        }

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

        using (SqlConnection con = new SqlConnection(conStr))
        {
            string query = "INSERT INTO Users (FullName, Email, Password) VALUES (@n, @e, @p)";

            SqlCommand cmd = new SqlCommand(query, con);

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

      
        if (userId == null)
        {
        
            TempData["Message"] = "Please login first!";
            return RedirectToAction("Index", "Home");


        }

        string conStr = _configuration.GetConnectionString("DefaultConnection");

        using (SqlConnection con = new SqlConnection(conStr))
        {
            con.Open();

            // 🔥 CHECK IF USER ALREADY HAS THIS PLAN
            // 🔥 CHECK IF USER ALREADY HAS ANY ACTIVE PLAN
            SqlCommand checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM UserMemberships WHERE UserId = @UserId AND ExpiryDate > GETDATE()",
                con);

            checkCmd.Parameters.AddWithValue("@UserId", userId.Value);

            int activePlan = (int)checkCmd.ExecuteScalar();

            if (activePlan > 0)
            {
                TempData["Message"] = "You already have an active membership!";
                return RedirectToAction("MyMembership");
            }

            // 🔥 GET PLAN DURATION
            SqlCommand cmdPlan = new SqlCommand(
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
            SqlCommand cmdInsert = new SqlCommand(
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
        return RedirectToAction("MyMembership","User");
    }




    [HttpPost]
    public IActionResult CancelMembership()
    {
        int? userId =HttpContext.Session.GetInt32("UserId");

        if (userId == null)
            return RedirectToAction("Index", "Home");

        string conStr = _configuration.GetConnectionString("DefaultConnection");
        using (SqlConnection con = new SqlConnection(conStr))
        {
            string deleteQuery = "DELETE FROM UserMemberships WHERE UserId=@uid";
            SqlCommand cmd = new SqlCommand(deleteQuery, con);
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

        using (SqlConnection con = new SqlConnection(conStr))
        {
            string query = @"SELECT m.PlanName,
                                m.Price,
                                m.Duration,
                                um.StartDate,
                                um.ExpiryDate
                         FROM UserMemberships um
                         JOIN MembershipPlans m ON um.PlanId = m.PlanId
                         WHERE um.UserId = @uid";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@uid", userId.Value);

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

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
