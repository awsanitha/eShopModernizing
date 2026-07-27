using eShopLegacyMVC.Models.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace eShopLegacyMVC.Models.Infrastructure
{
    /// <summary>
    /// Handles EF Core database seeding (replaces EF6 CatalogDBInitializer / CreateDatabaseIfNotExists pattern).
    /// </summary>
    public class CatalogDBInitializer
    {
        private readonly CatalogItemHiLoGenerator indexGenerator;
        private readonly bool useCustomizationData;
        private readonly string contentRootPath;

        public CatalogDBInitializer(
            CatalogItemHiLoGenerator indexGenerator,
            IConfiguration configuration,
            string contentRootPath)
        {
            this.indexGenerator = indexGenerator;
            this.useCustomizationData = configuration.GetValue<bool>("UseCustomizationData");
            this.contentRootPath = contentRootPath;
        }

        public void Seed(CatalogDBContext context)
        {
            context.Database.EnsureCreated();

            if (context.CatalogItems.Any())
                return; // already seeded

            AddCatalogTypes(context);
            AddCatalogBrands(context);
            AddCatalogItems(context);
        }

        private void AddCatalogTypes(CatalogDBContext context)
        {
            var types = useCustomizationData
                ? GetCatalogTypesFromFile()
                : PreconfiguredData.GetPreconfiguredCatalogTypes();

            int id = 1;
            foreach (var type in types)
            {
                type.Id = id++;
                context.CatalogTypes.Add(type);
            }
            context.SaveChanges();
        }

        private void AddCatalogBrands(CatalogDBContext context)
        {
            var brands = useCustomizationData
                ? GetCatalogBrandsFromFile()
                : PreconfiguredData.GetPreconfiguredCatalogBrands();

            int id = 1;
            foreach (var brand in brands)
            {
                brand.Id = id++;
                context.CatalogBrands.Add(brand);
            }
            context.SaveChanges();
        }

        private void AddCatalogItems(CatalogDBContext context)
        {
            var items = useCustomizationData
                ? GetCatalogItemsFromFile(context)
                : PreconfiguredData.GetPreconfiguredCatalogItems();

            foreach (var item in items)
            {
                item.Id = indexGenerator.GetNextSequenceValue(context);
                context.CatalogItems.Add(item);
            }
            context.SaveChanges();
        }

        private IEnumerable<CatalogType> GetCatalogTypesFromFile()
        {
            string csvFile = Path.Combine(contentRootPath, "Setup", "CatalogTypes.csv");
            if (!File.Exists(csvFile))
                return PreconfiguredData.GetPreconfiguredCatalogTypes();

            return File.ReadAllLines(csvFile)
                .Skip(1)
                .Select(x => CreateCatalogType(x))
                .Where(x => x != null)!;
        }

        private static CatalogType? CreateCatalogType(string type)
        {
            type = type.Trim('"').Trim();
            if (string.IsNullOrEmpty(type)) return null;
            return new CatalogType { Type = type };
        }

        private IEnumerable<CatalogBrand> GetCatalogBrandsFromFile()
        {
            string csvFile = Path.Combine(contentRootPath, "Setup", "CatalogBrands.csv");
            if (!File.Exists(csvFile))
                return PreconfiguredData.GetPreconfiguredCatalogBrands();

            return File.ReadAllLines(csvFile)
                .Skip(1)
                .Select(x => CreateCatalogBrand(x))
                .Where(x => x != null)!;
        }

        private static CatalogBrand? CreateCatalogBrand(string brand)
        {
            brand = brand.Trim('"').Trim();
            if (string.IsNullOrEmpty(brand)) return null;
            return new CatalogBrand { Brand = brand };
        }

        private IEnumerable<CatalogItem> GetCatalogItemsFromFile(CatalogDBContext context)
        {
            string csvFile = Path.Combine(contentRootPath, "Setup", "CatalogItems.csv");
            if (!File.Exists(csvFile))
                return PreconfiguredData.GetPreconfiguredCatalogItems();

            string[] requiredHeaders = { "catalogtypename", "catalogbrandname", "description", "name", "price", "picturefilename" };
            string[] optionalHeaders = { "availablestock", "restockthreshold", "maxstockthreshold", "onreorder" };
            var headers = GetHeaders(csvFile, requiredHeaders, optionalHeaders);

            var catalogTypeIdLookup = context.CatalogTypes.ToDictionary(ct => ct.Type!, ct => ct.Id);
            var catalogBrandIdLookup = context.CatalogBrands.ToDictionary(ct => ct.Brand!, ct => ct.Id);

            return File.ReadAllLines(csvFile)
                .Skip(1)
                .Select(row => Regex.Split(row, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"))
                .Select(cols => CreateCatalogItem(cols, headers, catalogTypeIdLookup, catalogBrandIdLookup))
                .Where(x => x != null)!;
        }

        private static CatalogItem? CreateCatalogItem(
            string[] column, string[] headers,
            Dictionary<string, int> catalogTypeIdLookup,
            Dictionary<string, int> catalogBrandIdLookup)
        {
            if (column.Length != headers.Length) return null;

            string typeName = column[Array.IndexOf(headers, "catalogtypename")].Trim('"').Trim();
            if (!catalogTypeIdLookup.TryGetValue(typeName, out int typeId)) return null;

            string brandName = column[Array.IndexOf(headers, "catalogbrandname")].Trim('"').Trim();
            if (!catalogBrandIdLookup.TryGetValue(brandName, out int brandId)) return null;

            string priceStr = column[Array.IndexOf(headers, "price")].Trim('"').Trim();
            if (!decimal.TryParse(priceStr, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal price))
                return null;

            var item = new CatalogItem
            {
                CatalogTypeId = typeId,
                CatalogBrandId = brandId,
                Description = column[Array.IndexOf(headers, "description")].Trim('"').Trim(),
                Name = column[Array.IndexOf(headers, "name")].Trim('"').Trim(),
                Price = price,
                PictureFileName = column[Array.IndexOf(headers, "picturefilename")].Trim('"').Trim()
            };

            int stockIdx = Array.IndexOf(headers, "availablestock");
            if (stockIdx != -1 && int.TryParse(column[stockIdx].Trim('"').Trim(), out int stock))
                item.AvailableStock = stock;

            int restockIdx = Array.IndexOf(headers, "restockthreshold");
            if (restockIdx != -1 && int.TryParse(column[restockIdx].Trim('"').Trim(), out int restock))
                item.RestockThreshold = restock;

            int maxStockIdx = Array.IndexOf(headers, "maxstockthreshold");
            if (maxStockIdx != -1 && int.TryParse(column[maxStockIdx].Trim('"').Trim(), out int maxStock))
                item.MaxStockThreshold = maxStock;

            int reorderIdx = Array.IndexOf(headers, "onreorder");
            if (reorderIdx != -1 && bool.TryParse(column[reorderIdx].Trim('"').Trim(), out bool onReorder))
                item.OnReorder = onReorder;

            return item;
        }

        private static string[] GetHeaders(string csvFile, string[] requiredHeaders, string[]? optionalHeaders = null)
        {
            var csvHeaders = File.ReadLines(csvFile).First().ToLowerInvariant().Split(',');
            foreach (var required in requiredHeaders)
            {
                if (!csvHeaders.Contains(required.ToLowerInvariant()))
                    throw new Exception($"CSV missing required header '{required}'");
            }
            return csvHeaders;
        }
    }
}
