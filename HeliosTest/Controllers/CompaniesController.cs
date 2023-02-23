using Microsoft.AspNetCore.Mvc;
using PhoneBook.Data;
using PhoneBook.Data.Repositories;
using PhoneBook.DTOs;
using PhoneBook.Exceptions;
using PhoneBook.Models;

namespace PhoneBook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyRepo companyRepo;

        public CompaniesController(PhoneBookContext context, ICompanyRepo companyRepo)
        {
            this.companyRepo = companyRepo;
        }

        // GET: api/Companies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Company>>> View()
        {
            return Ok(await companyRepo.GetAll());
        }

        // POST: api/Companies
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Company>> AddCompany(NewCompanyDTO company)
        {
            if (company.CompanyName == string.Empty)
            {
                return BadRequest("Company Name cannot be empty");
            }

            if (company.RegistrationDate == string.Empty)
            {
                return BadRequest("Registration Date cannot be empty");
            }

            try
            {
                await companyRepo.Add(company.CompanyName, company.RegistrationDate, company.people);
            }
            catch (RepoException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }

            return CreatedAtAction(nameof(View), nameof(CompaniesController), new { id = company.CompanyName }, company);
        }

    }
}
