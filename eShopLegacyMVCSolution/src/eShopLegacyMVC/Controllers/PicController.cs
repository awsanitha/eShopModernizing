using eShopPorted.Services;
using Microsoft.AspNetCore.Mvc;

namespace eShopPorted.Controllers;

public class PicController : Controller
{
    public const string GetPicRouteName = "GetPicRouteTemplate";

    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<PicController> _logger;
    private readonly ICatalogService _service;

    public PicController(ICatalogService service, IWebHostEnvironment environment, ILogger<PicController> logger)
    {
        _service = service;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet]
    [Route("items/{catalogItemId:int}/pic", Name = GetPicRouteName)]
    public IActionResult Index(int catalogItemId)
    {
        if (catalogItemId <= 0)
        {
            return BadRequest();
        }

        var item = _service.FindCatalogItem(catalogItemId);
        if (item is null)
        {
            return NotFound();
        }

        var fileName = Path.GetFileName(item.PictureFileName);
        var path = Path.Combine(_environment.WebRootPath, "Pics", fileName);
        if (!System.IO.File.Exists(path))
        {
            _logger.LogWarning("Catalog image {ImagePath} was not found", path);
            return NotFound();
        }

        return PhysicalFile(path, GetImageContentType(Path.GetExtension(fileName)));
    }

    private static string GetImageContentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".bmp" => "image/bmp",
        ".tiff" => "image/tiff",
        ".wmf" => "image/wmf",
        ".jp2" => "image/jp2",
        ".svg" => "image/svg+xml",
        _ => "application/octet-stream"
    };
}
