using System;
using System.Collections.Generic;
using System.Linq;
using CycleDesk.Data;
using CycleDesk.Models;

namespace CycleDesk.Services
{
    // DTO dla wyświetlania statusu magazynowego
    public class InventoryStatusDto
    {
        public int ProductId { get; set; }
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string CategoryEmoji { get; set; }
        public int QuantityInStock { get; set; }
        public int MinimumStock { get; set; }
        public int ReorderLevel { get; set; }
        public string ImagePath { get; set; }
        public string Status { get; set; }           // "In Stock", "Low Stock", "Critical", "Out of Stock"
        public string StatusColor { get; set; }      // Hex color for status
        public string StatusBackground { get; set; } // Hex background for status badge
        public string LocationInWarehouse { get; set; }
    }

    // DTO dla statystyk KPI
    public class InventoryKpiDto
    {
        public int OutOfStockCount { get; set; }
        public int CriticalCount { get; set; }
        public int LowStockCount { get; set; }
        public int InStockCount { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
    }

    public class InventoryService
    {
        private readonly CycleDeskDbContext _context;

        public InventoryService()
        {
            _context = new CycleDeskDbContext();
        }

        /// <summary>
        /// Pobiera statystyki KPI dla dashboard
        /// </summary>
        public InventoryKpiDto GetInventoryKpi()
        {
            // Łączymy Products z Inventory
            var productsWithStock = (from p in _context.Products
                                     join i in _context.Inventory on p.ProductId equals i.ProductId into invGroup
                                     from inv in invGroup.DefaultIfEmpty()
                                     where p.IsActive
                                     select new
                                     {
                                         Product = p,
                                         QuantityInStock = inv != null ? inv.QuantityInStock : 0
                                     }).ToList();

            var kpi = new InventoryKpiDto
            {
                TotalProducts = productsWithStock.Count,
                OutOfStockCount = 0,
                CriticalCount = 0,
                LowStockCount = 0,
                InStockCount = 0,
                TotalInventoryValue = 0
            };

            foreach (var item in productsWithStock)
            {
                var product = item.Product;
                var stock = item.QuantityInStock;

                // Oblicz wartość magazynu
                kpi.TotalInventoryValue += stock * product.SellingPrice;

                // Kategoryzuj według statusu
                if (stock == 0)
                {
                    kpi.OutOfStockCount++;
                }
                else if (stock <= product.MinimumStock)
                {
                    kpi.CriticalCount++;
                }
                else if (stock < product.ReorderLevel)
                {
                    kpi.LowStockCount++;
                }
                else
                {
                    kpi.InStockCount++;
                }
            }

            return kpi;
        }

        /// <summary>
        /// Pobiera wszystkie produkty ze statusem magazynowym
        /// </summary>
        public List<InventoryStatusDto> GetAllInventoryStatus()
        {
            var products = (from p in _context.Products
                            join c in _context.Categories on p.CategoryId equals c.CategoryId into catGroup
                            from category in catGroup.DefaultIfEmpty()
                            join i in _context.Inventory on p.ProductId equals i.ProductId into invGroup
                            from inv in invGroup.DefaultIfEmpty()
                            where p.IsActive
                            orderby (inv != null ? inv.QuantityInStock : 0) ascending, p.ProductName
                            select new
                            {
                                Product = p,
                                CategoryName = category != null ? category.CategoryName : "Uncategorized",
                                QuantityInStock = inv != null ? inv.QuantityInStock : 0,
                                LocationInWarehouse = inv != null ? inv.LocationInWarehouse : ""
                            }).ToList();

            return products.Select(x => MapToInventoryStatusDto(
                x.Product,
                x.CategoryName,
                x.QuantityInStock,
                x.LocationInWarehouse
            )).ToList();
        }

        /// <summary>
        /// Filtruje produkty według statusu i kategorii
        /// </summary>
        public List<InventoryStatusDto> Filter(string searchTerm, int? categoryId, string status)
        {
            var query = from p in _context.Products
                        join c in _context.Categories on p.CategoryId equals c.CategoryId into catGroup
                        from category in catGroup.DefaultIfEmpty()
                        join i in _context.Inventory on p.ProductId equals i.ProductId into invGroup
                        from inv in invGroup.DefaultIfEmpty()
                        where p.IsActive
                        select new
                        {
                            Product = p,
                            CategoryName = category != null ? category.CategoryName : "Uncategorized",
                            QuantityInStock = inv != null ? inv.QuantityInStock : 0,
                            LocationInWarehouse = inv != null ? inv.LocationInWarehouse : ""
                        };

            // Filtr kategorii
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(x => x.Product.CategoryId == categoryId.Value);
            }

