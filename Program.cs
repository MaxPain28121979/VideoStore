using Microsoft.AspNetCore.Authentication.Cookies;
using VideoStore.Services;
using Microsoft.EntityFrameworkCore;
using VideoStore.Data;
using VideoStore.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=videostore.db"));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
    });

builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Ensure database is created and migrate old schema (Username -> Email) if needed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Detect legacy column and migrate safely
    var connection = db.Database.GetDbConnection();
    connection.Open();
    try
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA table_info('Users');";
        using var reader = cmd.ExecuteReader();
        bool hasUsername = false, hasEmail = false;
        while (reader.Read())
        {
            var col = reader.GetString(1);
            if (string.Equals(col, "Username", StringComparison.OrdinalIgnoreCase)) hasUsername = true;
            if (string.Equals(col, "Email", StringComparison.OrdinalIgnoreCase)) hasEmail = true;
        }
        reader.Close();

        if (hasUsername && !hasEmail)
        {
            // Try simple rename first; if it fails, recreate the table and copy data
            try
            {
                using var tran = connection.BeginTransaction();
                cmd.Transaction = tran;
                cmd.CommandText = "ALTER TABLE Users RENAME COLUMN Username TO Email;";
                cmd.ExecuteNonQuery();
                tran.Commit();
            }
            catch
            {
                using var tran2 = connection.BeginTransaction();
                cmd.Transaction = tran2;
                cmd.CommandText = @"CREATE TABLE Users_new (
                    Id INTEGER PRIMARY KEY,
                    Email TEXT NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    Role TEXT NOT NULL
                );";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO Users_new (Id, Email, PasswordHash, Role) SELECT Id, Username, PasswordHash, Role FROM Users;";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "DROP TABLE Users;";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "ALTER TABLE Users_new RENAME TO Users;";
                cmd.ExecuteNonQuery();

                tran2.Commit();
            }
        }
    }
    finally
    {
        connection.Close();
    }

    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    if (!db.Users.Any())
    {
        var admin = new User { Email = "admin@videostore.local", Role = "Admin" };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");
        db.Users.Add(admin);
        db.Videos.Add(new Video { Title = "The Matrix", Genre = "Sci-Fi", Year = 1999, Price = 3.99M });
        db.SaveChanges();
    }
}

app.Run();
