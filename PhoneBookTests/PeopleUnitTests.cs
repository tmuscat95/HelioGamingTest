using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using PhoneBook.Data;
using PhoneBook.DTOs;
using PhoneBook.Models;
using System.Net.Http.Json;

namespace PhoneBookTests
{

    [TestClass]
    public class PeopleUnitTests
    {

        private static WebApplicationFactory<Program>? factory;
        private static HttpClient? client;
        private static PhoneBookContext? phoneBookContext;

        private static void CleanUpEntries(List<int> ids)
        {
            phoneBookContext!.RemoveRange(phoneBookContext.People.Where(p => ids.Contains(p.Id)).Select(p => p));
            phoneBookContext.SaveChanges();
        }

        private static bool ComparePersons(Person x, Person y)
        {
            return x.CompanyName == y.CompanyName && x.FullName == y.FullName && x.PhoneNumber == y.PhoneNumber && x.Address == y.Address;
        }

        private static void RecreateContext()
        {
            phoneBookContext?.Dispose();
            phoneBookContext = new PhoneBookContext();
        }
        [TestInitialize()]
        public void Startup()
        {
            RecreateContext();
        }

        [TestCleanup()]
        public void Cleanup()
        {

            phoneBookContext?.Dispose();
        }

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            factory = new WebApplicationFactory<Program>();
            client = factory!.CreateClient();

        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            factory!.Dispose();
            client!.Dispose();
            phoneBookContext?.Dispose();

        }

        [TestMethod]
        public async Task Person_Add()
        {
            var peopleRes = await client!.GetFromJsonAsync<List<Person>>("api/People");
            Assert.IsNotNull(peopleRes);
            var peopleCountBefore = peopleRes.Count;
            //NewPersonDTO newPerson = GetRandomPerson();
            var newPersonDto = new NewPersonDTO { Address = "One Hacker Way", CompanyName = "Meta", FullName = "John Doe", PhoneNumber = "99990000" };
            var response = await client!.PostAsJsonAsync("api/people", newPersonDto);
            Assert.AreEqual(StatusCodes.Status201Created, ((int)response.StatusCode));
            var createdPerson = await response.Content.ReadFromJsonAsync<Person>();
            Assert.IsNotNull(createdPerson);
            Assert.IsTrue(phoneBookContext!.People.Count() - peopleCountBefore == 1);
            var foundPerson = phoneBookContext.People
                .Where(p => p.Id == createdPerson.Id && p.Address == newPersonDto.Address && p.FullName == newPersonDto.FullName && p.PhoneNumber == newPersonDto.PhoneNumber && p.CompanyName == newPersonDto.CompanyName)
                .Select(p => p).FirstOrDefault();
            Assert.IsNotNull(foundPerson);
            CleanUpEntries(new List<int>(new int[] { foundPerson.Id }));
        }

        [TestMethod]
        public async Task Person_GetAll()
        {

            var personsFromDb = await phoneBookContext!.People.Select(p => p).ToListAsync();
            var peopleRes = await client!.GetFromJsonAsync<List<Person>>("api/people");
            Assert.IsNotNull(peopleRes);
            Assert.AreEqual(peopleRes.Count, personsFromDb.Count);

            foreach (var personFromDb in personsFromDb!)
            {
                var foundPerson = peopleRes
                    .Where(p => p.Id == personFromDb.Id && p.Address == personFromDb.Address && p.FullName == personFromDb.FullName && p.PhoneNumber == personFromDb.PhoneNumber && p.CompanyName == personFromDb.CompanyName)
                    .FirstOrDefault();
                Assert.IsNotNull(foundPerson);
            }


        }

