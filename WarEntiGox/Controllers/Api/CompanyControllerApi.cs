using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using WarEntiGox.Models;
using WarEntiGox.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WarEntiGox.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyControllerApi : ControllerBase
    {
        private readonly CompanyService _companyService;

        public CompanyControllerApi(CompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Company>>> GetAllCompanies()
        {
            var companies = await _companyService.GetCompaniesAsync();
            return Ok(companies);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> GetCompanyById(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest("Invalid ID format.");

            var company = await _companyService.GetCompanyByIdAsync(objectId);
            if (company == null)
                return NotFound();

            return Ok(company);
        }

        [HttpPost]
        public async Task<ActionResult> CreateCompany([FromBody] Company company)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            company.CreateDate = DateTime.Now;
            company.IsDeleted = false;

            await _companyService.CreateCompanyAsync(company);

            return CreatedAtAction(nameof(GetCompanyById), new { id = company.Id.ToString() }, company);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCompany(string id, [FromBody] Company company)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest("Invalid ID format.");

            company.UpdateDate = DateTime.Now;

            await _companyService.UpdateCompanyAsync(objectId, company);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return NotFound();

            await _companyService.SoftDeleteCompanyAsync(objectId);
            return NoContent();
        }
    }
}
