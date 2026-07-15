using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models;
using HMS.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "HMSAuth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

    await context.Database.MigrateAsync();
    await SeedDataAsync(context, userService);
}

app.Run();

static async Task SeedDataAsync(ApplicationDbContext context, IUserService userService)
{
    if (!await context.Users.AnyAsync())
    {
        var users = new[]
        {
            new User { FullName = "System Admin", Email = "admin@hms.local", Role = "Admin" },
            new User { FullName = "Restaurant Waiter", Email = "waiter@hms.local", Role = "Waiter" },
            new User { FullName = "Kitchen Staff", Email = "kitchen@hms.local", Role = "Kitchen" },
            new User { FullName = "Restaurant Cashier", Email = "cashier@hms.local", Role = "Cashier" },
            new User { FullName = "Shop Cashier", Email = "shop@hms.local", Role = "ShopCashier" }
        };

        foreach (var user in users)
            await userService.RegisterUserAsync(user, "Password123!");
    }

    if (!await context.RestaurantMenus.AnyAsync())
    {
        context.RestaurantMenus.AddRange(
            new HMS.Models.Restaurant.RestaurantMenu { Name = "Chapati", Category = RestaurantMenuCategory.Breakfast, Price = 50, Description = "Fresh chapati" },
            new HMS.Models.Restaurant.RestaurantMenu { Name = "Beef Stew", Category = RestaurantMenuCategory.Lunch, Price = 350, Description = "Served with rice" },
            new HMS.Models.Restaurant.RestaurantMenu { Name = "Grilled Chicken", Category = RestaurantMenuCategory.Dinner, Price = 450, Description = "Half chicken" },
            new HMS.Models.Restaurant.RestaurantMenu { Name = "Soda", Category = RestaurantMenuCategory.Drinks, Price = 80, Description = "500ml" }
        );
    }

    if (!await context.RestaurantTables.AnyAsync())
    {
        context.RestaurantTables.AddRange(
            new HMS.Models.Restaurant.RestaurantTable { TableNumber = "T1", Capacity = 2, Status = TableStatus.Available },
            new HMS.Models.Restaurant.RestaurantTable { TableNumber = "T2", Capacity = 4, Status = TableStatus.Available },
            new HMS.Models.Restaurant.RestaurantTable { TableNumber = "T3", Capacity = 6, Status = TableStatus.Available }
        );
    }

    if (!await context.ShopProducts.AnyAsync())
    {
        context.ShopProducts.AddRange(
            new HMS.Models.Shop.ShopProduct { Name = "Soap", Category = "Toiletries", Barcode = "100001", Price = 120, StockQuantity = 50 },
            new HMS.Models.Shop.ShopProduct { Name = "Bottled Water", Category = "Drinks", Barcode = "100002", Price = 60, StockQuantity = 100 },
            new HMS.Models.Shop.ShopProduct { Name = "Snacks Pack", Category = "Food", Barcode = "100003", Price = 150, StockQuantity = 8 }
        );
    }

    await context.SaveChangesAsync();
}
