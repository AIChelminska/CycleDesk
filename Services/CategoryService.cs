using CycleDesk.Data;
using CycleDesk.Models;
using Microsoft.EntityFrameworkCore;

namespace CycleDesk.Services
{
    /// <summary>
    /// DTO dla wyświetlania kategorii z liczbą produktów
    /// </summary>
    public class CategoryDisplayDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; }
        public int ProductCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<CategoryDisplayDto> Subcategories { get; set; } = new List<CategoryDisplayDto>();
    }

    /// <summary>
    /// Serwis do obsługi kategorii używający Entity Framework Core
    /// </summary>
    public class CategoryService
    {
        // ===== CREATE =====
        public int AddCategory(Category category)
        {
            using (var context = new CycleDeskDbContext())
            {
                category.CreatedDate = DateTime.Now;
                category.IsActive = true;
                context.Categories.Add(category);
                context.SaveChanges();
                return category.CategoryId;
            }
        }

        // ===== READ =====
        public List<CategoryDisplayDto> GetAllCategoriesWithDetails()
        {
            using (var context = new CycleDeskDbContext())
            {
                // Pobierz wszystkie kategorie
                var allCategories = context.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.CategoryName)
                    .ToList();

                // Policz produkty dla każdej kategorii
                var productCounts = context.Products
                    .Where(p => p.IsActive)
                    .GroupBy(p => p.CategoryId)
                    .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.CategoryId, x => x.Count);

                // Pobierz główne kategorie (bez rodzica)
                var mainCategories = allCategories
                    .Where(c => c.ParentCategoryId == null)
                    .Select(c => new CategoryDisplayDto
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName,
                        Description = c.Description,
                        ParentCategoryId = c.ParentCategoryId,
                        ParentCategoryName = null,
                        ProductCount = productCounts.ContainsKey(c.CategoryId) ? productCounts[c.CategoryId] : 0,
                        IsActive = c.IsActive,
                        CreatedDate = c.CreatedDate,
                        Subcategories = new List<CategoryDisplayDto>()
                    })
                    .ToList();

                // Dodaj podkategorie do każdej głównej kategorii
                foreach (var mainCat in mainCategories)
                {
                    var subs = allCategories
                        .Where(c => c.ParentCategoryId == mainCat.CategoryId)
                        .Select(c => new CategoryDisplayDto
                        {
                            CategoryId = c.CategoryId,
                            CategoryName = c.CategoryName,
                            Description = c.Description,
                            ParentCategoryId = c.ParentCategoryId,
                            ParentCategoryName = mainCat.CategoryName,
                            ProductCount = productCounts.ContainsKey(c.CategoryId) ? productCounts[c.CategoryId] : 0,
                            IsActive = c.IsActive,
                            CreatedDate = c.CreatedDate
                        })
                        .ToList();

                    mainCat.Subcategories = subs;

                    // Dodaj produkty z podkategorii do sumy głównej kategorii
                    mainCat.ProductCount += subs.Sum(s => s.ProductCount);
                }

                return mainCategories;
            }
        }

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

        public List<Category> GetMainCategories()
        {
            using (var context = new CycleDeskDbContext())
            {
                return context.Categories
                    .Where(c => c.IsActive && c.ParentCategoryId == null)
                    .OrderBy(c => c.CategoryName)
                    .ToList();
            }
        }

        public List<Category> GetSubcategories(int parentCategoryId)
        {
            using (var context = new CycleDeskDbContext())
            {
                return context.Categories
                    .Where(c => c.IsActive && c.ParentCategoryId == parentCategoryId)
                    .OrderBy(c => c.CategoryName)
                    .ToList();
            }
        }

        public Category GetCategoryById(int categoryId)
        {
            using (var context = new CycleDeskDbContext())
            {
                return context.Categories.FirstOrDefault(c => c.CategoryId == categoryId);
            }
        }

        public CategoryDisplayDto GetCategoryWithDetails(int categoryId)
        {
            using (var context = new CycleDeskDbContext())
            {
                var category = context.Categories.FirstOrDefault(c => c.CategoryId == categoryId);
                if (category == null) return null;

                var productCount = context.Products.Count(p => p.CategoryId == categoryId && p.IsActive);

                string parentName = null;
                if (category.ParentCategoryId.HasValue)
                {
                    var parent = context.Categories.FirstOrDefault(c => c.CategoryId == category.ParentCategoryId);
                    parentName = parent?.CategoryName;
                }

                return new CategoryDisplayDto
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                    Description = category.Description,
                    ParentCategoryId = category.ParentCategoryId,
                    ParentCategoryName = parentName,
                    ProductCount = productCount,
                    IsActive = category.IsActive,
                    CreatedDate = category.CreatedDate
                };
            }
        }

        // ===== UPDATE =====
        public bool UpdateCategory(Category category)
        {
            using (var context = new CycleDeskDbContext())
            {
                var existingCategory = context.Categories.FirstOrDefault(c => c.CategoryId == category.CategoryId);
                if (existingCategory == null) return false;

                existingCategory.CategoryName = category.CategoryName;
                existingCategory.Description = category.Description;
                existingCategory.ParentCategoryId = category.ParentCategoryId;
                existingCategory.ModifiedDate = DateTime.Now;

                context.SaveChanges();
                return true;
            }
        }

        // ===== DELETE (Soft delete) =====
        public bool DeactivateCategory(int categoryId)
        {
            using (var context = new CycleDeskDbContext())
            {
                var category = context.Categories.FirstOrDefault(c => c.CategoryId == categoryId);
                if (category == null) return false;

                // Sprawdź czy kategoria ma produkty
                var hasProducts = context.Products.Any(p => p.CategoryId == categoryId && p.IsActive);
                if (hasProducts)
                {
                    throw new InvalidOperationException("Cannot delete category with active products. Move or delete products first.");
                }

                // Sprawdź czy kategoria ma podkategorie
                var hasSubcategories = context.Categories.Any(c => c.ParentCategoryId == categoryId && c.IsActive);
                if (hasSubcategories)
                {
                    throw new InvalidOperationException("Cannot delete category with subcategories. Delete subcategories first.");
                }

                category.IsActive = false;
                category.ModifiedDate = DateTime.Now;
                context.SaveChanges();
                return true;
            }
        }

        public bool DeleteCategory(int categoryId)
        {
            using (var context = new CycleDeskDbContext())
            {
                var category = context.Categories.FirstOrDefault(c => c.CategoryId == categoryId);
                if (category == null) return false;

                // Sprawdź czy kategoria ma produkty
                var hasProducts = context.Products.Any(p => p.CategoryId == categoryId);
                if (hasProducts)
                {
                    throw new InvalidOperationException("Cannot delete category with products.");
                }

                context.Categories.Remove(category);
                context.SaveChanges();
                return true;
            }
        }

        // ===== VALIDATION =====
        public bool IsCategoryNameAvailable(string categoryName, int? excludeCategoryId = null)
        {
            using (var context = new CycleDeskDbContext())
            {
                if (excludeCategoryId.HasValue)
                {
                    return !context.Categories.Any(c => 
                        c.CategoryName == categoryName && 
                        c.CategoryId != excludeCategoryId.Value &&
                        c.IsActive);
                }
                return !context.Categories.Any(c => c.CategoryName == categoryName && c.IsActive);
            }
        }

        // ===== STATISTICS =====
        public Dictionary<string, int> GetCategoryStatistics()
        {
            using (var context = new CycleDeskDbContext())
            {
                var categories = context.Categories.Where(c => c.IsActive).ToList();
                var mainCategories = categories.Count(c => c.ParentCategoryId == null);
                var subcategories = categories.Count(c => c.ParentCategoryId != null);

                return new Dictionary<string, int>
                {
                    { "Total", categories.Count },
                    { "MainCategories", mainCategories },
                    { "Subcategories", subcategories }
                };
            }
        }

        // ===== SEARCH =====
        public List<CategoryDisplayDto> SearchCategories(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllCategoriesWithDetails();

            var allCategories = GetAllCategoriesWithDetails();
            var term = searchTerm.ToLower();

            // Filtruj główne kategorie i ich podkategorie
            var result = new List<CategoryDisplayDto>();

            foreach (var mainCat in allCategories)
            {
                // Sprawdź czy główna kategoria pasuje
                bool mainMatches = mainCat.CategoryName.ToLower().Contains(term) ||
                                   (mainCat.Description != null && mainCat.Description.ToLower().Contains(term));

                // Filtruj podkategorie
                var matchingSubcats = mainCat.Subcategories
                    .Where(s => s.CategoryName.ToLower().Contains(term) ||
                               (s.Description != null && s.Description.ToLower().Contains(term)))
                    .ToList();

                if (mainMatches || matchingSubcats.Any())
                {
                    var categoryToAdd = new CategoryDisplayDto
                    {
                        CategoryId = mainCat.CategoryId,
                        CategoryName = mainCat.CategoryName,
                        Description = mainCat.Description,
                        ParentCategoryId = mainCat.ParentCategoryId,
                        ProductCount = mainCat.ProductCount,
                        IsActive = mainCat.IsActive,
                        CreatedDate = mainCat.CreatedDate,
                        Subcategories = mainMatches ? mainCat.Subcategories : matchingSubcats
                    };
                    result.Add(categoryToAdd);
                }
            }

            return result;
        }
    }
}
