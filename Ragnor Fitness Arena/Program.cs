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

app.Run();