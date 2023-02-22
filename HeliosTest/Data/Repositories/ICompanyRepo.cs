using PhoneBook.DTOs;
using PhoneBook.Model;

namespace PhoneBook.Data.Repositories
{
    public interface ICompanyRepo
    {
        Task Add(string companyName, string registrationDate, List<NewPersonDTO> people);
        Task Add(string companyName, DateTime registrationDate, List<NewPersonDTO> people);

        Task<IEnumerable<Company>> GetAll();

        Task<Company?> Get(string companyName);

        public bool CompanyExists(string companyName);


    }
}
