using eShopLegacyMVC.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace eShopLegacyMVC.Models.Infrastructure
{
    public class CatalogDBInitializer
    {
        private const string DBCatalogSequenceName = "catalog_type_hilo";
        private const string DBBrandSequenceName = "catalog_brand_hilo";
        private const string CatalogItemHiLoSequenceScript = "Models/Infrastructure/dbo.catalog_hilo.Sequence.sql";
        private const string CatalogBrandHiLoSequenceScript = "Models/Infrastructure/dbo.catalog_brand_hilo.Sequence.sql";
        private const string CatalogTypeHiLoSequenceScript = "Models/Infrastructure/dbo.catalog_type_hilo.Sequence.sql";

        private readonly CatalogItemHiLoGenerator _indexGenerator;
        private readonly bool _useCustomizationData;
        private readonly string _contentRootPath;

        public CatalogDBInitializer(CatalogItemHiLoGenerator indexGenerator, IWebHostEnvironment env, IConfiguration configuration)
        {
            _indexGenerator = indexGenerator;
            _contentRootPath = env.ContentRootPath;
            _useCustomizationData = configuration.GetValue<bool>("UseCustomizationData");
        }

        public void Seed(CatalogDBContext context)
        {
            if (context.CatalogTypes.Any())
                return; // Already seeded

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
            var preconfiguredTypes = _useCustomizationData
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
            var preconfiguredBrands = _useCustomizationData
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
            var preconfiguredItems = _useCustomizationData
                ? GetCatalogItemsFromFile(context)
                : PreconfiguredData.GetPreconfiguredCatalogItems();

            foreach (var item in preconfiguredItems)
            {
                var sequenceId = _indexGenerator.GetNextSequenceValue(context);
                item.Id = sequenceId;
                context.CatalogItems.Add(item);
            }
            context.SaveChanges();
        }

        private IEnumerable<CatalogType> GetCatalogTypesFromFile()
        {
            string csvFileCatalogTypes = Path.Combine(_contentRootPath, "Setup", "CatalogTypes.csv");
            if (!File.Exists(csvFileCatalogTypes))
                return PreconfiguredData.GetPreconfiguredCatalogTypes();

            string[] requiredHeaders = { "catalogtype" };
            var csvheaders = GetHeaders(csvFileCatalogTypes, requiredHeaders);

            return File.ReadAllLines(csvFileCatalogTypes)
                .Skip(1)
                .Select(x => CreateCatalogType(x))
                .Where(x => x != null)!;
        }

        static CatalogType? CreateCatalogType(string type)
        {
            type = type.Trim('"').Trim();
            if (string.IsNullOrEmpty(type))
                throw new Exception("catalog Type Name is empty");

            return new CatalogType { Type = type };
        }

        static IEnumerable<CatalogBrand> GetCatalogBrandsFromFile()
        {
            // Uses instance method logic - placeholder for file loading
            return PreconfiguredData.GetPreconfiguredCatalogBrands();
        }

        IEnumerable<CatalogBrand> GetCatalogBrandsFromFile(string contentRootPath)
        {
            string csvFileCatalogBrands = Path.Combine(contentRootPath, "Setup", "CatalogBrands.csv");
            if (!File.Exists(csvFileCatalogBrands))
                return PreconfiguredData.GetPreconfiguredCatalogBrands();

            string[] requiredHeaders = { "catalogbrand" };
            GetHeaders(csvFileCatalogBrands, requiredHeaders);

            return File.ReadAllLines(csvFileCatalogBrands)
                .Skip(1)
                .Select(x => CreateCatalogBrand(x))
                .Where(x => x != null)!;
        }

        static CatalogBrand? CreateCatalogBrand(string brand)
        {
            brand = brand.Trim('"').Trim();
            if (string.IsNullOrEmpty(brand))
                throw new Exception("catalog Brand Name is empty");

            return new CatalogBrand { Brand = brand };
        }

        static IEnumerable<CatalogItem> GetCatalogItemsFromFile(CatalogDBContext context)
        {
            return PreconfiguredData.GetPreconfiguredCatalogItems();
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
            // dBSequenceName is always a compile-time constant; suppress EF1002 SQL injection warning
#pragma warning disable EF1002
            var result = context.Database.SqlQueryRaw<long>($"SELECT NEXT VALUE FOR {dBSequenceName}").ToList();
#pragma warning restore EF1002
            return (int)result[0];
        }

        private void ExecuteScript(CatalogDBContext context, string scriptFile)
        {
            var scriptFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, scriptFile);
            if (File.Exists(scriptFilePath))
            {
                context.Database.ExecuteSqlRaw(File.ReadAllText(scriptFilePath));
            }
        }

        private void AddCatalogItemPictures()
        {
            if (!_useCustomizationData)
                return;

            DirectoryInfo picturePath = new DirectoryInfo(Path.Combine(_contentRootPath, "Pics"));
            foreach (FileInfo file in picturePath.GetFiles())
                file.Delete();

            string zipFileCatalogItemPictures = Path.Combine(_contentRootPath, "Setup", "CatalogItems.zip");
            if (File.Exists(zipFileCatalogItemPictures))
            {
                ZipFile.ExtractToDirectory(zipFileCatalogItemPictures, picturePath.ToString());
            }
        }
    }
}
