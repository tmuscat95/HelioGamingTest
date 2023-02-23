using Microsoft.EntityFrameworkCore;
using PhoneBook.DTOs;
using PhoneBook.Exceptions;
using PhoneBook.Models;

namespace PhoneBook.Data.Repositories
{
    public class CompanyRepo : ICompanyRepo
    {
        private readonly PhoneBookContext phoneBookContext;

        public CompanyRepo(PhoneBookContext phoneBookContext, IPersonRepo personRepo)
        {
            this.phoneBookContext = phoneBookContext;
        }
        public async Task Add(string companyName, string registrationDate, List<NewPersonDTO> people)
        {
            try
            {
                var regDateParsed = DateTime.Parse(registrationDate);
                await Add(companyName, regDateParsed, people);
            }
            catch (FormatException)
            {
                throw new RepoException(StatusCodes.Status406NotAcceptable, "Registration date malformed");
            }

        }

        public async Task Add(string companyName, DateTime registrationDate, List<NewPersonDTO> people)
        {
            var company = await phoneBookContext.Companies.Where(c => c.CompanyName.ToLower().Trim() == companyName.ToLower().Trim()).FirstOrDefaultAsync();
            if (company != null)
            {
                throw new RepoException(StatusCodes.Status409Conflict, "Company Already Exists.");
            }
            try
            {
                await phoneBookContext.Companies.AddAsync(new Company { CompanyName = companyName, RegistrationDate = registrationDate });
                await phoneBookContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new RepoException(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        public async Task<Company> Get(string companyName)
        {

            var company = await phoneBookContext.Companies.Include("People").Where(c => c.CompanyName.ToLower().Trim() == companyName.ToLower().Trim()).FirstOrDefaultAsync();
            if (company == null)
                throw new RepoException(StatusCodes.Status404NotFound, "No company with that name exists");
            return company;


        }

        public async Task<IEnumerable<Company>> GetAll()
        {
            return await phoneBookContext.Companies.Include("People").Select(c => c).ToListAsync();
        }

        public bool CompanyExists(string companyName)
        {
            return phoneBookContext.Companies.Any(e => e.CompanyName == companyName);
        }
    }
}