            // Filtr wyszukiwania
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(x =>
                    x.Product.ProductName.ToLower().Contains(searchTerm) ||
                    x.Product.SKU.ToLower().Contains(searchTerm));
            }

            var results = query.ToList()
                              .OrderBy(x => x.QuantityInStock)
                              .ThenBy(x => x.Product.ProductName)
                              .ToList();

            var mappedResults = results.Select(x => MapToInventoryStatusDto(
                x.Product,
                x.CategoryName,
                x.QuantityInStock,
                x.LocationInWarehouse
            )).ToList();

            // Filtr statusu (po mapowaniu)
            if (!string.IsNullOrWhiteSpace(status) && status != "All Status" && status != "Status")
            {
                mappedResults = mappedResults.Where(p => p.Status == status).ToList();
            }

            return mappedResults;
        }

        /// <summary>
        /// Pobiera listę kategorii do ComboBox
        /// </summary>
        public List<Category> GetCategories()
        {
            return _context.Categories
                          .Where(c => c.IsActive)
                          .OrderBy(c => c.CategoryName)
                          .ToList();
        }

        /// <summary>
        /// Aktualizuje stan magazynowy produktu
        /// </summary>
        public bool UpdateStock(int productId, int newQuantity)
        {
            var inventory = _context.Inventory.FirstOrDefault(i => i.ProductId == productId);

            if (inventory == null)
            {
                // Utwórz nowy rekord Inventory jeśli nie istnieje
                inventory = new Inventory
                {
                    ProductId = productId,
                    QuantityInStock = newQuantity,
                    LastStockUpdate = DateTime.Now,
                    LocationInWarehouse = ""
                };
                _context.Inventory.Add(inventory);
            }
            else
            {
                inventory.QuantityInStock = newQuantity;
                inventory.LastStockUpdate = DateTime.Now;
            }

            _context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Mapuje Product do InventoryStatusDto
        /// </summary>
        private InventoryStatusDto MapToInventoryStatusDto(Product product, string categoryName, int quantityInStock, string location)
        {
            var dto = new InventoryStatusDto
            {
                ProductId = product.ProductId,
                SKU = product.SKU,
                ProductName = product.ProductName,
                CategoryName = categoryName,
                CategoryEmoji = GetCategoryEmoji(categoryName),
                QuantityInStock = quantityInStock,
                MinimumStock = product.MinimumStock,
                ReorderLevel = product.ReorderLevel,
                ImagePath = product.ImagePath,
                LocationInWarehouse = location
            };

            // Określ status
            if (quantityInStock == 0)
            {
                dto.Status = "Out of Stock";
                dto.StatusColor = "#FFDC3545";
                dto.StatusBackground = "#FFF8D7DA";
            }
            else if (quantityInStock <= product.MinimumStock)
            {
                dto.Status = "Critical";
                dto.StatusColor = "#FFDC3545";
                dto.StatusBackground = "#FFF8D7DA";
            }
            else if (quantityInStock < product.ReorderLevel)
            {
                dto.Status = "Low Stock";
                dto.StatusColor = "#FFFFC107";
                dto.StatusBackground = "#FFFFF3CD";
            }
            else
            {
                dto.Status = "In Stock";
                dto.StatusColor = "#FF28A745";
                dto.StatusBackground = "#FFD4EDDA";
            }

            return dto;
        }

        /// <summary>
        /// Zwraca emoji dla kategorii
        /// </summary>
        private string GetCategoryEmoji(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
                return "📦";

            return categoryName.ToLower() switch
            {
                "bikes" or "rowery" => "🚲",
                "tires" or "opony" => "🛞",
                "chains" or "łańcuchy" => "⛓️",
                "brakes" or "hamulce" => "🛑",
                "lights" or "światła" => "💡",
                "helmets" or "kaski" => "⛑️",
                "accessories" or "akcesoria" => "🎒",
                "tools" or "narzędzia" => "🔧",
                "pedals" or "pedały" => "🦶",
                "saddles" or "siodełka" => "🪑",
                "handlebars" or "kierownice" => "🎯",
                "wheels" or "koła" => "⭕",
                _ => "📦"
            };
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}