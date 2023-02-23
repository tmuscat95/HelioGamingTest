using PhoneBook.Models;

namespace PhoneBook.DTOs
{
    public class NewPersonDTO
    {
        //public int Id { get; set; }
        public string FullName { get; set; } = String.Empty;
        public string PhoneNumber { get; set; } = String.Empty;
        public string Address { get; set; } = String.Empty;
        public string CompanyName { get; set; } = String.Empty;
    }
}
