using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using WarEntiGox.Models;
using WarEntiGox.Services;
using System.Linq;
using System.Threading.Tasks;

[Route("mvc/[controller]")]
public class CompanyControllerMvc : Controller
{
    private readonly CompanyService _companyService;

    public CompanyControllerMvc(CompanyService companyService)
    {
        _companyService = companyService;
    }

    // Index action: Kullanıcının kendi şirketine ait şirketleri gösterir.
    [HttpGet]
    public async Task<IActionResult> Index(string searchTerm)
    {
        var companyId = HttpContext.Session.GetInt32("CompanyId");
        if (companyId == null)
        {
            return RedirectToAction("Index", "Login"); // Kullanıcı oturum açmamışsa giriş sayfasına yönlendir
        }

        // Filtreleme için arama terimini ViewData'ya aktar
        ViewData["CurrentFilter"] = searchTerm;

        // Şirketleri getir
        var companies = await _companyService.GetCompaniesAsync();

        // Yalnızca oturum açmış kullanıcının şirketine ait verileri göster
        companies = companies.Where(c => c.CompanyId == companyId).ToList();

        // Eğer bir arama terimi varsa, şirket isimlerine göre filtreleme yap
        if (!string.IsNullOrEmpty(searchTerm))
        {
            companies = companies.Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return View(companies);
    }

    // Yeni bir şirket oluşturma formuna yönlendir
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    // Yeni şirketi oluştur
    [HttpPost("Create")]
    public async Task<IActionResult> Create(Company company)
    {
        if (ModelState.IsValid)
        {
            company.CreateDate = DateTime.Now;
            company.IsDeleted = false;
            company.CompanyId = HttpContext.Session.GetInt32("CompanyId").Value; // Oturumdaki CompanyId'yi al
            await _companyService.CreateCompanyAsync(company);
            return RedirectToAction(nameof(Index));
        }
        return View(company);
    }

    // Şirketi düzenleme sayfasına yönlendir
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return NotFound();

        var company = await _companyService.GetCompanyByIdAsync(objectId);
        if (company == null)
            return NotFound();

        return View(company);
    }

    // Şirketi güncelle
    [HttpPost("Edit/{id}")]
    public async Task<IActionResult> Edit(string id, Company company)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return NotFound();

        if (ModelState.IsValid)
        {
            await _companyService.UpdateCompanyAsync(objectId, company);
            return RedirectToAction(nameof(Index));
        }

        return View(company);
    }

    // Şirket silme sayfasına yönlendir
    [HttpGet("Delete/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return NotFound();

        var company = await _companyService.GetCompanyByIdAsync(objectId);
        if (company == null)
            return NotFound();

        return View(company);
    }

    // Şirketi sil
    [HttpPost("Delete/{id}")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return NotFound();

        await _companyService.SoftDeleteCompanyAsync(objectId);
        return RedirectToAction(nameof(Index));
    }
}
