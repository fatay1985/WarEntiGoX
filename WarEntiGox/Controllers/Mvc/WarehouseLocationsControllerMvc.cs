using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WarEntiGox.Models;
using WarEntiGox.Services;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace WarEntiGox.Controllers.MVC
{
    [Route("warehouselocations")]
    public class WarehouseLocationsControllerMvc : Controller
    {
        private readonly WarehouseLocationService _warehouseLocationService;
        private readonly WarehouseService _warehouseService;

        public WarehouseLocationsControllerMvc(WarehouseLocationService warehouseLocationService, WarehouseService warehouseService)
        {
            _warehouseLocationService = warehouseLocationService;
            _warehouseService = warehouseService;
        }

        // List warehouse locations for the specific warehouse and company
        [HttpGet]
        public async Task<IActionResult> Index(int warehouseId)
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");

            if (!companyId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            var locations = await _warehouseLocationService.GetAllWarehouseLocationsAsync(warehouseId, companyId.Value);
            return View("~/Views/WarehouseLocation/Index.cshtml", locations);
        }

        // View warehouse location details
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(ObjectId id)
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");

            if (!companyId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            var location = await _warehouseLocationService.GetWarehouseLocationByIdAsync(id, companyId.Value);
            if (location == null)
            {
                return NotFound(); // If location not found, return 404
            }

            return View("~/Views/WarehouseLocation/Details.cshtml", location);
        }

        // Create new warehouse location
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");

            if (!companyId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            var warehouses = await _warehouseService.GetAllWarehouseAsync(companyId.Value);
            ViewBag.Warehouses = new SelectList(warehouses, "WarehouseId", "WarehouseName");

            return View("~/Views/WarehouseLocation/Create.cshtml");
        }

        // Create warehouse location POST
        [HttpPost("Create")]
        public async Task<IActionResult> Create(WarehouseLocation location)
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");

            if (!companyId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid)
            {
                var warehouses = await _warehouseService.GetAllWarehouseAsync(companyId.Value);
                ViewBag.Warehouses = new SelectList(warehouses, "WarehouseId", "WarehouseName");
                return View("~/Views/WarehouseLocation/Create.cshtml", location);
            }

            try
            {
                location.CompanyId = companyId.Value;
                location.CreateDate = DateTime.Now;
                await _warehouseLocationService.CreateWarehouseLocationAsync(location);
                return RedirectToAction(nameof(Index), new { warehouseId = location.WarehouseId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                var warehouses = await _warehouseService.GetAllWarehouseAsync(companyId.Value);
                ViewBag.Warehouses = new SelectList(warehouses, "WarehouseId", "WarehouseName");
                return View("~/Views/WarehouseLocation/Create.cshtml", location);
            }
        }

        // Edit warehouse location
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(ObjectId id)
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");

            if (!companyId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            var location = await _warehouseLocationService.GetWarehouseLocationByIdAsync(id, companyId.Value);
            if (location == null)
            {
                return NotFound(); // If location not found, return 404
            }

            var warehouses = await _warehouseService.GetAllWarehouseAsync(companyId.Value);
            ViewBag.Warehouses = new SelectList(warehouses, "WarehouseId", "WarehouseName", location.WarehouseId.ToString());

            return View("~/Views/WarehouseLocation/Edit.cshtml", location);
        }

        // Edit warehouse location POST
        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(ObjectId id, WarehouseLocation location)
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");

            if (!companyId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid)
            {
                var warehouses = await _warehouseService.GetAllWarehouseAsync(companyId.Value);
                ViewBag.Warehouses = new SelectList(warehouses, "WarehouseId", "WarehouseName", location.WarehouseId.ToString());
                return View("~/Views/WarehouseLocation/Edit.cshtml", location);
            }

            try
            {
                location.CompanyId = companyId.Value;
                location.UpdateDate = DateTime.Now;
                await _warehouseLocationService.UpdateWarehouseLocationAsync(id, location);
                return RedirectToAction(nameof(Index), new { warehouseId = location.WarehouseId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                var warehouses = await _warehouseService.GetAllWarehouseAsync(companyId.Value);
                ViewBag.Warehouses = new SelectList(warehouses, "WarehouseId", "WarehouseName", location.WarehouseId.ToString());
                return View("~/Views/WarehouseLocation/Edit.cshtml", location);
            }
        }

        // Delete warehouse location
        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(ObjectId id, int warehouseId)
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");

            if (!companyId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            try
            {
                await _warehouseLocationService.SoftDeleteWarehouseLocationAsync(id, warehouseId, companyId.Value);
                return RedirectToAction(nameof(Index), new { warehouseId = warehouseId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return RedirectToAction(nameof(Index), new { warehouseId = warehouseId });
            }
        }
    }
}
