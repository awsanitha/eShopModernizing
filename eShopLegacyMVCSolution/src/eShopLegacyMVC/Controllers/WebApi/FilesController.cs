using eShopLegacy.Utilities;
using eShopLegacyMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace eShopLegacyMVC.Controllers.WebApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly ICatalogService _service;

        public FilesController(ICatalogService service)
        {
            _service = service;
        }

        // GET api/files
        [HttpGet]
        public IActionResult Get()
        {
            var brands = _service.GetCatalogBrands()
                .Select(b => new BrandDTO
                {
                    Id = b.Id,
                    Brand = b.Brand
                }).ToList();

            var serializer = new Serializing();
            var stream = serializer.SerializeBinary(brands);
            return File(stream, "application/octet-stream");
        }

        [Serializable]
        [System.Runtime.Serialization.DataContract]
        public class BrandDTO
        {
            [System.Runtime.Serialization.DataMember]
            public int Id { get; set; }
            [System.Runtime.Serialization.DataMember]
            public string Brand { get; set; } = string.Empty;
        }
    }
}
