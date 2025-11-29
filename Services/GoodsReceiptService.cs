using System;
using System.Collections.Generic;
using System.Linq;
using CycleDesk.Data;
using CycleDesk.Models;

namespace CycleDesk.Services
{
    // DTO dla wyświetlania przyjęć towaru
    public class GoodsReceiptDisplayDto
    {
        public int GoodsReceiptId { get; set; }
        public string ReceiptNumber { get; set; }
        public string SupplierName { get; set; }
        public int SupplierId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string StatusColor { get; set; }
        public string StatusBackground { get; set; }
        public string ReceivedByName { get; set; }
        public int ReceivedByUserId { get; set; }
        public string Notes { get; set; }
        public int ItemCount { get; set; }
    }

    // DTO dla pozycji przyjęcia
    public class GoodsReceiptItemDisplayDto
    {
        public int GoodsReceiptItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string BatchNumber { get; set; }
    }

    // DTO dla KPI
    public class GoodsReceiptKpiDto
    {
        public int PendingCount { get; set; }
        public int CancelledCount { get; set; }
        public int ReceivedCount { get; set; }
        public decimal TotalValueThisMonth { get; set; }
    }

    public class GoodsReceiptService
    {
        private readonly CycleDeskDbContext _context;

        public GoodsReceiptService()
        {
            _context = new CycleDeskDbContext();
        }

        // ===== KPI =====
        public GoodsReceiptKpiDto GetKpi()
        {
            try
            {
                var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var receipts = _context.GoodsReceipts.ToList() ?? new List<GoodsReceipt>();

                return new GoodsReceiptKpiDto
                {
                    PendingCount = receipts.Count(r => r.Status == "Pending"),
                    CancelledCount = receipts.Count(r => r.Status == "Cancelled"),
                    ReceivedCount = receipts.Count(r => r.Status == "Received"),
                    TotalValueThisMonth = receipts
                        .Where(r => r.ReceiptDate >= startOfMonth && r.Status == "Received")
                        .Sum(r => r.TotalAmount)
                };
            }
            catch
            {
                return new GoodsReceiptKpiDto();
            }
        }

        // ===== CREATE =====
        public int CreateGoodsReceipt(GoodsReceipt receipt, List<GoodsReceiptItem> items)
        {
            if (receipt.CreatedDate == default)
                receipt.CreatedDate = DateTime.Now;
            if (receipt.ReceiptDate == default)
                receipt.ReceiptDate = DateTime.Now;
            if (string.IsNullOrEmpty(receipt.InvoiceNumber))
                receipt.InvoiceNumber = "";
            if (string.IsNullOrEmpty(receipt.Notes))
                receipt.Notes = "";
            if (string.IsNullOrEmpty(receipt.Status))
                receipt.Status = "Received";

            _context.GoodsReceipts.Add(receipt);
            _context.SaveChanges();

            foreach (var item in items)
            {
                item.GoodsReceiptId = receipt.GoodsReceiptId;
                if (string.IsNullOrEmpty(item.BatchNumber))
                    item.BatchNumber = "";
                _context.GoodsReceiptItems.Add(item);

                if (receipt.Status == "Received")
                {
                    UpdateInventoryStock(item.ProductId, item.Quantity);
                }
            }
            _context.SaveChanges();
            return receipt.GoodsReceiptId;
        }

        private void UpdateInventoryStock(int productId, int quantityToAdd)
        {
            var inventory = _context.Inventory.FirstOrDefault(i => i.ProductId == productId);

            if (inventory == null)
            {
                inventory = new Inventory
                {
                    ProductId = productId,
                    QuantityInStock = quantityToAdd,
                    LastStockUpdate = DateTime.Now,
                    LocationInWarehouse = ""
                };
                _context.Inventory.Add(inventory);
            }
            else
            {
                inventory.QuantityInStock += quantityToAdd;
                inventory.LastStockUpdate = DateTime.Now;
            }
        }

