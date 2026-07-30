using Microsoft.EntityFrameworkCore;
using HMS.Models;
using HMS.Models.Restaurant;
using HMS.Models.Shop;

namespace HMS.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<RestaurantMenu> RestaurantMenus { get; set; }
    public DbSet<RestaurantTable> RestaurantTables { get; set; }
    public DbSet<RestaurantOrder> RestaurantOrders { get; set; }
    public DbSet<RestaurantOrderItem> RestaurantOrderItems { get; set; }
    public DbSet<ShopProduct> ShopProducts { get; set; }
    public DbSet<ShopSale> ShopSales { get; set; }
    public DbSet<ShopSaleItem> ShopSaleItems { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StockMovement>(e =>
        {
            e.HasOne(m => m.Product)
                .WithMany()
                .HasForeignKey(m => m.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(m => m.PerformedByUser)
                .WithMany()
                .HasForeignKey(m => m.PerformedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RestaurantMenu>(e =>
        {
            e.Property(m => m.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<RestaurantOrder>(e =>
        {
            e.Property(o => o.TotalAmount).HasPrecision(18, 2);
            e.Property(o => o.AmountPaid).HasPrecision(18, 2);
            e.Property(o => o.ChangeAmount).HasPrecision(18, 2);

            e.HasOne(o => o.Table)
                .WithMany(t => t.Orders)
                .HasForeignKey(o => o.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(o => o.Waiter)
                .WithMany(u => u.RestaurantOrders)
                .HasForeignKey(o => o.WaiterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RestaurantOrderItem>(e =>
        {
            e.Property(i => i.UnitPrice).HasPrecision(18, 2);
            e.Ignore(i => i.LineTotal);

            e.HasOne(i => i.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(i => i.MenuItem)
                .WithMany(m => m.OrderItems)
                .HasForeignKey(i => i.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RestaurantTable>(e =>
        {
            e.HasIndex(t => t.TableNumber).IsUnique();
        });

        modelBuilder.Entity<ShopProduct>(e =>
        {
            e.Property(p => p.Price).HasPrecision(18, 2);
            e.HasIndex(p => p.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL");
        });

        modelBuilder.Entity<ShopSale>(e =>
        {
            e.Property(s => s.SubTotal).HasPrecision(18, 2);
            e.Property(s => s.Discount).HasPrecision(18, 2);
            e.Property(s => s.Tax).HasPrecision(18, 2);
            e.Property(s => s.TotalAmount).HasPrecision(18, 2);
            e.Property(s => s.AmountPaid).HasPrecision(18, 2);
            e.Property(s => s.ChangeAmount).HasPrecision(18, 2);

            e.HasOne(s => s.Cashier)
                .WithMany()
                .HasForeignKey(s => s.CashierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ShopSaleItem>(e =>
        {
            e.Property(i => i.UnitPrice).HasPrecision(18, 2);
            e.Ignore(i => i.LineTotal);

            e.HasOne(i => i.Sale)
                .WithMany(s => s.Items)
                .HasForeignKey(i => i.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(i => i.Product)
                .WithMany(p => p.SaleItems)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
        });
    }
}
