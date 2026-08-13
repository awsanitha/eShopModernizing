using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace eShopLegacyWebForms.Models.Infrastructure
{
    public class CatalogDBInitializer
    {
        private const string DBCatalogSequenceName = "catalog_type_hilo";
        private const string DBBrandSequenceName = "catalog_brand_hilo";
        private const string CatalogItemHiLoSequenceScript = "Models/Infrastructure/dbo.catalog_hilo.Sequence.sql";
        private const string CatalogBrandHiLoSequenceScript = "Models/Infrastructure/dbo.catalog_brand_hilo.Sequence.sql";
        private const string CatalogTypeHiLoSequenceScript = "Models/Infrastructure/dbo.catalog_type_hilo.Sequence.sql";

        private readonly CatalogItemHiLoGenerator indexGenerator;
        private readonly bool useCustomizationData;
        private readonly string contentRootPath;

        public CatalogDBInitializer(CatalogItemHiLoGenerator indexGenerator, string contentRootPath, bool useCustomizationData)
        {
            this.indexGenerator = indexGenerator;
            this.contentRootPath = contentRootPath;
            this.useCustomizationData = useCustomizationData;
        }

        public void Seed(CatalogDBContext context)
        {
            context.Database.EnsureCreated();
            ExecuteScript(context, CatalogItemHiLoSequenceScript);
            ExecuteScript(context, CatalogBrandHiLoSequenceScript);
            ExecuteScript(context, CatalogTypeHiLoSequenceScript);
            AddCatalogTypes(context);
            AddCatalogBrands(context);
            AddCatalogItems(context);
        }

        private void AddCatalogTypes(CatalogDBContext context)
        {
            if (context.CatalogTypes.Any()) return;
            var types = useCustomizationData ? GetCatalogTypesFromFile() : PreconfiguredData.GetPreconfiguredCatalogTypes();
            int seqId = GetSequenceIdFromDB(context, DBCatalogSequenceName);
            foreach (var t in types) { t.Id = seqId++; context.CatalogTypes.Add(t); }
            context.SaveChanges();
        }

        private void AddCatalogBrands(CatalogDBContext context)
        {
            if (context.CatalogBrands.Any()) return;
            var brands = useCustomizationData ? GetCatalogBrandsFromFile() : PreconfiguredData.GetPreconfiguredCatalogBrands();
            int seqId = GetSequenceIdFromDB(context, DBBrandSequenceName);
            foreach (var b in brands) { b.Id = seqId++; context.CatalogBrands.Add(b); }
            context.SaveChanges();
        }

        private void AddCatalogItems(CatalogDBContext context)
        {
            if (context.CatalogItems.Any()) return;
            var items = useCustomizationData ? GetCatalogItemsFromFile(context) : PreconfiguredData.GetPreconfiguredCatalogItems();
            foreach (var item in items)
            {
                item.Id = indexGenerator.GetNextSequenceValue(context);
                context.CatalogItems.Add(item);
            }
            context.SaveChanges();
        }

        private IEnumerable<CatalogType> GetCatalogTypesFromFile()
        {
            string path = Path.Combine(contentRootPath, "Setup", "CatalogTypes.csv");
            if (!File.Exists(path)) return PreconfiguredData.GetPreconfiguredCatalogTypes();
            return File.ReadAllLines(path).Skip(1).Select(x => { x = x.Trim('"').Trim(); return string.IsNullOrEmpty(x) ? null : new CatalogType { Type = x }; }).Where(x => x != null);
        }

        private IEnumerable<CatalogBrand> GetCatalogBrandsFromFile()
        {
            string path = Path.Combine(contentRootPath, "Setup", "CatalogBrands.csv");
            if (!File.Exists(path)) return PreconfiguredData.GetPreconfiguredCatalogBrands();
            return File.ReadAllLines(path).Skip(1).Select(x => { x = x.Trim('"').Trim(); return string.IsNullOrEmpty(x) ? null : new CatalogBrand { Brand = x }; }).Where(x => x != null);
        }

        private IEnumerable<CatalogItem> GetCatalogItemsFromFile(CatalogDBContext context)
        {
            string path = Path.Combine(contentRootPath, "Setup", "CatalogItems.csv");
            if (!File.Exists(path)) return PreconfiguredData.GetPreconfiguredCatalogItems();

            string[] required = { "catalogtypename", "catalogbrandname", "description", "name", "price", "pictureFileName" };
            string[] optional = { "availablestock", "restockthreshold", "maxstockthreshold", "onreorder" };
            var headers = GetHeaders(path, required, optional);
            var typeIdMap = context.CatalogTypes.ToDictionary(ct => ct.Type, ct => ct.Id);
            var brandIdMap = context.CatalogBrands.ToDictionary(ct => ct.Brand, ct => ct.Id);

            return File.ReadAllLines(path).Skip(1)
                .Select(row => Regex.Split(row, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"))
                .Select(cols => BuildItem(cols, headers, typeIdMap, brandIdMap))
                .Where(x => x != null);
        }

        private static CatalogItem BuildItem(string[] cols, string[] headers, Dictionary<string, int> typeMap, Dictionary<string, int> brandMap)
        {
            string typeName = cols[Array.IndexOf(headers, "catalogtypename")].Trim('"').Trim();
            string brandName = cols[Array.IndexOf(headers, "catalogbrandname")].Trim('"').Trim();
            string priceStr = cols[Array.IndexOf(headers, "price")].Trim('"').Trim();
            if (!decimal.TryParse(priceStr, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal price)) return null;

            var item = new CatalogItem
            {
                CatalogTypeId = typeMap.TryGetValue(typeName, out int tid) ? tid : 0,
                CatalogBrandId = brandMap.TryGetValue(brandName, out int bid) ? bid : 0,
                Description = cols[Array.IndexOf(headers, "description")].Trim('"').Trim(),
                Name = cols[Array.IndexOf(headers, "name")].Trim('"').Trim(),
                Price = price,
                PictureFileName = cols[Array.IndexOf(headers, "picturefilename")].Trim('"').Trim(),
            };

            int idx;
            if ((idx = Array.IndexOf(headers, "availablestock")) != -1 && int.TryParse(cols[idx].Trim('"').Trim(), out int av)) item.AvailableStock = av;
            if ((idx = Array.IndexOf(headers, "restockthreshold")) != -1 && int.TryParse(cols[idx].Trim('"').Trim(), out int rt)) item.RestockThreshold = rt;
            if ((idx = Array.IndexOf(headers, "maxstockthreshold")) != -1 && int.TryParse(cols[idx].Trim('"').Trim(), out int ms)) item.MaxStockThreshold = ms;
            if ((idx = Array.IndexOf(headers, "onreorder")) != -1 && bool.TryParse(cols[idx].Trim('"').Trim(), out bool or)) item.OnReorder = or;
            return item;
        }

        private static string[] GetHeaders(string file, string[] required, string[] optional = null)
        {
            string[] headers = File.ReadLines(file).First().ToLowerInvariant().Split(',');
            foreach (var h in required) if (!headers.Contains(h.ToLowerInvariant())) throw new Exception($"Missing header: {h}");
            return headers;
        }

        private static int GetSequenceIdFromDB(CatalogDBContext context, string seqName)
        {
            var result = context.Database.SqlQueryRaw<long>($"SELECT NEXT VALUE FOR {seqName}").ToList();
            return (int)result.Single();
        }

        private void ExecuteScript(CatalogDBContext context, string scriptFile)
        {
            string path = Path.Combine(contentRootPath, scriptFile);
            if (File.Exists(path)) context.Database.ExecuteSqlRaw(File.ReadAllText(path));
        }
    }
}
