using Microsoft.AspNetCore.Mvc;
using PhoneBook.Data.Repositories;
using PhoneBook.DTOs;
using PhoneBook.Exceptions;
using PhoneBook.Models;

namespace PhoneBook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        //private readonly PhoneBookContext _context;
        private readonly IPersonRepo personRepo;
        private readonly ICompanyRepo companyRepo;

        public PeopleController(IPersonRepo personRepo, ICompanyRepo companyRepo)
        {
            this.personRepo = personRepo;
            this.companyRepo = companyRepo;
        }

        // GET: api/People
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Person>>> GetAll()
        {
            return Ok(await personRepo.GetAll());
        }


        //GET: api/People
        [HttpGet("wildcard")]
        public async Task<ActionResult<Person>> WildCard()
        {
            return Ok(await personRepo.GetRandomProfile());
        }
        // GET: api/People/5
        [HttpGet("{id}")]
        public async Task<ActionResult<List<Person>>> Search(string searchTerm)
        {
            var persons = await personRepo.Search(searchTerm);

            if (persons.Count == 0)
            {
                return NotFound();
            }

            return Ok(persons);
        }

        // PUT: api/People/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerson(int id, [FromBody] NewPersonDTO updatedPersonProfile)
        {



            try
            {
                var company = await companyRepo.Get(updatedPersonProfile.CompanyName);
                if (company == null)
                {
                    return NotFound($"Company {updatedPersonProfile.CompanyName} not found.");
                }
                await personRepo.EditProfile(new Person { Id = id, Address = updatedPersonProfile.Address, CompanyName = company.CompanyName, FullName = updatedPersonProfile.FullName, PhoneNumber = updatedPersonProfile.PhoneNumber });
            }
            catch (RepoException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }


            return NoContent();
        }

        // POST: api/People
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Person>> Add(NewPersonDTO newPerson)
        {
            if (newPerson.FullName == string.Empty || newPerson.FullName == string.Empty || newPerson.Address == string.Empty)
                return BadRequest();

            Company? newPersonCompany = null;
            try
            {
                if (newPerson.CompanyName != string.Empty)
                {
  
                        newPersonCompany = await companyRepo.Get(newPerson.CompanyName);
                 
                }

                if (newPersonCompany != null)
                    await personRepo.Add(newPerson.Id, newPerson.FullName, newPerson.PhoneNumber, newPerson.Address, newPersonCompany.CompanyName);
    
            }
            catch (RepoException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }

            return CreatedAtAction("GetPerson", new { id = newPerson.FullName }, newPerson);
        }

        // DELETE: api/People/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerson(int id)
        {
            try
            {
                await personRepo.RemoveProfile(id);
            }
            catch (RepoException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }

            return NoContent();
        }



    }
}
