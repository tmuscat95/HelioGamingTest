using PhoneBook.Models;

namespace PhoneBook.Data.Repositories
{
    public interface IPersonRepo
    {
        public Task Add(int id, string fullName, string phoneNumber, string address, string CompanyName);

        public Task<List<Person>> GetAll();

        public Task<List<Person>> Search(string keyword);

        public Task<Person> GetProfile(int id);

        public Task EditProfile(Person person);

        public Task RemoveProfile(int id);

        public Task<Person> GetRandomProfile();
        public bool PersonExists(int id);
    }
}
