using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BarSheetAPI.Models
{
  public class InventoryDbContext : DbContext
  {
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Shop> Shops { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductSize> ProductSizes { get; set; }
    public DbSet<DailyReport> DailyReports { get; set; }
    public DbSet<SaleProduct> SaleProducts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      // Unique constraint for User email
      modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

      modelBuilder.Entity<Product>()
          .HasIndex(a => new { a.ShopId, a.Name, a.CategoryId }).IsUnique();

      modelBuilder.Entity<ProductSize>()
          .HasIndex(a => new { a.ShopId, a.Name }).IsUnique();

      modelBuilder.Entity<Category>()
          .HasIndex(a => new { a.ShopId, a.Name }).IsUnique();

      modelBuilder.Entity<User>()
          .HasOne(u => u.Shop)
          .WithMany(s => s.Users)
          .HasForeignKey(u => u.ShopId)
          .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<Product>()
          .HasOne(p => p.Shop)
          .WithMany(s => s.Products)
          .HasForeignKey(ps => ps.ShopId)
          .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<ProductSize>()
          .HasOne(ps => ps.Shop)
          .WithMany(s => s.ProductSizes)
          .HasForeignKey(ps => ps.ShopId)
          .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<Category>()
          .HasOne(c => c.Shop)
          .WithMany(s => s.Categories)
          .HasForeignKey(c => c.ShopId)
          .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<DailyReport>()
          .HasOne(dr => dr.Shop)
          .WithMany(s => s.DailySalesReports)
          .HasForeignKey(dr => dr.ShopId)
          .OnDelete(DeleteBehavior.Restrict);

      modelBuilder.Entity<SaleProduct>()
          .HasOne(sp => sp.Shop)
          .WithMany(s => s.SalesProducts)
          .HasForeignKey(sp => sp.ShopId)
          .OnDelete(DeleteBehavior.Restrict);

      // ---- New mappings for DailyReport ----
      modelBuilder.Entity<DailyReport>(entity =>
      {
        // JSON snapshots
        entity.Property(dr => dr.OBJson).HasColumnType("nvarchar(max)");
        entity.Property(dr => dr.ReceiptsJson).HasColumnType("nvarchar(max)");
        entity.Property(dr => dr.SalesJson).HasColumnType("nvarchar(max)");
        entity.Property(dr => dr.CBJson).HasColumnType("nvarchar(max)");
        entity.Property(dr => dr.BreaksJson).HasColumnType("nvarchar(max)");

        // Amount aggregates
        entity.Property(dr => dr.TotalReceiptsAmount).HasColumnType("decimal(18,2)");
        entity.Property(dr => dr.TotalSalesAmount).HasColumnType("decimal(18,2)");
        entity.Property(dr => dr.TotalBreaksAmount).HasColumnType("decimal(18,2)");
        entity.Property(dr => dr.OverallTotalAmount).HasColumnType("decimal(18,2)");
      });
    }
  }
}
