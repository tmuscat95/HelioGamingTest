using Microsoft.EntityFrameworkCore;
using PhoneBook.Exceptions;
using PhoneBook.Model;

namespace PhoneBook.Data.Repositories
{
    public class PersonRepo : IPersonRepo
    {
        private readonly PhoneBookContext phoneBookContext;

        public PersonRepo(PhoneBookContext phoneBookContext)
        {
            this.phoneBookContext = phoneBookContext;
        }

        public async Task Add(int id, string fullName, string phoneNumber, string address, Company? company)
        {
            if(await phoneBookContext.Persons.Where(p => p.Id == id).Select(p => p).FirstOrDefaultAsync() == null){
                throw new RepoException(StatusCodes.Status409Conflict, "Person with this ID already exists.");
            }

            try
            {
                await phoneBookContext.Persons.AddAsync(new Person { Id = id,FullName = fullName, Address = address, Company = company, PhoneNumber = phoneNumber });
                await phoneBookContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new RepoException(StatusCodes.Status500InternalServerError, ex.Message);

            }
        }

        public async Task Add(int id, string fullName, string phoneNumber, string address)
        {
            if (await phoneBookContext.Persons.Where(p => p.Id == id).Select(p => p).FirstOrDefaultAsync() == null)
            {
                throw new RepoException(StatusCodes.Status409Conflict, "Person with this ID already exists.");
            }

            try
            {
                await phoneBookContext.Persons.AddAsync(new Person { Id = id, FullName = fullName, Address = address, PhoneNumber = phoneNumber });
                await phoneBookContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new RepoException(StatusCodes.Status500InternalServerError, ex.Message);

            }
        }
        public async Task EditProfile(Person person)
        {
            try
            {
                var profile = await GetProfile(person.Id);
                profile.Address = person.Address;
                profile.Company = person.Company;
                profile.PhoneNumber = person.PhoneNumber;
                phoneBookContext.Entry(person).State = EntityState.Modified;
                await phoneBookContext.SaveChangesAsync();
            }
            catch (RepoException ex)
            {

                throw ex;
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
                phoneBookContext.Persons.Remove(person);
                await phoneBookContext.SaveChangesAsync();
            }
            catch (RepoException ex) {

                throw ex;
            }
            catch (Exception ex)
            {
                throw new RepoException(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        public async Task<List<Person>> Search(string keyword)
        {
            keyword = keyword.ToLower().Trim();
            var matches = await (from p in phoneBookContext.Persons.Include("Company")
                                 where p.Address.ToLower().Contains(keyword) || p.PhoneNumber.ToLower().Contains(keyword) || p.FullName.ToLower().Contains(keyword) || (p.Company != null && p.Company.Name.ToLower().Contains(keyword))
                                 select p).ToListAsync();
            return matches;
        }

        public async Task<List<Person>> GetAll()
        {
            return await phoneBookContext.Persons.Include("Company").Select(p => p).ToListAsync();
        }

        public async Task<Person> GetProfile(int id)
        {
            var person = await phoneBookContext.Persons.Include("Company").Where(c=> c.Id == id).FirstOrDefaultAsync();
            if (person == null)
            {
                throw new RepoException(StatusCodes.Status404NotFound, $"Person with ID {id} not found.");
            }
            return person;
        }

        public bool PersonExists(int id)
        {
            return phoneBookContext.Persons.Any(e => e.Id == id);
        }

        async Task<Person> IPersonRepo.GetRandomProfile()
        {
            var maxID = await phoneBookContext.Persons.MaxAsync(p => p.Id);

            var randomId = (new Random()).Next(maxID);
            return await phoneBookContext.Persons.Include("Company").Where(p => p.Id == randomId).FirstAsync();
        }
    }
}
