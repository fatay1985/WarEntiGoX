    using Microsoft.AspNetCore.Mvc;
    using WarEntiGox.Models;
    using WarEntiGox.Services;
    using MongoDB.Bson;
    using System;
    using System.Threading.Tasks;

    namespace WarEntiGox.Controllers.MVC
    {
        [Route("warehouse")]
        public class WarehouseControllerMvc : Controller
        {
            private readonly WarehouseService _warehouseService;

            public WarehouseControllerMvc(WarehouseService warehouseService)
            {
                _warehouseService = warehouseService;
            }

            // Depoları listele (Şirket için)
            [HttpGet]
            public async Task<IActionResult> Index()
            {
                // Oturumdan CompanyId'yi al
                var companyId = HttpContext.Session.GetInt32("CompanyId");

                // CompanyId mevcut değilse, login sayfasına yönlendir
                if (!companyId.HasValue)
                {
                    return RedirectToAction("Index", "Login");
                }

                // Şirketin depolarını al ve listeyi view'a gönder
                var warehouses = await _warehouseService.GetAllWarehouseAsync(companyId.Value);
                return View("~/Views/Warehouse/Index.cshtml", warehouses);
            }

            // Depo detaylarını görüntüle (Şirket için)
            [HttpGet("Details/{id}")]
            public async Task<IActionResult> Details(ObjectId id)
            {
                var companyId = HttpContext.Session.GetInt32("CompanyId");

                // Eğer CompanyId bulunamazsa, login sayfasına yönlendir
                if (!companyId.HasValue)
                {
                    return RedirectToAction("Index", "Login");
                }

                // Depo bilgilerini al
                var warehouse = await _warehouseService.GetWarehouseByIdAsync(id, companyId.Value);

                // Depo bulunmazsa, 404 hatası döndür
                if (warehouse == null)
                {
                    return NotFound(); 
                }

                return View("~/Views/Warehouse/Details.cshtml", warehouse);
            }

            // Yeni depo ekleme sayfasını görüntüle
            [HttpGet("Create")]
            public IActionResult Create()
            {
                var companyId = HttpContext.Session.GetInt32("CompanyId");

                // Eğer CompanyId bulunamazsa, login sayfasına yönlendir
                if (!companyId.HasValue)
                {
                    return RedirectToAction("Index", "Login");
                }

                return View("~/Views/Warehouse/Create.cshtml");
            }

            // Yeni depo ekle (POST)
            [HttpPost("Create")]
            public async Task<IActionResult> Create(Warehouse warehouse)
            {
                var companyId = HttpContext.Session.GetInt32("CompanyId");

                // Eğer CompanyId bulunamazsa, login sayfasına yönlendir
                if (!companyId.HasValue)
                {
                    return RedirectToAction("Index", "Login");
                }

                // Model doğrulama hatası varsa formu tekrar göster
                if (!ModelState.IsValid)
                {
                    return View("~/Views/Warehouse/Create.cshtml", warehouse);
                }

                try
                {
                    // Şirket ID'sini depo nesnesine ekle
                    warehouse.CompanyId = companyId.Value;

                    // Depoyu veritabanına ekle
                    await _warehouseService.CreateWarehouseAsync(warehouse);

                    // Depo başarıyla eklendiyse, depo listesine yönlendir
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Hata durumunda mesaj ekle ve formu tekrar göster
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                    return View("~/Views/Warehouse/Create.cshtml", warehouse);
                }
            }

            // Depo düzenleme sayfasını görüntüle
            [HttpGet("Edit/{id}")]
            public async Task<IActionResult> Edit(ObjectId id)
            {
                var companyId = HttpContext.Session.GetInt32("CompanyId");

                // Eğer CompanyId bulunamazsa, login sayfasına yönlendir
                if (!companyId.HasValue)
                {
                    return RedirectToAction("Index", "Login");
                }

                // Depo bilgilerini al
                var warehouse = await _warehouseService.GetWarehouseByIdAsync(id, companyId.Value);

                // Depo bulunmazsa, 404 hatası döndür
                if (warehouse == null)
                {
                    return NotFound();
                }

                return View("~/Views/Warehouse/Edit.cshtml", warehouse);
            }

            // Depo düzenleme (POST)
            [HttpPost("Edit/{id}")]
            public async Task<IActionResult> Edit(ObjectId id, Warehouse warehouse)
            {
                var companyId = HttpContext.Session.GetInt32("CompanyId");

                // Eğer CompanyId bulunamazsa, login sayfasına yönlendir
                if (!companyId.HasValue)
                {
                    return RedirectToAction("Index", "Login");
                }

                // Model doğrulama hatası varsa formu tekrar göster
                if (!ModelState.IsValid)
                {
                    return View("~/Views/Warehouse/Edit.cshtml", warehouse);
                }

                try
                {
                    // Şirket ID'sini depo nesnesine ekle
                    warehouse.CompanyId = companyId.Value;
                    warehouse.UpdateDate = DateTime.Now;

                    // Depoyu güncelle
                    await _warehouseService.UpdateWarehouseAsync(id, warehouse);

                    // Depo başarıyla güncelleniyorsa, depo listesine yönlendir
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Hata durumunda mesaj ekle ve formu tekrar göster
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                    return View("~/Views/Warehouse/Edit.cshtml", warehouse);
                }
            }

            // Depoyu silme (POST)
            [HttpPost("Delete/{id}")]
            public async Task<IActionResult> Delete(ObjectId id)
            {
                var companyId = HttpContext.Session.GetInt32("CompanyId");

                // Eğer CompanyId bulunamazsa, login sayfasına yönlendir
                if (!companyId.HasValue)
                {
                    return RedirectToAction("Index", "Login");
                }

                try
                {
                    // Depoyu soft delete et
                    await _warehouseService.SoftDeleteWarehouseAsync(id, companyId.Value);

                    // Depo başarıyla silindiyse, depo listesine yönlendir
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Hata durumunda mesaj ekle ve depo listesine yönlendir
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                    return RedirectToAction(nameof(Index));
                }
            }
        }
    }