        [TestMethod]
        public async Task Add_Edit_Remove()
        {
            //Add
            var peopleRes = await client!.GetFromJsonAsync<List<Person>>("api/People");
            Assert.IsNotNull(peopleRes);
            var peopleCountBefore = peopleRes.Count;
            //NewPersonDTO newPerson = GetRandomPerson();
            var newPersonDto = new NewPersonDTO { Address = "One Hacker Way", CompanyName = "Meta", FullName = "John Doe", PhoneNumber = "99990000" };
            var response = await client!.PostAsJsonAsync("api/people", newPersonDto);
            Assert.AreEqual(StatusCodes.Status201Created, ((int)response.StatusCode));
            var createdPerson = await response.Content.ReadFromJsonAsync<Person>();
            Assert.IsNotNull(createdPerson);
            Assert.IsTrue(phoneBookContext!.People.Count() - peopleCountBefore == 1);
            var foundPerson = phoneBookContext.People
                .Where(p => p.Id == createdPerson.Id && p.Address == newPersonDto.Address && p.FullName == newPersonDto.FullName && p.PhoneNumber == newPersonDto.PhoneNumber && p.CompanyName == newPersonDto.CompanyName)
                .Select(p => p).FirstOrDefault();
            Assert.IsNotNull(foundPerson);

            //Edit
            var editedPersonDto = new NewPersonDTO { Address = "1 Negra Arroyo Lane", CompanyName = "Google", FullName = "Walter White", PhoneNumber = "99990000" };
            var editRes = await client!.PutAsJsonAsync($"api/People/{createdPerson.Id}", editedPersonDto);
            Assert.AreEqual(StatusCodes.Status204NoContent, ((int)editRes.StatusCode));
            RecreateContext();
            var editedPerson = await phoneBookContext.People.Where(p => p.Id == createdPerson.Id).Select(p => p).FirstOrDefaultAsync();
            Assert.IsNotNull(editedPerson);
            Assert.AreEqual(editedPersonDto.Address, editedPerson.Address);
            Assert.AreEqual(editedPersonDto.CompanyName, editedPerson.CompanyName);
            Assert.AreEqual(editedPersonDto.FullName, editedPerson.FullName);
            Assert.AreEqual(editedPersonDto.PhoneNumber, editedPerson.PhoneNumber);

            //Remove
            await client!.DeleteAsync($"api/People/{createdPerson.Id}");
            var deletedPerson = await phoneBookContext.People.Where(p => p.Id == createdPerson.Id).Select(p => p).FirstOrDefaultAsync();
            Assert.IsNull(deletedPerson);


        }

        [TestMethod]
        public async Task Person_WildCard()
        {
            var person = client!.GetFromJsonAsync<Person>("api/person/wildcard");
            Assert.IsNotNull(person);
            var personFound = await phoneBookContext!.People.Where(p => p.Id == person.Id).Select(p => p).FirstOrDefaultAsync();
            Assert.IsNotNull(personFound);
        }

        [TestMethod]
        public async Task Person_Search()
        {
            var testSuffix = (new Random()).Next();
            var testAddress = $"TestAddress{testSuffix}";
            var testCompanyName = $"Tesla";
            var testPhoneNumber = $"TestPhone{testSuffix}";
            var testFullName = $"TestName{testSuffix}";

            var testPerson = new Person() { Address = testAddress, CompanyName = testCompanyName, PhoneNumber = testPhoneNumber, FullName = testFullName };
            await phoneBookContext!.People.AddAsync(testPerson);
            await phoneBookContext.SaveChangesAsync();
            //Search By Full Name
            var searchResults = await client!.GetFromJsonAsync<List<Person>>($"api/People/search/{testPerson.FullName}");
            Assert.IsNotNull(searchResults);
            Assert.AreEqual(1,searchResults.Count);
            Assert.IsTrue(ComparePersons(testPerson,searchResults.First()));

            //Search By Phone Number
            searchResults = await client!.GetFromJsonAsync<List<Person>>($"api/People/search/{testPerson.PhoneNumber}");
            Assert.IsNotNull(searchResults);
            Assert.AreEqual(1, searchResults.Count);
            Assert.IsTrue(ComparePersons(testPerson, searchResults.First()));

            //Search By Address
            searchResults = await client!.GetFromJsonAsync<List<Person>>($"api/People/search/{testPerson.Address}");
            Assert.IsNotNull(searchResults);
            Assert.AreEqual(1, searchResults.Count);
            Assert.IsTrue(ComparePersons(testPerson, searchResults.First()));

            //Search By Company Name
            searchResults = await client!.GetFromJsonAsync<List<Person>>($"api/People/search/{testPerson.Address}");
            Assert.IsNotNull(searchResults);
            Assert.IsTrue(searchResults.Count >= 1);
            Assert.IsTrue(searchResults.Where(p => ComparePersons(p, testPerson)).Count() >= 1);

            //Search By Company Registration Date
            var companyRegistrationDate = await phoneBookContext.Companies.Where(c => c.CompanyName == testPerson.CompanyName).Select(c => c.RegistrationDate).FirstAsync();
            searchResults = await client!.GetFromJsonAsync<List<Person>>($"api/People/search/{companyRegistrationDate!.Value.ToString("yyyy-MM-dd")}");
            Assert.IsNotNull(searchResults);
            Assert.IsTrue(searchResults.Count >= 1);
            Assert.IsTrue(searchResults.Where(p => ComparePersons(p, testPerson)).Count() >= 1);

            CleanUpEntries(new List<int>() { testPerson.Id });

        }


    }
}