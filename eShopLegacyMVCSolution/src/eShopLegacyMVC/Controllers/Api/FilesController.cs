using eShopLegacy.Utilities;
using eShopPorted.Services;
using Microsoft.AspNetCore.Mvc;

namespace eShopPorted.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly ICatalogService _service;

    public FilesController(ICatalogService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var brands = _service.GetCatalogBrands()
            .Select(brand => new BrandDto(brand.Id, brand.Brand))
            .ToList();

        var serializer = new Serializing();
        var data = serializer.SerializeBinary(brands);
        return File(data, "application/json", "brands.json");
    }

    public sealed record BrandDto(int Id, string Brand);
}
