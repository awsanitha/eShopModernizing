using eShopLegacyMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace eShopLegacyMVC.Controllers.WebApi
{
    [Route("api/brands")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly ICatalogService _service;

        public BrandsController(ICatalogService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_service.GetCatalogBrands());
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var brand = _service.GetCatalogBrands().FirstOrDefault(b => b.Id == id);
            if (brand == null) return NotFound();
            return Ok(brand);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return Ok();
        }
    }
}
