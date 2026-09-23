using Microsoft.AspNetCore.Hosting;
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
    public class CatalogDBInitializer
    {
        private const string DBCatalogSequenceName = "catalog_type_hilo";
        private const string DBBrandSequenceName = "catalog_brand_hilo";
        private const string CatalogItemHiLoSequenceScript = @"Models/Infrastructure/dbo.catalog_hilo.Sequence.sql";
        private const string CatalogBrandHiLoSequenceScript = @"Models/Infrastructure/dbo.catalog_brand_hilo.Sequence.sql";
        private const string CatalogTypeHiLoSequenceScript = @"Models/Infrastructure/dbo.catalog_type_hilo.Sequence.sql";

        private readonly CatalogItemHiLoGenerator indexGenerator;
        private readonly bool useCustomizationData;
        private readonly IWebHostEnvironment env;

        public CatalogDBInitializer(CatalogItemHiLoGenerator indexGenerator, IWebHostEnvironment env, IConfiguration configuration)
        {
            this.indexGenerator = indexGenerator;
            this.env = env;
            useCustomizationData = bool.Parse(configuration["UseCustomizationData"] ?? "false");
        }

        public void Seed(CatalogDBContext context)
        {
            if (context.CatalogItems.Any())
                return;

            ExecuteScript(context, CatalogItemHiLoSequenceScript);
            ExecuteScript(context, CatalogBrandHiLoSequenceScript);
            ExecuteScript(context, CatalogTypeHiLoSequenceScript);

            AddCatalogTypes(context);
            AddCatalogBrands(context);
            AddCatalogItems(context);
            AddCatalogItemPictures();
        }

        private void AddCatalogTypes(CatalogDBContext context)
        {
            var preconfiguredTypes = useCustomizationData
                ? GetCatalogTypesFromFile()
                : PreconfiguredData.GetPreconfiguredCatalogTypes();

            int sequenceId = GetSequenceIdFromSelectedDBSequence(context, DBCatalogSequenceName);
            foreach (var type in preconfiguredTypes)
            {
                type.Id = sequenceId;
                context.CatalogTypes.Add(type);
                sequenceId++;
            }
            context.SaveChanges();
        }

        private void AddCatalogBrands(CatalogDBContext context)
        {
            var preconfiguredBrands = useCustomizationData
                ? GetCatalogBrandsFromFile()
                : PreconfiguredData.GetPreconfiguredCatalogBrands();

            int sequenceId = GetSequenceIdFromSelectedDBSequence(context, DBBrandSequenceName);
            foreach (var brand in preconfiguredBrands)
            {
                brand.Id = sequenceId;
                context.CatalogBrands.Add(brand);
                sequenceId++;
            }
            context.SaveChanges();
        }

        private void AddCatalogItems(CatalogDBContext context)
        {
            var preconfiguredItems = useCustomizationData
                ? GetCatalogItemsFromFile(context)
                : PreconfiguredData.GetPreconfiguredCatalogItems();

            foreach (var item in preconfiguredItems)
            {
                var sequenceId = indexGenerator.GetNextSequenceValue(context);
                item.Id = sequenceId;
                context.CatalogItems.Add(item);
            }
            context.SaveChanges();
        }

        private IEnumerable<CatalogType> GetCatalogTypesFromFile()
        {
            var contentRootPath = env.ContentRootPath;
            string csvFileCatalogTypes = Path.Combine(contentRootPath, "Setup", "CatalogTypes.csv");

            if (!File.Exists(csvFileCatalogTypes))
            {
                return PreconfiguredData.GetPreconfiguredCatalogTypes();
            }

            string[] requiredHeaders = { "catalogtype" };
            string[] csvheaders = GetHeaders(csvFileCatalogTypes, requiredHeaders);

            return File.ReadAllLines(csvFileCatalogTypes)
                .Skip(1)
                .Select(x => CreateCatalogType(x))
                .Where(x => x != null)!;
        }

        static CatalogType CreateCatalogType(string type)
        {
            type = type.Trim('"').Trim();
            if (string.IsNullOrEmpty(type))
                throw new Exception("catalog Type Name is empty");
            return new CatalogType { Type = type };
        }

        static IEnumerable<CatalogBrand> GetCatalogBrandsFromFile(string contentRootPath)
        {
            string csvFileCatalogBrands = Path.Combine(contentRootPath, "Setup", "CatalogBrands.csv");

            if (!File.Exists(csvFileCatalogBrands))
            {
                return PreconfiguredData.GetPreconfiguredCatalogBrands();
            }

            string[] requiredHeaders = { "catalogbrand" };
            GetHeaders(csvFileCatalogBrands, requiredHeaders);

            return File.ReadAllLines(csvFileCatalogBrands)
                .Skip(1)
                .Select(x => CreateCatalogBrand(x))
                .Where(x => x != null)!;
        }

        private IEnumerable<CatalogBrand> GetCatalogBrandsFromFile()
        {
            return GetCatalogBrandsFromFile(env.ContentRootPath);
        }

        static CatalogBrand CreateCatalogBrand(string brand)
        {
            brand = brand.Trim('"').Trim();
            if (string.IsNullOrEmpty(brand))
                throw new Exception("catalog Brand Name is empty");
            return new CatalogBrand { Brand = brand };
        }

        static IEnumerable<CatalogItem> GetCatalogItemsFromFile(CatalogDBContext context, string contentRootPath)
        {
            string csvFileCatalogItems = Path.Combine(contentRootPath, "Setup", "CatalogItems.csv");

            if (!File.Exists(csvFileCatalogItems))
            {
                return PreconfiguredData.GetPreconfiguredCatalogItems();
            }

            string[] requiredHeaders = { "catalogtypename", "catalogbrandname", "description", "name", "price", "pictureFileName" };
            string[] optionalheaders = { "availablestock", "restockthreshold", "maxstockthreshold", "onreorder" };
            string[] csvheaders = GetHeaders(csvFileCatalogItems, requiredHeaders, optionalheaders);

            var catalogTypeIdLookup = context.CatalogTypes.ToDictionary(ct => ct.Type, ct => ct.Id);
            var catalogBrandIdLookup = context.CatalogBrands.ToDictionary(ct => ct.Brand, ct => ct.Id);

            return File.ReadAllLines(csvFileCatalogItems)
                .Skip(1)
                .Select(row => Regex.Split(row, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"))
                .Select(column => CreateCatalogItem(column, csvheaders, catalogTypeIdLookup, catalogBrandIdLookup))
                .Where(x => x != null)!;
        }

        private IEnumerable<CatalogItem> GetCatalogItemsFromFile(CatalogDBContext context)
        {
            return GetCatalogItemsFromFile(context, env.ContentRootPath);
        }

        static CatalogItem? CreateCatalogItem(string[] column, string[] headers, Dictionary<string, int> catalogTypeIdLookup, Dictionary<string, int> catalogBrandIdLookup)
        {
            if (column.Length != headers.Length)
                throw new Exception($"column count '{column.Length}' not the same as headers count '{headers.Length}'");

            string catalogTypeName = column[Array.IndexOf(headers, "catalogtypename")].Trim('"').Trim();
            if (!catalogTypeIdLookup.ContainsKey(catalogTypeName))
                throw new Exception($"type={catalogTypeName} does not exist in catalogTypes");

            string catalogBrandName = column[Array.IndexOf(headers, "catalogbrandname")].Trim('"').Trim();
            if (!catalogBrandIdLookup.ContainsKey(catalogBrandName))
                throw new Exception($"brand={catalogBrandName} does not exist in catalogBrands");

            string priceString = column[Array.IndexOf(headers, "price")].Trim('"').Trim();
            if (!decimal.TryParse(priceString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal price))
                throw new Exception($"price={priceString} is not a valid decimal number");

            var catalogItem = new CatalogItem()
            {
                CatalogTypeId = catalogTypeIdLookup[catalogTypeName],
                CatalogBrandId = catalogBrandIdLookup[catalogBrandName],
                Description = column[Array.IndexOf(headers, "description")].Trim('"').Trim(),
                Name = column[Array.IndexOf(headers, "name")].Trim('"').Trim(),
                Price = price,
                PictureFileName = column[Array.IndexOf(headers, "picturefilename")].Trim('"').Trim(),
            };

            int availableStockIndex = Array.IndexOf(headers, "availablestock");
            if (availableStockIndex != -1)
            {
                string val = column[availableStockIndex].Trim('"').Trim();
                if (!string.IsNullOrEmpty(val))
                {
                    if (int.TryParse(val, out int availableStock))
                        catalogItem.AvailableStock = availableStock;
                    else
                        throw new Exception($"availableStock={val} is not a valid integer");
                }
            }

            int restockThresholdIndex = Array.IndexOf(headers, "restockthreshold");
            if (restockThresholdIndex != -1)
            {
                string val = column[restockThresholdIndex].Trim('"').Trim();
                if (!string.IsNullOrEmpty(val))
                {
                    if (int.TryParse(val, out int restockThreshold))
                        catalogItem.RestockThreshold = restockThreshold;
                    else
                        throw new Exception($"restockThreshold={val} is not a valid integer");
                }
            }

            int maxStockThresholdIndex = Array.IndexOf(headers, "maxstockthreshold");
            if (maxStockThresholdIndex != -1)
            {
                string val = column[maxStockThresholdIndex].Trim('"').Trim();
                if (!string.IsNullOrEmpty(val))
                {
                    if (int.TryParse(val, out int maxStockThreshold))
                        catalogItem.MaxStockThreshold = maxStockThreshold;
                    else
                        throw new Exception($"maxStockThreshold={val} is not a valid integer");
                }
            }

            int onReorderIndex = Array.IndexOf(headers, "onreorder");
            if (onReorderIndex != -1)
            {
                string val = column[onReorderIndex].Trim('"').Trim();
                if (!string.IsNullOrEmpty(val))
                {
                    if (bool.TryParse(val, out bool onReorder))
                        catalogItem.OnReorder = onReorder;
                    else
                        throw new Exception($"onReorder={val} is not a valid boolean");
                }
            }

            return catalogItem;
        }

        static string[] GetHeaders(string csvfile, string[] requiredHeaders, string[]? optionalHeaders = null)
        {
            string[] csvheaders = File.ReadLines(csvfile).First().ToLowerInvariant().Split(',');

            if (csvheaders.Length < requiredHeaders.Length)
                throw new Exception($"requiredHeader count '{requiredHeaders.Length}' is bigger than csv header count '{csvheaders.Length}'");

            if (optionalHeaders != null && csvheaders.Length > (requiredHeaders.Length + optionalHeaders.Length))
                throw new Exception($"csv header count '{csvheaders.Length}' is larger than required '{requiredHeaders.Length}' and optional '{optionalHeaders.Length}' headers count");

            foreach (var requiredHeader in requiredHeaders)
            {
                if (!csvheaders.Contains(requiredHeader.ToLowerInvariant()))
                    throw new Exception($"does not contain required header '{requiredHeader}'");
            }

            return csvheaders;
        }

        private static int GetSequenceIdFromSelectedDBSequence(CatalogDBContext context, string dBSequenceName)
        {
#pragma warning disable EF1003
            var sequenceId = context.Database.SqlQueryRaw<long>("SELECT NEXT VALUE FOR " + dBSequenceName).Single();
#pragma warning restore EF1003
            return (int)sequenceId;
        }

        private void ExecuteScript(CatalogDBContext context, string scriptFile)
        {
            var scriptFilePath = Path.Combine(AppContext.BaseDirectory, scriptFile);
            context.Database.ExecuteSqlRaw(File.ReadAllText(scriptFilePath));
        }

        private void AddCatalogItemPictures()
        {
            if (!useCustomizationData)
                return;

            var contentRootPath = env.ContentRootPath;
            DirectoryInfo picturePath = new DirectoryInfo(Path.Combine(contentRootPath, "Pics"));
            foreach (FileInfo file in picturePath.GetFiles())
            {
                file.Delete();
            }

            string zipFileCatalogItemPictures = Path.Combine(contentRootPath, "Setup", "CatalogItems.zip");
            ZipFile.ExtractToDirectory(zipFileCatalogItemPictures, picturePath.ToString());
        }
    }
}
