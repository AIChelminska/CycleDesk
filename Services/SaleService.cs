using CycleDesk.Data;
using CycleDesk.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CycleDesk.Services
{
    /// <summary>
    /// DTO for products available for sale
    /// </summary>
    public class SaleProductDto
    {
        public int ProductId { get; set; }
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal VATRate { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable => StockQuantity > 0;
    }

    /// <summary>
    /// DTO for cart item display
    /// </summary>
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal VATRate { get; set; }
        public int Quantity { get; set; }
        public int MaxQuantity { get; set; }

        public decimal NetAmount => UnitPrice * Quantity;
        public decimal VATAmount => NetAmount * (VATRate / 100);
        public decimal GrossAmount => NetAmount + VATAmount;
    }

    /// <summary>
    /// DTO for sale summary
    /// </summary>
    public class SaleSummaryDto
    {
        public int ItemCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal SubtotalNet { get; set; }
        public decimal TotalVAT { get; set; }
        public decimal TotalGross { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
    }

    /// <summary>
    /// Request object for completing a sale
    /// </summary>
    public class CompleteSaleRequest
    {
        public List<CartItemDto> Items { get; set; }
        public string PaymentMethod { get; set; } // "Cash" or "Card"
        public decimal PaidAmount { get; set; }
        public decimal ChangeAmount { get; set; }
        public string DocumentType { get; set; } // "Receipt" or "Invoice"
        
        // Invoice data (optional)
        public string CompanyName { get; set; }
        public string NIP { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        
        public int SoldByUserId { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Result of completed sale
    /// </summary>
    public class CompleteSaleResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? SaleId { get; set; }
        public string SaleNumber { get; set; }
        public int? InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
    }

    /// <summary>
    /// Service for managing sales operations
    /// </summary>
    public class SaleService
    {
        /// <summary>
        /// Get all available products for sale
        /// </summary>
        public List<SaleProductDto> GetAvailableProducts()
        {
            try
            {
                using (var context = new CycleDeskDbContext())
                {
                    var products = context.Products
                        .Where(p => p.IsActive)
                        .Select(p => new
                        {
                            Product = p,
                            Category = context.Categories
                                .Where(c => c.CategoryId == p.CategoryId)
                                .Select(c => c.CategoryName)
                                .FirstOrDefault(),
                            Stock = context.Inventory
                                .Where(i => i.ProductId == p.ProductId)
                                .Select(i => i.QuantityInStock)
                                .FirstOrDefault()
                        })
                        .ToList()
                        .Select(x => new SaleProductDto
                        {
                            ProductId = x.Product.ProductId,
                            SKU = x.Product.SKU,
                            ProductName = x.Product.ProductName,
                            CategoryName = x.Category ?? "No Category",
                            UnitPrice = x.Product.SellingPrice,
                            VATRate = x.Product.VAT,
                            StockQuantity = x.Stock
                        })
                        .OrderBy(p => p.ProductName)
                        .ToList();

                    return products;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting available products: {ex.Message}");
                return new List<SaleProductDto>();
            }
        }

        /// <summary>
        /// Search products by name or SKU
        /// </summary>
        public List<SaleProductDto> SearchProducts(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return GetAvailableProducts();

            try
            {
                using (var context = new CycleDeskDbContext())
                {
                    var searchLower = searchText.ToLower();

                    var products = context.Products
                        .Where(p => p.IsActive && 
                               (p.ProductName.ToLower().Contains(searchLower) ||
                                p.SKU.ToLower().Contains(searchLower)))
                        .Select(p => new
                        {
                            Product = p,
                            Category = context.Categories
                                .Where(c => c.CategoryId == p.CategoryId)
                                .Select(c => c.CategoryName)
                                .FirstOrDefault(),
                            Stock = context.Inventory
                                .Where(i => i.ProductId == p.ProductId)
                                .Select(i => i.QuantityInStock)
                                .FirstOrDefault()
                        })
                        .ToList()
                        .Select(x => new SaleProductDto
                        {
                            ProductId = x.Product.ProductId,
                            SKU = x.Product.SKU,
                            ProductName = x.Product.ProductName,
                            CategoryName = x.Category ?? "No Category",
                            UnitPrice = x.Product.SellingPrice,
                            VATRate = x.Product.VAT,
                            StockQuantity = x.Stock
                        })
                        .OrderBy(p => p.ProductName)
                        .ToList();

                    return products;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching products: {ex.Message}");
                return new List<SaleProductDto>();
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        public SaleProductDto GetProductById(int productId)
        {
            try
            {
                using (var context = new CycleDeskDbContext())
                {
                    var product = context.Products
                        .Where(p => p.ProductId == productId && p.IsActive)
                        .Select(p => new
                        {
                            Product = p,
                            Category = context.Categories
                                .Where(c => c.CategoryId == p.CategoryId)
                                .Select(c => c.CategoryName)
                                .FirstOrDefault(),
                            Stock = context.Inventory
                                .Where(i => i.ProductId == p.ProductId)
                                .Select(i => i.QuantityInStock)
                                .FirstOrDefault()
                        })
                        .FirstOrDefault();

                    if (product == null)
                        return null;

                    return new SaleProductDto
                    {
                        ProductId = product.Product.ProductId,
                        SKU = product.Product.SKU,
                        ProductName = product.Product.ProductName,
                        CategoryName = product.Category ?? "No Category",
                        UnitPrice = product.Product.SellingPrice,
                        VATRate = product.Product.VAT,
                        StockQuantity = product.Stock
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting product: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get product by SKU (barcode)
        /// </summary>
        public SaleProductDto GetProductBySKU(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return null;

            try
            {
                using (var context = new CycleDeskDbContext())
                {
                    var product = context.Products
                        .Where(p => p.SKU == sku && p.IsActive)
                        .Select(p => new
                        {
                            Product = p,
                            Category = context.Categories
                                .Where(c => c.CategoryId == p.CategoryId)
                                .Select(c => c.CategoryName)
                                .FirstOrDefault(),
                            Stock = context.Inventory
                                .Where(i => i.ProductId == p.ProductId)
                                .Select(i => i.QuantityInStock)
                                .FirstOrDefault()
                        })
                        .FirstOrDefault();

                    if (product == null)
                        return null;

                    return new SaleProductDto
                    {
                        ProductId = product.Product.ProductId,
                        SKU = product.Product.SKU,
                        ProductName = product.Product.ProductName,
                        CategoryName = product.Category ?? "No Category",
                        UnitPrice = product.Product.SellingPrice,
                        VATRate = product.Product.VAT,
                        StockQuantity = product.Stock
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting product by SKU: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Calculate sale summary from cart items
        /// </summary>
        public SaleSummaryDto CalculateSummary(List<CartItemDto> items, decimal discountPercentage = 0)
        {
            var summary = new SaleSummaryDto
            {
                ItemCount = items.Count,
                TotalQuantity = items.Sum(i => i.Quantity),
                SubtotalNet = items.Sum(i => i.NetAmount),
                TotalVAT = items.Sum(i => i.VATAmount),
                TotalGross = items.Sum(i => i.GrossAmount)
            };

            if (discountPercentage > 0)
            {
                summary.DiscountAmount = summary.TotalGross * (discountPercentage / 100);
            }

            summary.FinalAmount = summary.TotalGross - summary.DiscountAmount;

            return summary;
        }

        /// <summary>
        /// Generate next sale number
        /// </summary>
        public string GenerateSaleNumber()
        {
            try
            {
                using (var context = new CycleDeskDbContext())
                {
                    var today = DateTime.Today;
                    var prefix = $"S-{today:yyyy}-";

                    var lastSale = context.Sales
                        .Where(s => s.SaleNumber.StartsWith(prefix))
                        .OrderByDescending(s => s.SaleNumber)
                        .FirstOrDefault();

                    int nextNumber = 1;
                    if (lastSale != null)
                    {
                        var lastNumberStr = lastSale.SaleNumber.Replace(prefix, "");
                        if (int.TryParse(lastNumberStr, out int lastNumber))
                        {
                            nextNumber = lastNumber + 1;
                        }
                    }

                    return $"{prefix}{nextNumber:D3}";
                }
            }
            catch
            {
                return $"S-{DateTime.Now:yyyyMMddHHmmss}";
            }
        }

        /// <summary>
        /// Generate next invoice number
        /// </summary>
        public string GenerateInvoiceNumber()
        {
            try
            {
                using (var context = new CycleDeskDbContext())
                {
                    var today = DateTime.Today;
                    var prefix = $"FV/{today:yyyy}/{today:MM}/";

                    var lastInvoice = context.Invoices
                        .Where(i => i.InvoiceNumber.StartsWith(prefix))
                        .OrderByDescending(i => i.InvoiceNumber)
                        .FirstOrDefault();

                    int nextNumber = 1;
                    if (lastInvoice != null)
                    {
                        var lastNumberStr = lastInvoice.InvoiceNumber.Replace(prefix, "");
                        if (int.TryParse(lastNumberStr, out int lastNumber))
                        {
                            nextNumber = lastNumber + 1;
                        }
                    }

                    return $"{prefix}{nextNumber:D4}";
                }
            }
            catch
            {
                return $"FV/{DateTime.Now:yyyy}/{DateTime.Now:MM}/{DateTime.Now:ddHHmmss}";
            }
        }

        /// <summary>
        /// Complete a sale transaction
        /// </summary>
        public CompleteSaleResult CompleteSale(CompleteSaleRequest request)
        {
            var result = new CompleteSaleResult { Success = false };

            if (request.Items == null || request.Items.Count == 0)
            {
                result.Message = "Cart is empty. Cannot complete sale.";
                return result;
            }

            try
            {
                using (var context = new CycleDeskDbContext())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            // 1. Verify stock availability
                            foreach (var item in request.Items)
                            {
                                var inventory = context.Inventory
                                    .FirstOrDefault(i => i.ProductId == item.ProductId);

                                if (inventory == null || inventory.QuantityInStock < item.Quantity)
                                {
                                    result.Message = $"Insufficient stock for {item.ProductName}. Available: {inventory?.QuantityInStock ?? 0}";
                                    return result;
                                }
                            }

                            // 2. Calculate totals
                            decimal subtotalNet = request.Items.Sum(i => i.NetAmount);
                            decimal totalVAT = request.Items.Sum(i => i.VATAmount);
                            decimal totalGross = request.Items.Sum(i => i.GrossAmount);

                            // 3. Create Sale record
                            var sale = new Sale
                            {
                                SaleNumber = GenerateSaleNumber(),
                                SaleDate = DateTime.Now,
                                CustomerName = request.DocumentType == "Invoice" ? request.CompanyName : "Walk-in Customer",
                                PaymentMethod = request.PaymentMethod,
                                SubtotalAmount = subtotalNet,
                                DiscountAmount = 0,
                                DiscountPercentage = 0,
                                VATAmount = totalVAT,
                                TotalAmount = totalGross,
                                PaidAmount = request.PaidAmount,
                                ChangeAmount = request.ChangeAmount,
                                Status = "Completed",
                                Notes = request.Notes,
                                SoldByUserId = request.SoldByUserId,
                                CreatedDate = DateTime.Now
                            };

                            context.Sales.Add(sale);
                            context.SaveChanges();

                            result.SaleId = sale.SaleId;
                            result.SaleNumber = sale.SaleNumber;

                            // 4. Create SaleItems
                            foreach (var item in request.Items)
                            {
                                var saleItem = new SaleItem
                                {
                                    SaleId = sale.SaleId,
                                    ProductId = item.ProductId,
                                    ProductName = item.ProductName,
                                    Quantity = item.Quantity,
                                    UnitPrice = item.UnitPrice,
                                    DiscountAmount = 0,
                                    VATRate = item.VATRate,
                                    VATAmount = item.VATAmount,
                                    TotalPrice = item.GrossAmount
                                };

                                context.SaleItems.Add(saleItem);
                            }

                            // 5. Update inventory (deduct stock) and record adjustments
                            foreach (var item in request.Items)
                            {
                                var inventory = context.Inventory
                                    .FirstOrDefault(i => i.ProductId == item.ProductId);

                                if (inventory != null)
                                {
                                    inventory.QuantityInStock -= item.Quantity;
                                    inventory.LastStockUpdate = DateTime.Now;
                                }

                                // Add inventory adjustment entry
                                var adjustment = new InventoryAdjustment
                                {
                                    ProductId = item.ProductId,
                                    AdjustmentType = "Subtraction",
                                    Quantity = item.Quantity,
                                    Reason = $"Sale: {sale.SaleNumber}",
                                    AdjustmentDate = DateTime.Now,
                                    AdjustedByUserId = request.SoldByUserId
                                };

                                context.InventoryAdjustments.Add(adjustment);
                            }

                            // 6. Create Invoice if requested
                            if (request.DocumentType == "Invoice")
                            {
                                // First, find or create customer
                                int? customerId = null;
                                if (!string.IsNullOrWhiteSpace(request.NIP))
                                {
                                    var existingCustomer = context.Customers
                                        .FirstOrDefault(c => c.TaxNumber == request.NIP);
                                    
                                    if (existingCustomer != null)
                                    {
                                        customerId = existingCustomer.CustomerId;
                                    }
                                    else
                                    {
                                        var newCustomer = new Customer
                                        {
                                            CustomerName = request.CompanyName,
                                            TaxNumber = request.NIP,
                                            Address = request.Address,
                                            City = request.City,
                                            PostalCode = request.PostalCode,
                                            IsCompany = true,
                                            IsActive = true,
                                            CreatedDate = DateTime.Now
                                        };
                                        context.Customers.Add(newCustomer);
                                        context.SaveChanges();
                                        customerId = newCustomer.CustomerId;
                                    }
                                }

                                var invoice = new Invoice
                                {
                                    InvoiceNumber = GenerateInvoiceNumber(),
                                    InvoiceType = "VAT Invoice",
                                    SaleId = sale.SaleId,
                                    CustomerId = customerId,
                                    IssueDate = DateTime.Now,
                                    DueDate = DateTime.Now.AddDays(14),
                                    PaymentMethod = request.PaymentMethod,
                                    SubtotalAmount = subtotalNet,
                                    VATAmount = totalVAT,
                                    TotalAmount = totalGross,
                                    Status = "Paid",
                                    Notes = request.Notes,
                                    IssuedByUserId = request.SoldByUserId,
                                    CreatedDate = DateTime.Now
                                };

                                context.Invoices.Add(invoice);
                                context.SaveChanges();

                                result.InvoiceId = invoice.InvoiceId;
                                result.InvoiceNumber = invoice.InvoiceNumber;
                            }

                            context.SaveChanges();
                            transaction.Commit();

                            result.Success = true;
                            result.Message = request.DocumentType == "Invoice"
                                ? $"Sale completed successfully! Invoice: {result.InvoiceNumber}"
                                : $"Sale completed successfully! Receipt: {result.SaleNumber}";

                            return result;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            result.Message = $"Error completing sale: {ex.Message}";
                            System.Diagnostics.Debug.WriteLine($"Sale error: {ex}");
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = $"Database error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Database error: {ex}");
                return result;
            }
        }

        /// <summary>
        /// Get user ID by username
        /// </summary>
        public int GetUserIdByUsername(string username)
        {
            try
            {
                using (var context = new CycleDeskDbContext())
                {
                    var user = context.Users.FirstOrDefault(u => u.Username == username);
                    return user?.UserId ?? 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get today's sales statistics
        /// </summary>
        public (int SalesCount, decimal TotalAmount) GetTodayStats()
        {
            try
            {
                using (var context = new CycleDeskDbContext())
                {
                    var today = DateTime.Today;
                    var tomorrow = today.AddDays(1);

                    var stats = context.Sales
                        .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow && s.Status == "Completed")
                        .GroupBy(s => 1)
                        .Select(g => new
                        {
                            Count = g.Count(),
                            Total = g.Sum(s => s.TotalAmount)
                        })
                        .FirstOrDefault();

                    return (stats?.Count ?? 0, stats?.Total ?? 0);
                }
            }
            catch
            {
                return (0, 0);
            }
        }
    }
}
