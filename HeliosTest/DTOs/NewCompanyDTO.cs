using PhoneBook.Models;

namespace PhoneBook.DTOs
{
    public class NewCompanyDTO
    {
        public string CompanyName { get; set; } = String.Empty;
        public string RegistrationDate { get; set; } = String.Empty;

        public List<NewPersonDTO> people { get; set; } = new List<NewPersonDTO>();
    }
}
