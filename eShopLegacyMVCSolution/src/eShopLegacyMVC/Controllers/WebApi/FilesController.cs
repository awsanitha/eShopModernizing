using eShopLegacyMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace eShopLegacyMVC.Controllers.WebApi
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly ICatalogService _service;

        public FilesController(ICatalogService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var brands = _service.GetCatalogBrands()
                .Select(b => new BrandDTO { Id = b.Id, Brand = b.Brand })
                .ToList();
            return Ok(brands);
        }

        private class BrandDTO
        {
            public int Id { get; set; }
            public string? Brand { get; set; }
        }
    }
}
