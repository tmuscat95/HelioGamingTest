using Microsoft.EntityFrameworkCore;
using PhoneBook.DTOs;
using PhoneBook.Exceptions;
using PhoneBook.Models;

namespace PhoneBook.Data.Repositories
{
    public class PersonRepo : IPersonRepo
    {
        private readonly PhoneBookContext phoneBookContext;

        public PersonRepo(PhoneBookContext phoneBookContext)
        {
            this.phoneBookContext = phoneBookContext;
        }

        public async Task<Person> Add(string fullName, string phoneNumber, string address, string companyName)
        {
            try
            {
                var newPerson = new Person { FullName = fullName, Address = address, CompanyName = companyName, PhoneNumber = phoneNumber };
                await phoneBookContext.People.AddAsync(newPerson);
                await phoneBookContext.SaveChangesAsync();
                return newPerson;
            }
            catch (Exception ex)
            {
                throw new RepoException(StatusCodes.Status500InternalServerError, ex.Message);

            }
        }

        public async Task EditProfile(int id, NewPersonDTO editedPersonDto)
        {
            try
            {
                var profile = await phoneBookContext.People.Where(c => c.Id == id).FirstOrDefaultAsync();
                if(profile == null)
                {
                    throw new RepoException(StatusCodes.Status404NotFound, $"Person with ID {id} not found.");
                }
                profile.Address = editedPersonDto.Address;
                profile.CompanyName = editedPersonDto.CompanyName;
                profile.PhoneNumber = editedPersonDto.PhoneNumber;
                profile.FullName = editedPersonDto.FullName;
                phoneBookContext.Entry(profile).State = EntityState.Modified;
                await phoneBookContext.SaveChangesAsync();
            }
            catch (RepoException)
            {

                throw;
            }
            catch (DbUpdateConcurrencyException ex)
            {

                throw new RepoException(StatusCodes.Status409Conflict, ex.Message); ;
            }
            catch (Exception ex)
            {
                throw new RepoException(StatusCodes.Status500InternalServerError, ex.Message);

            }
        }

        public async Task RemoveProfile(int id)
        {
            try
            {
                var person = await GetProfile(id);
                phoneBookContext.People.Remove(person);
                await phoneBookContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex) {

                throw new RepoException(StatusCodes.Status409Conflict, ex.Message); ;
            }
            catch (Exception ex)
            {
                throw new RepoException(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        public async Task<List<Person>> Search(string keyword)
        {
            keyword = keyword.ToLower().Trim();
            var matches = await (from p in phoneBookContext.People.Include("CompanyNameNavigation")
                                 where p.Address.ToLower().Contains(keyword) || p.PhoneNumber.ToLower().Contains(keyword) || p.FullName.ToLower().Contains(keyword) || (p.CompanyName.ToLower().Contains(keyword) || (p.CompanyNameNavigation.RegistrationDate != null && p.CompanyNameNavigation.RegistrationDate!.ToString()!.Contains(keyword)))
                                 select p).ToListAsync();
            return matches;
        }

        public async Task<List<Person>> GetAll()
        {
            return await phoneBookContext.People.Include("CompanyNameNavigation").Select(p => p).ToListAsync();
        }

        public async Task<Person> GetProfile(int id)
        {
            var person = await phoneBookContext.People.Include("CompanyNameNavigation").Where(c=> c.Id == id).FirstOrDefaultAsync();
            if (person == null)
            {
                throw new RepoException(StatusCodes.Status404NotFound, $"Person with ID {id} not found.");
            }
            return person;
        }

        public bool PersonExists(int id)
        {
            return phoneBookContext.People.Any(e => e.Id == id);
        }

        async Task<Person> IPersonRepo.GetRandomProfile()
        {
            var rand = new Random();
            return await phoneBookContext.People.OrderBy(p=>rand.Next()).Include("CompanyNameNavigation").FirstAsync();
            
        }
    }
}
