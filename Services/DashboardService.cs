using System;
using System.Collections.Generic;
using System.Linq;
using CycleDesk.Data;
using CycleDesk.Models;

namespace CycleDesk.Services
{
    #region DTOs

    /// <summary>
    /// KPI cards data for dashboard
    /// </summary>
    public class DashboardKpiDto
    {
        public decimal TodaySales { get; set; }
        public decimal TodaySalesChange { get; set; } // % change vs yesterday
        public bool TodaySalesUp { get; set; }

        public int TodayTransactions { get; set; }
        public decimal TransactionsChange { get; set; }
        public bool TransactionsUp { get; set; }

        public int LowStockCount { get; set; }

        public decimal WarehouseValue { get; set; }
    }

    /// <summary>
    /// Top selling product DTO
    /// </summary>
    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    /// <summary>
    /// Product to order (low stock) DTO
    /// </summary>
    public class ProductToOrderDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public string StockColor { get; set; } // Red for critical, Yellow for low
    }

    /// <summary>
    /// Recent goods receipt DTO for dashboard
    /// </summary>
    public class RecentGoodsReceiptDto
    {
        public int GoodsReceiptId { get; set; }
        public string ReceiptNumber { get; set; }
        public string SupplierName { get; set; }
        public DateTime ReceiptDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string CreatedByName { get; set; }
        public string Status { get; set; }
        public string StatusColor { get; set; }
        public string StatusBackground { get; set; }
        public string StatusIcon { get; set; }
    }

    /// <summary>
    /// Sales chart data point
    /// </summary>
    public class SalesChartDataDto
    {
        public DateTime Date { get; set; }
        public string Label { get; set; } // e.g. "Mon", "Tue" or "01.11"
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
    }

    #endregion

    public class DashboardService
    {
        /// <summary>
        /// Get KPI data for the 4 main cards
        /// </summary>
        public DashboardKpiDto GetKpiData()
        {
            try
            {
                using (var context = new CycleDeskDbContext())
                {
                    var today = DateTime.Today;
                    var yesterday = today.AddDays(-1);
                    var tomorrow = today.AddDays(1);

                    // Today's sales
                    var todaySales = context.Sales
                        .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow && s.Status == "Completed")
                        .ToList();

                    var todayTotal = todaySales.Sum(s => s.TotalAmount);
                    var todayCount = todaySales.Count;

                    // Yesterday's sales for comparison
                    var yesterdaySales = context.Sales
                        .Where(s => s.SaleDate >= yesterday && s.SaleDate < today && s.Status == "Completed")
                        .ToList();

                    var yesterdayTotal = yesterdaySales.Sum(s => s.TotalAmount);
                    var yesterdayCount = yesterdaySales.Count;

                    // Calculate changes
                    decimal salesChange = 0;
                    if (yesterdayTotal > 0)
                        salesChange = ((todayTotal - yesterdayTotal) / yesterdayTotal) * 100;

                    decimal transactionsChange = 0;
                    if (yesterdayCount > 0)
                        transactionsChange = ((decimal)(todayCount - yesterdayCount) / yesterdayCount) * 100;

                    // Low stock count
                    var lowStockCount = (from p in context.Products
                                         join i in context.Inventory on p.ProductId equals i.ProductId into invGroup
                                         from inv in invGroup.DefaultIfEmpty()
                                         where p.IsActive && (inv == null || inv.QuantityInStock <= p.ReorderLevel)
                                         select p).Count();

                    // Warehouse value
                    var warehouseValue = (from p in context.Products
                                          join i in context.Inventory on p.ProductId equals i.ProductId into invGroup
                                          from inv in invGroup.DefaultIfEmpty()
                                          where p.IsActive
                                          select new { Price = p.SellingPrice, Qty = inv != null ? inv.QuantityInStock : 0 })
                                          .ToList()
                                          .Sum(x => x.Price * x.Qty);

                    return new DashboardKpiDto
                    {
                        TodaySales = todayTotal,
                        TodaySalesChange = Math.Abs(salesChange),
                        TodaySalesUp = salesChange >= 0,
                        TodayTransactions = todayCount,
                        TransactionsChange = Math.Abs(transactionsChange),
                        TransactionsUp = transactionsChange >= 0,
                        LowStockCount = lowStockCount,
                        WarehouseValue = warehouseValue
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting KPI data: {ex.Message}");
                return new DashboardKpiDto();
            }
        }

        /// <summary>
        /// Get top 5 selling products by revenue
        /// </summary>
        public List<TopProductDto> GetTopProducts(int days = 30, int count = 5)
        {
            try
            {
                using (var context = new CycleDeskDbContext())
                {
                    var startDate = DateTime.Today.AddDays(-days);

                    var topProducts = (from si in context.SaleItems
                                       join s in context.Sales on si.SaleId equals s.SaleId
                                       join p in context.Products on si.ProductId equals p.ProductId
                                       where s.SaleDate >= startDate && s.Status == "Completed"
                                       group new { si, p } by new { p.ProductId, p.ProductName } into g
                                       orderby g.Sum(x => x.si.TotalPrice) descending
                                       select new TopProductDto
                                       {
                                           ProductId = g.Key.ProductId,
                                           ProductName = g.Key.ProductName,
                                           QuantitySold = g.Sum(x => x.si.Quantity),
                                           Revenue = g.Sum(x => x.si.TotalPrice)
                                       })
                                       .Take(count)
                                       .ToList();

                    return topProducts;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting top products: {ex.Message}");
                return new List<TopProductDto>();
            }
        }

        /// <summary>
        /// Get products to order (low stock)
        /// </summary>
        public List<ProductToOrderDto> GetProductsToOrder(int count = 5)
        {
            try
            {
                using (var context = new CycleDeskDbContext())
                {
                    var products = (from p in context.Products
                                    join i in context.Inventory on p.ProductId equals i.ProductId into invGroup
                                    from inv in invGroup.DefaultIfEmpty()
                                    where p.IsActive && (inv == null || inv.QuantityInStock <= p.ReorderLevel)
                                    orderby (inv != null ? inv.QuantityInStock : 0) ascending
                                    select new
                                    {
                                        p.ProductId,
                                        p.ProductName,
                                        p.MinimumStock,
                                        CurrentStock = inv != null ? inv.QuantityInStock : 0
                                    })
                                    .Take(count)
                                    .ToList();

                    return products.Select(p => new ProductToOrderDto
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        CurrentStock = p.CurrentStock,
                        MinimumStock = p.MinimumStock,
                        StockColor = p.CurrentStock <= p.MinimumStock ? "#FFDC3545" : "#FFFFC107"
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting products to order: {ex.Message}");
                return new List<ProductToOrderDto>();
            }
        }

        /// <summary>
        /// Get recent goods receipts
        /// </summary>
        public List<RecentGoodsReceiptDto> GetRecentGoodsReceipts(int count = 6)
        {
            try
            {
                using (var context = new CycleDeskDbContext())
                {
                    var receipts = (from gr in context.GoodsReceipts
                                    join s in context.Suppliers on gr.SupplierId equals s.SupplierId into suppGroup
                                    from supplier in suppGroup.DefaultIfEmpty()
                                    join u in context.Users on gr.ReceivedByUserId equals u.UserId into userGroup
                                    from user in userGroup.DefaultIfEmpty()
                                    orderby gr.ReceiptDate descending
                                    select new
                                    {
                                        gr.GoodsReceiptId,
                                        gr.ReceiptNumber,
                                        SupplierName = supplier != null ? supplier.SupplierName : "Unknown",
                                        gr.ReceiptDate,
                                        gr.TotalAmount,
                                        CreatedByName = user != null ? (user.FirstName + " " + user.LastName) : "Unknown",
                                        gr.Status
                                    })
                                    .Take(count)
                                    .ToList();

                    return receipts.Select(r => new RecentGoodsReceiptDto
                    {
                        GoodsReceiptId = r.GoodsReceiptId,
                        ReceiptNumber = r.ReceiptNumber ?? "",
                        SupplierName = r.SupplierName,
                        ReceiptDate = r.ReceiptDate,
                        TotalAmount = r.TotalAmount,
                        CreatedByName = r.CreatedByName,
                        Status = MapStatus(r.Status),
                        StatusColor = GetStatusColor(r.Status),
                        StatusBackground = GetStatusBackground(r.Status),
                        StatusIcon = GetStatusIcon(r.Status)
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting recent goods receipts: {ex.Message}");
                return new List<RecentGoodsReceiptDto>();
            }
        }

        /// <summary>
        /// Get sales chart data for last N days
        /// </summary>
        public List<SalesChartDataDto> GetSalesChartData(int days = 7)
        {
            try
            {
                using (var context = new CycleDeskDbContext())
                {
                    var startDate = DateTime.Today.AddDays(-(days - 1));
                    var result = new List<SalesChartDataDto>();

                    // Get all sales in the period
                    var sales = context.Sales
                        .Where(s => s.SaleDate >= startDate && s.Status == "Completed")
                        .ToList();

                    // Get all goods receipts in the period
                    var receipts = context.GoodsReceipts
                        .Where(gr => gr.ReceiptDate >= startDate && gr.Status == "Received")
                        .ToList();

                    // Build data for each day
                    for (int i = 0; i < days; i++)
                    {
                        var date = startDate.AddDays(i);
                        var nextDate = date.AddDays(1);

                        var dayIncome = sales
                            .Where(s => s.SaleDate >= date && s.SaleDate < nextDate)
                            .Sum(s => s.TotalAmount);

                        var dayExpenses = receipts
                            .Where(r => r.ReceiptDate >= date && r.ReceiptDate < nextDate)
                            .Sum(r => r.TotalAmount);

                        result.Add(new SalesChartDataDto
                        {
                            Date = date,
                            Label = date.ToString("ddd"),
                            Income = dayIncome,
                            Expenses = dayExpenses
                        });
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting sales chart data: {ex.Message}");
                return new List<SalesChartDataDto>();
            }
        }

        /// <summary>
        /// Get sales chart data for last N days (30 days version)
        /// </summary>
        public List<SalesChartDataDto> GetSalesChartData30Days()
        {
            return GetSalesChartData(30);
        }

        /// <summary>
        /// Get sales chart data for last 90 days
        /// </summary>
        public List<SalesChartDataDto> GetSalesChartData90Days()
        {
            return GetSalesChartData(90);
        }

        #region Helpers

        private string MapStatus(string status)
        {
            return (status ?? "").ToLower() switch
            {
                "received" => "Approved",
                "pending" => "Pending",
                "cancelled" => "Cancelled",
                "draft" => "Draft",
                _ => status ?? "Unknown"
            };
        }

        private string GetStatusColor(string status)
        {
            return (status ?? "").ToLower() switch
            {
                "received" => "#FF28A745",
                "pending" => "#0C5460",
                "cancelled" => "#FFDC3545",
                "draft" => "#FFFFC107",
                _ => "#FF6C757D"
            };
        }

        private string GetStatusBackground(string status)
        {
            return (status ?? "").ToLower() switch
            {
                "received" => "#FFD4EDDA",
                "pending" => "#D1ECF1",
                "cancelled" => "#FFF8D7DA",
                "draft" => "#FFFEF3CD",
                _ => "#FFE9ECEF"
            };
        }

        private string GetStatusIcon(string status)
        {
            return (status ?? "").ToLower() switch
            {
                "received" => "✓",
                "pending" => "◐",
                "cancelled" => "✗",
                "draft" => "○",
                _ => "?"
            };
        }

        #endregion
    }
}
