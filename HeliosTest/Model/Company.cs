using System.ComponentModel.DataAnnotations;

namespace PhoneBook.Model
{
    public class Company
    {
        [Key]
        [Required]
        public string Name { get; set; } = "";
        public DateTime registrationDate { get; set; }

        public IEnumerable<Person>? People { get; set; }

        public int PersonCount { get { return this.People != null ? this.People.Count() : 0; Registrat} }
    }
}
