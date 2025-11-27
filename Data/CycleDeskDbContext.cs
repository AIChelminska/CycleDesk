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

            // Konfiguracja jest w atrybutach [Table] i [Column] w modelach
            // Dodatkowa konfiguracja jeśli potrzebna:

            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<Product>()
                .HasKey(p => p.ProductId);

            modelBuilder.Entity<Category>()
                .HasKey(c => c.CategoryId);

            modelBuilder.Entity<Supplier>()
                .HasKey(s => s.SupplierId);

            modelBuilder.Entity<Customer>()
                .HasKey(c => c.CustomerId);

            modelBuilder.Entity<Sale>()
                .HasKey(s => s.SaleId);

            modelBuilder.Entity<SaleItem>()
                .HasKey(si => si.SaleItemId);

            modelBuilder.Entity<Invoice>()
                .HasKey(i => i.InvoiceId);

            modelBuilder.Entity<GoodsReceipt>()
                .HasKey(gr => gr.GoodsReceiptId);

            modelBuilder.Entity<GoodsReceiptItem>()
                .HasKey(gri => gri.GoodsReceiptItemId);

            modelBuilder.Entity<Inventory>()
                .HasKey(i => i.InventoryId);

            modelBuilder.Entity<StockHistory>()
                .HasKey(sh => sh.AdjustmentId);

            modelBuilder.Entity<AccessCode>()
                .HasKey(ac => ac.AccessCodeId);

            modelBuilder.Entity<ApplicationSettings>()
                .HasKey(a => a.SettingId);

            modelBuilder.Entity<AuditLog>()
                .HasKey(a => a.AuditId);
        }
    }
}