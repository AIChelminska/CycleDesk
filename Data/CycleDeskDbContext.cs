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
        public DbSet<InventoryAdjustment> InventoryAdjustments { get; set; }
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

            modelBuilder.Entity<InventoryAdjustment>(entity =>
            {
                entity.ToTable("InventoryAdjustments");
                entity.HasKey(e => e.AdjustmentId);
                entity.Property(e => e.AdjustmentId).HasColumnName("AdjustmentID");
                entity.Property(e => e.ProductId).HasColumnName("ProductID");
                entity.Property(e => e.AdjustedByUserId).HasColumnName("AdjustedByUserID");
            });

            modelBuilder.Entity<Sale>(entity =>
            {
                entity.ToTable("Sales");
                entity.HasKey(e => e.SaleId);
                entity.Property(e => e.SaleId).HasColumnName("SaleID");
                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
                entity.Property(e => e.SoldByUserId).HasColumnName("SoldByUserID");
            });

            modelBuilder.Entity<SaleItem>(entity =>
            {
                entity.ToTable("SaleItems");
                entity.HasKey(e => e.SaleItemId);
                entity.Property(e => e.SaleItemId).HasColumnName("SaleItemID");
                entity.Property(e => e.SaleId).HasColumnName("SaleID");
                entity.Property(e => e.ProductId).HasColumnName("ProductID");
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.ToTable("Invoices");
                entity.HasKey(e => e.InvoiceId);
                entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");
                entity.Property(e => e.SaleId).HasColumnName("SaleID");
                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
                entity.Property(e => e.IssuedByUserId).HasColumnName("IssuedByUserID");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers");
                entity.HasKey(e => e.CustomerId);
                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            });
        }
    }
}
