using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhoneBook.Model
{
    public class Person
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FullName { get; set; } = String.Empty;
        public string PhoneNumber { get; set; } = String.Empty;
        public string Address { get; set; } = String.Empty;
        public Company? Company { get; set; }


    }
}
