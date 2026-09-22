using eShopPorted.Models;
using eShopPorted.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace eShopPorted.Controllers;

public class CatalogController : Controller
{
    private readonly ILogger<CatalogController> _logger;
    private readonly ICatalogService _service;

    public CatalogController(ICatalogService service, ILogger<CatalogController> logger)
    {
        _service = service;
        _logger = logger;
    }

    public IActionResult Index(int pageSize = 10, int pageIndex = 0)
    {
        _logger.LogInformation("Loading catalog page {PageIndex} with page size {PageSize}", pageIndex, pageSize);
        var paginatedItems = _service.GetCatalogItemsPaginated(pageSize, pageIndex);
        SetPictureUris(paginatedItems.Data);
        return View(paginatedItems);
    }

    public IActionResult Details(int? id)
    {
        if (id is null)
        {
            return BadRequest();
        }

        var catalogItem = _service.FindCatalogItem(id.Value);
        if (catalogItem is null)
        {
            return NotFound();
        }

        SetPictureUri(catalogItem);
        return View(catalogItem);
    }

    public IActionResult Create()
    {
        PopulateCatalogLists();
        return View(new CatalogItem());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Id,Name,Description,Price,PictureFileName,CatalogTypeId,CatalogBrandId,AvailableStock,RestockThreshold,MaxStockThreshold,OnReorder")] CatalogItem catalogItem)
    {
        if (ModelState.IsValid)
        {
            _service.CreateCatalogItem(catalogItem);
            return RedirectToAction(nameof(Index));
        }

        PopulateCatalogLists(catalogItem);
        return View(catalogItem);
    }

    public IActionResult Edit(int? id)
    {
        if (id is null)
        {
            return BadRequest();
        }

        var catalogItem = _service.FindCatalogItem(id.Value);
        if (catalogItem is null)
        {
            return NotFound();
        }

        SetPictureUri(catalogItem);
        PopulateCatalogLists(catalogItem);
        return View(catalogItem);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit([Bind("Id,Name,Description,Price,PictureFileName,CatalogTypeId,CatalogBrandId,AvailableStock,RestockThreshold,MaxStockThreshold,OnReorder")] CatalogItem catalogItem)
    {
        if (ModelState.IsValid)
        {
            _service.UpdateCatalogItem(catalogItem);
            return RedirectToAction(nameof(Index));
        }

        PopulateCatalogLists(catalogItem);
        return View(catalogItem);
    }

    public IActionResult Delete(int? id)
    {
        if (id is null)
        {
            return BadRequest();
        }

        var catalogItem = _service.FindCatalogItem(id.Value);
        if (catalogItem is null)
        {
            return NotFound();
        }

        SetPictureUri(catalogItem);
        return View(catalogItem);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var catalogItem = _service.FindCatalogItem(id);
        if (catalogItem is null)
        {
            return NotFound();
        }

        _service.RemoveCatalogItem(catalogItem);
        return RedirectToAction(nameof(Index));
    }

    private void PopulateCatalogLists(CatalogItem item = null)
    {
        ViewBag.CatalogBrandId = new SelectList(_service.GetCatalogBrands(), "Id", "Brand", item?.CatalogBrandId);
        ViewBag.CatalogTypeId = new SelectList(_service.GetCatalogTypes(), "Id", "Type", item?.CatalogTypeId);
    }

    private static void SetPictureUris(IEnumerable<CatalogItem> items)
    {
        foreach (var item in items)
        {
            SetPictureUri(item);
        }
    }

    private static void SetPictureUri(CatalogItem item) => item.PictureUri = $"/Pics/{item.PictureFileName}";
}
