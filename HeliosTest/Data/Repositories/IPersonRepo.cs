using PhoneBook.DTOs;
using PhoneBook.Models;

namespace PhoneBook.Data.Repositories
{
    public interface IPersonRepo
    {
        public Task<Person> Add(string fullName, string phoneNumber, string address, string companyName);

        public Task<List<Person>> GetAll();

        public Task<List<Person>> Search(string keyword);

        public Task<Person> GetProfile(int id);

        public Task EditProfile(int id, NewPersonDTO newPersonDTO);

        public Task RemoveProfile(int id);

        public Task<Person> GetRandomProfile();
        public bool PersonExists(int id);
    }
}
