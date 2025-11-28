using CycleDesk.Data;
using CycleDesk.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleDesk.Services
{
    /// <summary>
    /// DTO dla wyświetlania produktów z pełnymi danymi (JOIN z Inventory, Category, Supplier)
    /// </summary>
    public class ProductDisplayDto
    {
        public int ProductId { get; set; }
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public string SupplierName { get; set; }
        public int? SupplierId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal VAT { get; set; }
        public int QuantityInStock { get; set; }
        public int MinimumStock { get; set; }
        public int ReorderLevel { get; set; }
        public string StockStatus { get; set; }
        public string StockStatusColor { get; set; }
        public bool IsActive { get; set; }
        public string ImagePath { get; set; }
        public string Barcode { get; set; }
        public string Unit { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Serwis do obsługi produktów używający Entity Framework Core
    /// </summary>
    public class ProductService
    {
        // ===== CREATE =====
        public int AddProduct(Product product, int initialQuantity = 0, string locationInWarehouse = null)
        {
            using (var context = new CycleDeskDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        // Dodaj produkt
                        product.CreatedDate = DateTime.Now;
                        product.IsActive = true;
                        context.Products.Add(product);
                        context.SaveChanges();

                        // Dodaj rekord Inventory
                        var inventory = new Inventory
                        {
                            ProductId = product.ProductId,
                            QuantityInStock = initialQuantity,
                            LastStockUpdate = DateTime.Now,
                            LocationInWarehouse = locationInWarehouse
                        };
                        context.Inventory.Add(inventory);
                        context.SaveChanges();

                        transaction.Commit();
                        return product.ProductId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // ===== READ =====
        public List<ProductDisplayDto> GetAllProductsWithDetails()
        {
            using (var context = new CycleDeskDbContext())
            {
                var products = (from p in context.Products
                                join c in context.Categories on p.CategoryId equals c.CategoryId into catGroup
                                from c in catGroup.DefaultIfEmpty()
                                join s in context.Suppliers on p.SupplierId equals s.SupplierId into supGroup
                                from s in supGroup.DefaultIfEmpty()
                                join i in context.Inventory on p.ProductId equals i.ProductId into invGroup
                                from i in invGroup.DefaultIfEmpty()
                                where p.IsActive
                                orderby p.ProductName
                                select new ProductDisplayDto
                                {
                                    ProductId = p.ProductId,
                                    SKU = p.SKU,
                                    ProductName = p.ProductName,
                                    Description = p.Description,
                                    CategoryName = c != null ? c.CategoryName : "Brak kategorii",
                                    CategoryId = p.CategoryId,
                                    SupplierName = s != null ? s.SupplierName : "Brak dostawcy",
                                    SupplierId = p.SupplierId,
                                    PurchasePrice = p.PurchasePrice,
                                    SellingPrice = p.SellingPrice,
                                    VAT = p.VAT,
                                    QuantityInStock = i != null ? i.QuantityInStock : 0,
                                    MinimumStock = p.MinimumStock,
                                    ReorderLevel = p.ReorderLevel,
                                    IsActive = p.IsActive,
                                    ImagePath = p.ImagePath,
                                    Barcode = p.Barcode,
                                    Unit = p.Unit,
                                    CreatedDate = p.CreatedDate
                                }).ToList();

                // Oblicz status magazynowy
                foreach (var product in products)
                {
                    if (product.QuantityInStock <= 0)
                    {
                        product.StockStatus = "Out of Stock";
                        product.StockStatusColor = "#FFDC3545"; // czerwony
                    }
                    else if (product.QuantityInStock <= product.MinimumStock)
                    {
                        product.StockStatus = "Critical";
                        product.StockStatusColor = "#FFDC3545"; // czerwony
                    }
                    else if (product.QuantityInStock <= product.ReorderLevel)
                    {
                        product.StockStatus = "Low Stock";
                        product.StockStatusColor = "#FFFFC107"; // żółty
                    }
                    else
                    {
                        product.StockStatus = "In Stock";
                        product.StockStatusColor = "#FF28A745"; // zielony
                    }
                }

                return products;
            }
        }

        public ProductDisplayDto GetProductById(int productId)
        {
            using (var context = new CycleDeskDbContext())
            {
                var product = (from p in context.Products
                               join c in context.Categories on p.CategoryId equals c.CategoryId into catGroup
                               from c in catGroup.DefaultIfEmpty()
                               join s in context.Suppliers on p.SupplierId equals s.SupplierId into supGroup
                               from s in supGroup.DefaultIfEmpty()
                               join i in context.Inventory on p.ProductId equals i.ProductId into invGroup
                               from i in invGroup.DefaultIfEmpty()
                               where p.ProductId == productId
                               select new ProductDisplayDto
                               {
                                   ProductId = p.ProductId,
                                   SKU = p.SKU,
                                   ProductName = p.ProductName,
                                   Description = p.Description,
                                   CategoryName = c != null ? c.CategoryName : "Brak kategorii",
                                   CategoryId = p.CategoryId,
                                   SupplierName = s != null ? s.SupplierName : "Brak dostawcy",
                                   SupplierId = p.SupplierId,
                                   PurchasePrice = p.PurchasePrice,
                                   SellingPrice = p.SellingPrice,
                                   VAT = p.VAT,
                                   QuantityInStock = i != null ? i.QuantityInStock : 0,
                                   MinimumStock = p.MinimumStock,
                                   ReorderLevel = p.ReorderLevel,
                                   IsActive = p.IsActive,
                                   ImagePath = p.ImagePath,
                                   Barcode = p.Barcode,
                                   Unit = p.Unit,
                                   CreatedDate = p.CreatedDate
                               }).FirstOrDefault();

                if (product != null)
                {
                    // Oblicz status
                    if (product.QuantityInStock <= 0)
                    {
                        product.StockStatus = "Out of Stock";
                        product.StockStatusColor = "#FFDC3545";
                    }
                    else if (product.QuantityInStock <= product.MinimumStock)
                    {
                        product.StockStatus = "Critical";
                        product.StockStatusColor = "#FFDC3545";
                    }
                    else if (product.QuantityInStock <= product.ReorderLevel)
                    {
                        product.StockStatus = "Low Stock";
                        product.StockStatusColor = "#FFFFC107";
                    }
                    else
                    {
                        product.StockStatus = "In Stock";
                        product.StockStatusColor = "#FF28A745";
                    }
                }

                return product;
            }
        }

        public Product GetProductEntityById(int productId)
        {
            using (var context = new CycleDeskDbContext())
            {
                return context.Products.FirstOrDefault(p => p.ProductId == productId);
            }
        }

        // ===== UPDATE =====
        public bool UpdateProduct(Product product)
        {
            using (var context = new CycleDeskDbContext())
            {
                var existingProduct = context.Products.FirstOrDefault(p => p.ProductId == product.ProductId);
                if (existingProduct == null) return false;

                existingProduct.SKU = product.SKU;
                existingProduct.ProductName = product.ProductName;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.SupplierId = product.SupplierId;
                existingProduct.PurchasePrice = product.PurchasePrice;
                existingProduct.SellingPrice = product.SellingPrice;
                existingProduct.VAT = product.VAT;
                existingProduct.Unit = product.Unit;
                existingProduct.Barcode = product.Barcode;
                existingProduct.MinimumStock = product.MinimumStock;
                existingProduct.ReorderLevel = product.ReorderLevel;
                existingProduct.ImagePath = product.ImagePath;
                existingProduct.IsActive = product.IsActive;
                existingProduct.ModifiedDate = DateTime.Now;

                context.SaveChanges();
                return true;
            }
        }

        public bool UpdateStock(int productId, int newQuantity)
        {
            using (var context = new CycleDeskDbContext())
            {
                var inventory = context.Inventory.FirstOrDefault(i => i.ProductId == productId);
                if (inventory == null) return false;

                inventory.QuantityInStock = newQuantity;
                inventory.LastStockUpdate = DateTime.Now;
                context.SaveChanges();
                return true;
            }
        }

        // ===== DELETE (Soft delete) =====
        public bool DeactivateProduct(int productId)
        {
            using (var context = new CycleDeskDbContext())
            {
                var product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                if (product == null) return false;

                product.IsActive = false;
                product.ModifiedDate = DateTime.Now;
                context.SaveChanges();
                return true;
            }
        }

        public bool DeleteProduct(int productId)
        {
            using (var context = new CycleDeskDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        // Najpierw usuń rekord Inventory
                        var inventory = context.Inventory.FirstOrDefault(i => i.ProductId == productId);
                        if (inventory != null)
                        {
                            context.Inventory.Remove(inventory);
                        }

                        // Potem usuń produkt
                        var product = context.Products.FirstOrDefault(p => p.ProductId == productId);
                        if (product == null) return false;

                        context.Products.Remove(product);
                        context.SaveChanges();

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // ===== SEARCH & FILTER =====
        public List<ProductDisplayDto> SearchProducts(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllProductsWithDetails();

            var allProducts = GetAllProductsWithDetails();
            var term = searchTerm.ToLower();

            return allProducts.Where(p =>
                p.SKU.ToLower().Contains(term) ||
                p.ProductName.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)) ||
                p.CategoryName.ToLower().Contains(term) ||
                p.SupplierName.ToLower().Contains(term)
            ).ToList();
        }

        public List<ProductDisplayDto> FilterProducts(
            string searchTerm = null,
            int? categoryId = null,
            int? supplierId = null,
            string stockStatus = null)
        {
            var products = GetAllProductsWithDetails();

            // Szukaj po tekście
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                products = products.Where(p =>
                    p.SKU.ToLower().Contains(term) ||
                    p.ProductName.ToLower().Contains(term) ||
                    (p.Description != null && p.Description.ToLower().Contains(term))
                ).ToList();
            }

            // Filtruj po kategorii
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value).ToList();
            }

            // Filtruj po dostawcy
            if (supplierId.HasValue && supplierId.Value > 0)
            {
                products = products.Where(p => p.SupplierId == supplierId.Value).ToList();
            }

            // Filtruj po statusie magazynowym
            if (!string.IsNullOrWhiteSpace(stockStatus) && stockStatus != "All Status" && stockStatus != "Status")
            {
                products = products.Where(p => p.StockStatus == stockStatus).ToList();
            }

            return products;
        }

        // ===== VALIDATION =====
        public bool IsSKUAvailable(string sku, int? excludeProductId = null)
        {
            using (var context = new CycleDeskDbContext())
            {
                if (excludeProductId.HasValue)
                {
                    return !context.Products.Any(p => p.SKU == sku && p.ProductId != excludeProductId.Value);
                }
                return !context.Products.Any(p => p.SKU == sku);
            }
        }

        // ===== STATISTICS =====
        public Dictionary<string, object> GetProductStatistics()
        {
            var products = GetAllProductsWithDetails();
            
            return new Dictionary<string, object>
            {
                { "TotalProducts", products.Count },
                { "InStock", products.Count(p => p.StockStatus == "In Stock") },
                { "LowStock", products.Count(p => p.StockStatus == "Low Stock") },
                { "Critical", products.Count(p => p.StockStatus == "Critical") },
                { "OutOfStock", products.Count(p => p.StockStatus == "Out of Stock") },
                { "TotalInventoryValue", products.Sum(p => p.QuantityInStock * p.PurchasePrice) },
                { "TotalSellingValue", products.Sum(p => p.QuantityInStock * p.SellingPrice) }
            };
        }

        // ===== HELPERS =====
        public List<Category> GetAllCategories()
        {
            using (var context = new CycleDeskDbContext())
            {
                return context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.CategoryName)
                    .ToList();
            }
        }

        public List<Supplier> GetAllSuppliers()
        {
            using (var context = new CycleDeskDbContext())
            {
                return context.Suppliers
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SupplierName)
                    .ToList();
            }
        }

        public string GenerateSKU(string categoryPrefix = "PRD")
        {
            using (var context = new CycleDeskDbContext())
            {
                var lastProduct = context.Products
                    .Where(p => p.SKU.StartsWith(categoryPrefix))
                    .OrderByDescending(p => p.ProductId)
                    .FirstOrDefault();

                int nextNumber = 1;
                if (lastProduct != null)
                {
                    var parts = lastProduct.SKU.Split('-');
                    if (parts.Length > 1 && int.TryParse(parts.Last(), out int num))
                    {
                        nextNumber = num + 1;
                    }
                }

                return $"{categoryPrefix}-{nextNumber:D3}";
            }
        }
    }
}
