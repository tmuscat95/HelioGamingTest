using Microsoft.EntityFrameworkCore;
using PhoneBook.DTOs;
using PhoneBook.Exceptions;
using PhoneBook.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
            //if(await phoneBookContext.People.Where(p => p.Id == id).Select(p => p).FirstOrDefaultAsync() != null){
            //    throw new RepoException(StatusCodes.Status409Conflict, "Person with this ID already exists.");
            //}

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
            catch(Exception ex)
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
            catch (RepoException) {

                throw;
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
                                 where p.Address.ToLower().Contains(keyword) || p.PhoneNumber.ToLower().Contains(keyword) || p.FullName.ToLower().Contains(keyword) || (p.CompanyName != null && p.CompanyNameNavigation.CompanyName.ToLower().Contains(keyword))
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
            var maxID = await phoneBookContext.People.MaxAsync(p => p.Id);

            var randomId = (new Random()).Next(maxID);
            return await phoneBookContext.People.Include("CompanyNameNavigation").Where(p => p.Id == randomId).FirstAsync();
        }
    }
}
