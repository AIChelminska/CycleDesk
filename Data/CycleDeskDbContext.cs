using CycleDesk.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleDesk.Data
{
    public class CycleDeskDbContext : DbContext
    {
        // ===== DbSets =====
        public DbSet<User> Users { get; set; }
        public DbSet<AccessCode> AccessCodes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Inventory> Inventory { get; set; }
        public DbSet<StockHistory> StockHistory { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<GoodsReceipt> GoodsReceipts { get; set; }
        public DbSet<GoodsReceiptItem> GoodsReceiptItems { get; set; }
        public DbSet<ApplicationSettings> ApplicationSettings { get; set; }
        public DbSet<AuditLog> AuditLog { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Server=localhost\\SQLEXPRESS;" +
                "Database=CycleDesk;" +
                "Trusted_Connection=True;" +
                "TrustServerCertificate=True;"
            );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductId).HasColumnName("ProductID");
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.ToTable("Suppliers");
                entity.HasKey(e => e.SupplierId);
                entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId).HasColumnName("UserID");
            });

            modelBuilder.Entity<GoodsReceipt>(entity =>
            {
                entity.ToTable("GoodsReceipts");
                entity.HasKey(e => e.GoodsReceiptId);
                entity.Property(e => e.GoodsReceiptId).HasColumnName("GoodsReceiptID");
                entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
                entity.Property(e => e.ReceivedByUserId).HasColumnName("ReceivedByUserID");
            });

            modelBuilder.Entity<GoodsReceiptItem>(entity =>
            {
                entity.ToTable("GoodsReceiptItems");
                entity.HasKey(e => e.GoodsReceiptItemId);
                entity.Property(e => e.GoodsReceiptItemId).HasColumnName("GoodsReceiptItemID");
                entity.Property(e => e.GoodsReceiptId).HasColumnName("GoodsReceiptID");
                entity.Property(e => e.ProductId).HasColumnName("ProductID");
            });

            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.ToTable("Inventory");
                entity.HasKey(e => e.InventoryId);
                entity.Property(e => e.InventoryId).HasColumnName("InventoryID");
                entity.Property(e => e.ProductId).HasColumnName("ProductID");
            });
        }
    }
}