        // ===== READ =====
        public List<GoodsReceiptDisplayDto> GetAllReceipts()
        {
            try
            {
                var receipts = (from gr in _context.GoodsReceipts
                                join s in _context.Suppliers on gr.SupplierId equals s.SupplierId into suppGroup
                                from supplier in suppGroup.DefaultIfEmpty()
                                join u in _context.Users on gr.ReceivedByUserId equals u.UserId into userGroup
                                from user in userGroup.DefaultIfEmpty()
                                orderby gr.ReceiptDate descending
                                select new
                                {
                                    Receipt = gr,
                                    SupplierName = supplier != null ? supplier.SupplierName : "Unknown",
                                    ReceivedByName = user != null ? (user.FirstName + " " + user.LastName) : "Unknown"
                                }).ToList();

                return receipts.Select(x => MapToDisplayDto(x.Receipt, x.SupplierName, x.ReceivedByName)).ToList();
            }
            catch
            {
                return new List<GoodsReceiptDisplayDto>();
            }
        }

        public List<GoodsReceiptDisplayDto> Filter(string searchTerm, int? supplierId, string status, int? createdByUserId)
        {
            try
            {
                var query = from gr in _context.GoodsReceipts
                            join s in _context.Suppliers on gr.SupplierId equals s.SupplierId into suppGroup
                            from supplier in suppGroup.DefaultIfEmpty()
                            join u in _context.Users on gr.ReceivedByUserId equals u.UserId into userGroup
                            from user in userGroup.DefaultIfEmpty()
                            select new
                            {
                                Receipt = gr,
                                SupplierName = supplier != null ? supplier.SupplierName : "Unknown",
                                ReceivedByName = user != null ? (user.FirstName + " " + user.LastName) : "Unknown"
                            };

                if (supplierId.HasValue && supplierId.Value > 0)
                {
                    query = query.Where(x => x.Receipt.SupplierId == supplierId.Value);
                }

                if (!string.IsNullOrWhiteSpace(status) && status != "All Status" && status != "Status")
                {
                    query = query.Where(x => x.Receipt.Status == status);
                }

                if (createdByUserId.HasValue && createdByUserId.Value > 0)
                {
                    query = query.Where(x => x.Receipt.ReceivedByUserId == createdByUserId.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(x =>
                        x.Receipt.ReceiptNumber.ToLower().Contains(searchTerm) ||
                        (x.Receipt.InvoiceNumber != null && x.Receipt.InvoiceNumber.ToLower().Contains(searchTerm)) ||
                        x.SupplierName.ToLower().Contains(searchTerm));
                }

                var results = query.OrderByDescending(x => x.Receipt.ReceiptDate).ToList();
                return results.Select(x => MapToDisplayDto(x.Receipt, x.SupplierName, x.ReceivedByName)).ToList();
            }
            catch
            {
                return new List<GoodsReceiptDisplayDto>();
            }
        }

        public GoodsReceiptDisplayDto GetReceiptById(int receiptId)
        {
            try
            {
                var data = (from gr in _context.GoodsReceipts
                            join s in _context.Suppliers on gr.SupplierId equals s.SupplierId into suppGroup
                            from supplier in suppGroup.DefaultIfEmpty()
                            join u in _context.Users on gr.ReceivedByUserId equals u.UserId into userGroup
                            from user in userGroup.DefaultIfEmpty()
                            where gr.GoodsReceiptId == receiptId
                            select new
                            {
                                Receipt = gr,
                                SupplierName = supplier != null ? supplier.SupplierName : "Unknown",
                                ReceivedByName = user != null ? (user.FirstName + " " + user.LastName) : "Unknown"
                            }).FirstOrDefault();

                if (data == null) return null;
                return MapToDisplayDto(data.Receipt, data.SupplierName, data.ReceivedByName);
            }
            catch
            {
                return null;
            }
        }

        public List<GoodsReceiptItemDisplayDto> GetReceiptItems(int receiptId)
        {
            try
            {
                return (from ri in _context.GoodsReceiptItems
                        join p in _context.Products on ri.ProductId equals p.ProductId into prodGroup
                        from product in prodGroup.DefaultIfEmpty()
                        where ri.GoodsReceiptId == receiptId
                        select new GoodsReceiptItemDisplayDto
                        {
                            GoodsReceiptItemId = ri.GoodsReceiptItemId,
                            ProductId = ri.ProductId,
                            ProductName = product != null ? product.ProductName : "Unknown",
                            SKU = product != null ? product.SKU : "",
                            Quantity = ri.Quantity,
                            UnitPrice = ri.UnitPrice,
                            TotalPrice = ri.TotalPrice,
                            BatchNumber = ri.BatchNumber ?? ""
                        }).ToList();
            }
            catch
            {
                return new List<GoodsReceiptItemDisplayDto>();
            }
        }

        // ===== UPDATE =====
        public bool UpdateReceiptStatus(int receiptId, string newStatus)
        {
            try
            {
                var receipt = _context.GoodsReceipts.FirstOrDefault(r => r.GoodsReceiptId == receiptId);
                if (receipt == null) return false;

                var oldStatus = receipt.Status;
                receipt.Status = newStatus;

                if (oldStatus == "Pending" && newStatus == "Received")
                {
                    var items = _context.GoodsReceiptItems.Where(i => i.GoodsReceiptId == receiptId).ToList();
                    foreach (var item in items)
                    {
                        UpdateInventoryStock(item.ProductId, item.Quantity);
                    }
                }

                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ===== HELPERS =====
        public List<Supplier> GetSuppliers()
        {
            try
            {
                if (_context.Suppliers == null)
                    return new List<Supplier>();

                return _context.Suppliers
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SupplierName)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading suppliers: {ex.Message}");
                return new List<Supplier>();
            }
        }

        public List<User> GetUsers()
        {
            try
            {
                if (_context.Users == null)
                    return new List<User>();

                return _context.Users
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.FirstName)
                    .ToList();
            }
            catch
            {
                return new List<User>();
            }
        }

        public List<Product> GetProducts()
        {
            try
            {
                if (_context.Products == null)
                    return new List<Product>();

                return _context.Products
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.ProductName)
                    .ToList();
            }
            catch
            {
                return new List<Product>();
            }
        }

