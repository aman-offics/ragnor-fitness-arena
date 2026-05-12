using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// 🔥 Add services
builder.Services.AddControllersWithViews();

// ✅ Session (IMPORTANT)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Antiforgery (optional but ok)
builder.Services.AddAntiforgery(options =>
{
    options.SuppressXFrameOptionsHeader = true;
});

var app = builder.Build();


// 🔥 ERROR HANDLING (ONLY ONCE)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


// 🔥 MIDDLEWARE ORDER IMPORTANT
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();     // ✅ MUST BEFORE Authorization
app.UseAuthorization();


// ❌ REMOVE DUPLICATE ERROR HANDLERS
// (you had 3 — removed now)


// 🔥 ROUTING
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



using Microsoft.Data.Sqlite;

using (var scope = app.Services.CreateScope())
{
    var conStr = builder.Configuration.GetConnectionString("DefaultConnection");

    using var con = new SqliteConnection(conStr);

    con.Open();

    string sql = @"

    CREATE TABLE IF NOT EXISTS MembershipPlans (
        PlanId INTEGER PRIMARY KEY AUTOINCREMENT,
        PlanName TEXT,
        Price REAL,
        Duration INTEGER,
        Features TEXT,
        CreatedAt TEXT
    );

    CREATE TABLE IF NOT EXISTS Users (
        UserId INTEGER PRIMARY KEY AUTOINCREMENT,
        FullName TEXT,
        Email TEXT,
        Password TEXT,
        CreatedAt TEXT
    );

    CREATE TABLE IF NOT EXISTS Admins (
        AdminId INTEGER PRIMARY KEY AUTOINCREMENT,
        Username TEXT,
        Password TEXT
    );

    CREATE TABLE IF NOT EXISTS Contacts (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Name TEXT,
        Email TEXT,
        Comment TEXT,
        CreatedAt TEXT,
        Status TEXT
    );

    CREATE TABLE IF NOT EXISTS TrialBookings (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        FullName TEXT,
        PhoneNumber TEXT,
        PreferredDate TEXT,
        Message TEXT,
        CreatedAt TEXT,
        Status TEXT
    );

    CREATE TABLE IF NOT EXISTS TrainerAppointments (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        FullName TEXT,
        PhoneNumber TEXT,
        TrainerName TEXT,
        AppointmentDate TEXT,
        AppointmentTime TEXT,
        Message TEXT,
        Status TEXT,
        CreatedAt TEXT
    );

    CREATE TABLE IF NOT EXISTS UserMemberships (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        UserId INTEGER,
        PlanId INTEGER,
        StartDate TEXT,
        ExpiryDate TEXT
    );

    ";

    var cmd = new SqliteCommand(sql, con);

    cmd.ExecuteNonQuery();

    // Default Admin
    string adminCheck = "SELECT COUNT(*) FROM Admins";

    var checkCmd = new SqliteCommand(adminCheck, con);

    int adminCount = Convert.ToInt32(checkCmd.ExecuteScalar());

    if (adminCount == 0)
    {
        string insertAdmin = @"
        INSERT INTO Admins (Username, Password)
        VALUES ('admin', 'admin123')
        ";

        var insertCmd = new SqliteCommand(insertAdmin, con);

        insertCmd.ExecuteNonQuery();
    }
}

app.Run();