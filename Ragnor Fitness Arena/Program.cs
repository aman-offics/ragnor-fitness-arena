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



using (var scope = app.Services.CreateScope())
{
    var conStr = builder.Configuration.GetConnectionString("DefaultConnection");

    using var con = new Microsoft.Data.Sqlite.SqliteConnection(conStr);

    con.Open();

    string sql = @"

    CREATE TABLE IF NOT EXISTS MembershipPlans (
        PlanId INTEGER PRIMARY KEY AUTOINCREMENT,
        PlanName TEXT,
        Price REAL,
        Duration INTEGER,
        Features TEXT
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

    ";

    var cmd = new Microsoft.Data.Sqlite.SqliteCommand(sql, con);

    cmd.ExecuteNonQuery();
}

app.Run();