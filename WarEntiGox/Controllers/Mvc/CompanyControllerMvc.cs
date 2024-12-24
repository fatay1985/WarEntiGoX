using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using WarEntiGox.Models;
using WarEntiGox.Services;

[Route("companies")]
public class CompanyControllerMvc : Controller
{
    private readonly CompanyService _companyService;

    public CompanyControllerMvc(CompanyService companyService)
    {
        _companyService = companyService;
    }

    // Company listing with search functionality
    [HttpGet]
    public async Task<IActionResult> Index(string searchTerm)
    {
        ViewData["CurrentFilter"] = searchTerm;

        var companies = await _companyService.GetAllCompaniesAsync() ?? new List<Company>();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            companies = companies.Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return View("~/Views/Company/Index.cshtml", companies); // Pass companies list to the view
    }

    // Company creation page
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View("~/Views/Company/Create.cshtml", new Company()); // Pass a new empty company model to the view
    }

    // Company creation action
    [HttpPost("Create")]
    public async Task<IActionResult> Create(Company company)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Please ensure all fields are filled correctly.");
            return View("~/Views/Company/Create.cshtml", company);
        }

        try
        {
            company.CreateDate = DateTime.Now;
            company.UpdateDate = DateTime.Now;
            company.IsDeleted = false;

            await _companyService.CreateCompanyAsync(company);
            return RedirectToAction(nameof(Index)); // Redirect to the company listing page
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            return View("~/Views/Company/Create.cshtml", company); // Return to the create view with error message
        }
    }

    // Edit company page
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return NotFound(); // Return NotFound if id is invalid
        }

        var company = await _companyService.GetCompanyByIdAsync(objectId);
        if (company == null)
        {
            return NotFound(); // Return NotFound if company doesn't exist
        }

        return View("~/Views/Company/Edit.cshtml", company); // Pass the company details to the view
    }

    // Edit company action
    [HttpPost("Edit/{id}")]
    public async Task<IActionResult> Edit(string id, Company company)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return NotFound(); // Return NotFound if id is invalid
        }

        if (objectId != company.Id)
        {
            return NotFound(); // Return NotFound if company ID doesn't match
        }

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Please ensure all fields are filled correctly.");
            return View("~/Views/Company/Edit.cshtml", company); // Return to the edit view with error message
        }

        try
        {
            company.UpdateDate = DateTime.Now;
            await _companyService.UpdateCompanyAsync(objectId, company);
            return RedirectToAction(nameof(Index)); // Redirect to the company listing page after successful update
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            return View("~/Views/Company/Edit.cshtml", company); // Return to the edit view with error message
        }
    }

    // Delete company page
    [HttpGet("Delete/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return NotFound(); // Return NotFound if id is invalid
        }

        var company = await _companyService.GetCompanyByIdAsync(objectId);
        if (company == null)
        {
            return NotFound(); // Return NotFound if company doesn't exist
        }

        return View("~/Views/Company/Delete.cshtml", company); // Pass the company details to the delete confirmation view
    }

    // Confirm delete company action
    [HttpPost("Delete/{id}")]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return NotFound(); // Return NotFound if id is invalid
        }

        try
        {
            await _companyService.SoftDeleteCompanyAsync(objectId);
            return RedirectToAction(nameof(Index)); // Redirect to the company listing page after successful deletion
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            return RedirectToAction(nameof(Index)); // Redirect to the company listing page in case of error
        }
    }
}
