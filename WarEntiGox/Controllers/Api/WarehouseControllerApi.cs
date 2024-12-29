using Microsoft.AspNetCore.Mvc;
using WarEntiGox.Models;
using WarEntiGox.Services;
using MongoDB.Bson;
using System;
using System.Threading.Tasks;

namespace WarEntiGox.Controllers.API
{
    [Route("api/warehouse")]
    [ApiController]
    public class WarehouseControllerApi : ControllerBase
    {
        private readonly WarehouseService _warehouseService;

        public WarehouseControllerApi(WarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        // Depoları listele (Şirket için)
        [HttpGet]
        public async Task<IActionResult> GetAllWarehouses()
        {
            // Oturumdan CompanyId'yi al
            var companyId = HttpContext.Session.GetInt32("CompanyId");

            // Eğer CompanyId bulunamazsa, 401 Unauthorized döndür
            if (!companyId.HasValue)
            {
                return Unauthorized(new { message = "CompanyId not found in session" });
            }

            // Şirketin depolarını al
            var warehouses = await _warehouseService.GetAllWarehouseAsync(companyId.Value);

            // Depolar başarılı bir şekilde alındıysa JSON formatında döndür
            return Ok(warehouses);
        }

        // Depo detaylarını görüntüle (Şirket için)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWarehouseById(ObjectId id)
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");

            // Eğer CompanyId bulunamazsa, 401 Unauthorized döndür
            if (!companyId.HasValue)
            {
                return Unauthorized(new { message = "CompanyId not found in session" });
            }

            // Depo bilgilerini al
            var warehouse = await _warehouseService.GetWarehouseByIdAsync(id, companyId.Value);

            // Depo bulunmazsa, 404 NotFound döndür
            if (warehouse == null)
            {
                return NotFound(new { message = "Warehouse not found" });
            }

            return Ok(warehouse);
        }

        // Yeni depo ekle (POST)
        [HttpPost]
        public async Task<IActionResult> CreateWarehouse([FromBody] Warehouse warehouse)
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");

            // Eğer CompanyId bulunamazsa, 401 Unauthorized döndür
            if (!companyId.HasValue)
            {
                return Unauthorized(new { message = "CompanyId not found in session" });
            }

            // Model doğrulama hatası varsa, 400 BadRequest döndür
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Şirket ID'sini depo nesnesine ekle
                warehouse.CompanyId = companyId.Value;

                // Depoyu veritabanına ekle
                await _warehouseService.CreateWarehouseAsync(warehouse);

                // Depo başarıyla eklendiyse, 201 Created döndür
                return CreatedAtAction(nameof(GetWarehouseById), new { id = warehouse.Id }, warehouse);
            }
            catch (Exception ex)
            {
                // Hata durumunda mesaj döndür
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        // Depo düzenle (PUT)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWarehouse(ObjectId id, [FromBody] Warehouse warehouse)
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");

            // Eğer CompanyId bulunamazsa, 401 Unauthorized döndür
            if (!companyId.HasValue)
            {
                return Unauthorized(new { message = "CompanyId not found in session" });
            }

            // Model doğrulama hatası varsa, 400 BadRequest döndür
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Depo bilgilerini güncelle
                warehouse.CompanyId = companyId.Value;
                warehouse.UpdateDate = DateTime.Now;

                // Depoyu güncelle
                await _warehouseService.UpdateWarehouseAsync(id, warehouse);

                return NoContent(); // Güncelleme başarılı ise 204 NoContent döndür
            }
            catch (Exception ex)
            {
                // Hata durumunda mesaj döndür
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        // Depo sil (DELETE)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWarehouse(ObjectId id)
        {
            var companyId = HttpContext.Session.GetInt32("CompanyId");

            // Eğer CompanyId bulunamazsa, 401 Unauthorized döndür
            if (!companyId.HasValue)
            {
                return Unauthorized(new { message = "CompanyId not found in session" });
            }

            try
            {
                // Depoyu soft delete et
                await _warehouseService.SoftDeleteWarehouseAsync(id, companyId.Value);

                return NoContent(); // Silme işlemi başarılı ise 204 NoContent döndür
            }
            catch (Exception ex)
            {
                // Hata durumunda mesaj döndür
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
