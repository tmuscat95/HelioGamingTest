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
        // GET: api/People/search/{searchTerm}
        [HttpGet("search/{searchTerm}")]
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
                await personRepo.EditProfile(id, updatedPersonProfile);
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
        public async Task<ActionResult<int>> Add(NewPersonDTO newPersonDto)
        {
            if (newPersonDto.FullName == string.Empty || newPersonDto.FullName == string.Empty || newPersonDto.Address == string.Empty)
                return BadRequest();

            try
            {
                Company newPersonCompany = await companyRepo.Get(newPersonDto.CompanyName);
                Person newPerson = await personRepo.Add(newPersonDto.FullName, newPersonDto.PhoneNumber, newPersonDto.Address, newPersonCompany.CompanyName);
                return CreatedAtAction(nameof(GetPerson), nameof(PeopleController), new { id = newPerson.Id }, newPerson);
            }
            catch (RepoException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }


        }

        //GET: api/People/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Person>> GetPerson(int id)
        {
            try
            {
                return await personRepo.GetProfile(id);
            }
            catch (RepoException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
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
