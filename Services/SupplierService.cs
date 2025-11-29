using CycleDesk.Data;
using CycleDesk.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CycleDesk.Services
{
    public class SupplierService
    {
        private readonly CycleDeskDbContext _context;

        public SupplierService()
        {
            _context = new CycleDeskDbContext();
        }

        // ===== GET ALL SUPPLIERS =====
        public List<Supplier> GetAllSuppliers()
        {
            try
            {
                if (_context.Suppliers == null)
                    return new List<Supplier>();

                return _context.Suppliers
                    .OrderBy(s => s.SupplierName)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading suppliers: {ex.Message}");
                return new List<Supplier>();
            }
        }

        // ===== GET ACTIVE SUPPLIERS =====
        public List<Supplier> GetActiveSuppliers()
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
                System.Diagnostics.Debug.WriteLine($"Error loading active suppliers: {ex.Message}");
                return new List<Supplier>();
            }
        }

        // ===== GET SUPPLIER BY ID =====
        public Supplier? GetSupplierById(int supplierId)
        {
            try
            {
                return _context.Suppliers?.FirstOrDefault(s => s.SupplierId == supplierId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting supplier by ID: {ex.Message}");
                return null;
            }
        }

        // ===== ADD NEW SUPPLIER =====
        public bool AddSupplier(Supplier supplier)
        {
            try
            {
                supplier.CreatedDate = DateTime.Now;
                supplier.IsActive = true;

                _context.Suppliers?.Add(supplier);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding supplier: {ex.Message}");
                return false;
            }
        }

        // ===== UPDATE SUPPLIER =====
        public bool UpdateSupplier(Supplier supplier)
        {
            try
            {
                var existingSupplier = _context.Suppliers?.FirstOrDefault(s => s.SupplierId == supplier.SupplierId);
                if (existingSupplier == null)
                    return false;

                existingSupplier.SupplierName = supplier.SupplierName;
                existingSupplier.ContactPerson = supplier.ContactPerson;
                existingSupplier.Email = supplier.Email;
                existingSupplier.Phone = supplier.Phone;
                existingSupplier.Address = supplier.Address;
                existingSupplier.City = supplier.City;
                existingSupplier.PostalCode = supplier.PostalCode;
                existingSupplier.Country = supplier.Country;
                existingSupplier.TaxNumber = supplier.TaxNumber;
                existingSupplier.BankAccount = supplier.BankAccount;
                existingSupplier.PaymentTerms = supplier.PaymentTerms;
                existingSupplier.Notes = supplier.Notes;
                existingSupplier.IsActive = supplier.IsActive;
                existingSupplier.ModifiedDate = DateTime.Now;

                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating supplier: {ex.Message}");
                return false;
            }
        }

        // ===== DELETE SUPPLIER (Soft Delete) =====
        public bool DeleteSupplier(int supplierId)
        {
            try
            {
                var supplier = _context.Suppliers?.FirstOrDefault(s => s.SupplierId == supplierId);
                if (supplier == null)
                    return false;

                supplier.IsActive = false;
                supplier.ModifiedDate = DateTime.Now;

                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting supplier: {ex.Message}");
                return false;
            }
        }

        // ===== HARD DELETE SUPPLIER =====
        public bool HardDeleteSupplier(int supplierId)
        {
            try
            {
                var supplier = _context.Suppliers?.FirstOrDefault(s => s.SupplierId == supplierId);
                if (supplier == null)
                    return false;

                _context.Suppliers?.Remove(supplier);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error hard deleting supplier: {ex.Message}");
                return false;
            }
        }

        // ===== SEARCH SUPPLIERS =====
        public List<Supplier> SearchSuppliers(string searchText, string? statusFilter = null, string? countryFilter = null)
        {
            try
            {
                if (_context.Suppliers == null)
                    return new List<Supplier>();

                var query = _context.Suppliers.AsQueryable();

                // Search by text
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    searchText = searchText.ToLower();
                    query = query.Where(s =>
                        s.SupplierName.ToLower().Contains(searchText) ||
                        s.ContactPerson.ToLower().Contains(searchText) ||
                        s.Email.ToLower().Contains(searchText) ||
                        s.Phone.Contains(searchText) ||
                        s.City.ToLower().Contains(searchText) ||
                        s.TaxNumber.Contains(searchText));
                }

                // Filter by status
                if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All Status")
                {
                    if (statusFilter == "Active")
                        query = query.Where(s => s.IsActive);
                    else if (statusFilter == "Inactive")
                        query = query.Where(s => !s.IsActive);
                }

                // Filter by country
                if (!string.IsNullOrWhiteSpace(countryFilter) && countryFilter != "All Countries")
                {
                    query = query.Where(s => s.Country == countryFilter);
                }

                return query.OrderBy(s => s.SupplierName).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching suppliers: {ex.Message}");
                return new List<Supplier>();
            }
        }

        // ===== GET KPI DATA =====
        public SupplierKpi GetKpi()
        {
            try
            {
                var allSuppliers = _context.Suppliers?.ToList() ?? new List<Supplier>();

                return new SupplierKpi
                {
                    TotalSuppliers = allSuppliers.Count,
                    ActiveSuppliers = allSuppliers.Count(s => s.IsActive),
                    InactiveSuppliers = allSuppliers.Count(s => !s.IsActive),
                    NewThisMonth = allSuppliers.Count(s => s.CreatedDate.Month == DateTime.Now.Month && s.CreatedDate.Year == DateTime.Now.Year)
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting KPI: {ex.Message}");
                return new SupplierKpi();
            }
        }

        // ===== GET UNIQUE COUNTRIES =====
        public List<string> GetUniqueCountries()
        {
            try
            {
                if (_context.Suppliers == null)
                    return new List<string>();

                return _context.Suppliers
                    .Where(s => !string.IsNullOrEmpty(s.Country))
                    .Select(s => s.Country)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting countries: {ex.Message}");
                return new List<string>();
            }
        }

        // ===== CHECK IF TAX NUMBER EXISTS =====
        public bool TaxNumberExists(string taxNumber, int? excludeSupplierId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(taxNumber))
                    return false;

                var query = _context.Suppliers?.Where(s => s.TaxNumber == taxNumber);

                if (excludeSupplierId.HasValue)
                    query = query?.Where(s => s.SupplierId != excludeSupplierId.Value);

                return query?.Any() ?? false;
            }
            catch
            {
                return false;
            }
        }
    }

    // ===== KPI DTO =====
    public class SupplierKpi
    {
        public int TotalSuppliers { get; set; }
        public int ActiveSuppliers { get; set; }
        public int InactiveSuppliers { get; set; }
        public int NewThisMonth { get; set; }
    }

    // ===== DISPLAY DTO =====
    public class SupplierDisplayDto
    {
        public int SupplierId { get; set; }
        public string SupplierCode { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public string ContactPerson { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
        public string TaxNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string PostalCode { get; set; } = "";
        public string BankAccount { get; set; } = "";
        public string PaymentTerms { get; set; } = "";
        public string Notes { get; set; } = "";
        public bool IsActive { get; set; }
        public string StatusText => IsActive ? "✓ Active" : "✗ Inactive";
        public string StatusColor => IsActive ? "#FF28A745" : "#FFDC3545";
        public string StatusBackground => IsActive ? "#FFD4EDDA" : "#FFF8D7DA";
        public DateTime CreatedDate { get; set; }

        public static SupplierDisplayDto FromSupplier(Supplier s)
        {
            return new SupplierDisplayDto
            {
                SupplierId = s.SupplierId,
                SupplierCode = $"SUP-{s.SupplierId:D3}",
                SupplierName = s.SupplierName ?? "",
                ContactPerson = s.ContactPerson ?? "",
                Phone = s.Phone ?? "",
                Email = s.Email ?? "",
                City = s.City ?? "",
                Country = s.Country ?? "",
                TaxNumber = s.TaxNumber ?? "",
                Address = s.Address ?? "",
                PostalCode = s.PostalCode ?? "",
                BankAccount = s.BankAccount ?? "",
                PaymentTerms = s.PaymentTerms ?? "",
                Notes = s.Notes ?? "",
                IsActive = s.IsActive,
                CreatedDate = s.CreatedDate
            };
        }
    }
}