        public string GenerateReceiptNumber()
        {
            try
            {
                var today = DateTime.Now;
                var prefix = $"PZ/{today:yy}/{today:MM}/";

                var lastReceipt = _context.GoodsReceipts
                    .Where(r => r.ReceiptNumber.StartsWith(prefix))
                    .OrderByDescending(r => r.ReceiptNumber)
                    .FirstOrDefault();

                int nextNumber = 1;
                if (lastReceipt != null)
                {
                    var lastNumberStr = lastReceipt.ReceiptNumber.Substring(prefix.Length);
                    if (int.TryParse(lastNumberStr, out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }

                return $"{prefix}{nextNumber:D3}";
            }
            catch
            {
                return $"PZ/{DateTime.Now:yy}/{DateTime.Now:MM}/001";
            }
        }

        private GoodsReceiptDisplayDto MapToDisplayDto(GoodsReceipt receipt, string supplierName, string receivedByName)
        {
            int itemCount = 0;
            try
            {
                itemCount = _context.GoodsReceiptItems.Count(i => i.GoodsReceiptId == receipt.GoodsReceiptId);
            }
            catch { }

            var dto = new GoodsReceiptDisplayDto
            {
                GoodsReceiptId = receipt.GoodsReceiptId,
                ReceiptNumber = receipt.ReceiptNumber ?? "",
                SupplierName = supplierName,
                SupplierId = receipt.SupplierId,
                ReceiptDate = receipt.ReceiptDate,
                InvoiceNumber = receipt.InvoiceNumber ?? "",
                TotalAmount = receipt.TotalAmount,
                Status = receipt.Status ?? "Unknown",
                ReceivedByName = receivedByName,
                ReceivedByUserId = receipt.ReceivedByUserId,
                Notes = receipt.Notes ?? "",
                ItemCount = itemCount
            };

            switch ((receipt.Status ?? "").ToLower())
            {
                case "received":
                    dto.StatusColor = "#FF28A745";
                    dto.StatusBackground = "#FFD4EDDA";
                    break;
                case "pending":
                    dto.StatusColor = "#FFFFC107";
                    dto.StatusBackground = "#FFFFF3CD";
                    break;
                case "cancelled":
                    dto.StatusColor = "#FFDC3545";
                    dto.StatusBackground = "#FFF8D7DA";
                    break;
                default:
                    dto.StatusColor = "#FF6C757D";
                    dto.StatusBackground = "#FFE9ECEF";
                    break;
            }

            return dto;
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}