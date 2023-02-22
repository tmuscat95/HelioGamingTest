using Microsoft.EntityFrameworkCore;
using PhoneBook.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhoneBook.Data
{
    public class PhoneBookContext: DbContext
    {
        public PhoneBookContext(DbContextOptions<PhoneBookContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>()
                .HasMany(c => c.People)
                .WithOne(p => p.Company);

     
        }
        virtual public DbSet<Company> Companies { get; set; }
        virtual public DbSet<Person> Persons { get; set; }



    }
}
