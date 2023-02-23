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
    public class CompanyUnitTests
    {

        private static WebApplicationFactory<Program>? factory;
        private static HttpClient? client;
        private static PhoneBookContext? phoneBookContext;
        private static void CleanUpEntries(List<string> companyNames)
        {
            phoneBookContext!.RemoveRange(phoneBookContext.Companies.Where(c => companyNames.Contains(c.CompanyName)).Select(c => c));
            phoneBookContext.SaveChanges();
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
        public async Task Company_Add()
        {

            var companiesCountBefore = phoneBookContext!.Companies.Count();
            var companyName = $"TEST{(new Random()).Next()}";
            var postedDto = new NewCompanyDTO { CompanyName = companyName, RegistrationDate = DateTime.Today.ToString() };
            var response = await client!.PostAsJsonAsync("api/companies", postedDto);
            Assert.AreEqual(StatusCodes.Status201Created, ((int)response.StatusCode));
            var companiesCountAfter = phoneBookContext.Companies.Count();
            Assert.AreEqual(1, companiesCountAfter - companiesCountBefore);
            var newCompanyOccurrenceInDbCount = await phoneBookContext.Companies
                .Where(c => c.CompanyName == postedDto.CompanyName && c.RegistrationDate.Equals(DateTime.Parse(postedDto.RegistrationDate)))
                .Select(c => c).CountAsync();
            Assert.AreEqual(1, newCompanyOccurrenceInDbCount);


            //TEST error condition
            var duplicateDto = new NewCompanyDTO { CompanyName = "Tesla", RegistrationDate = DateTime.Today.ToString() };
            response = await client!.PostAsJsonAsync("api/companies", duplicateDto);
            Assert.AreEqual(StatusCodes.Status409Conflict, ((int)response.StatusCode));

            CleanUpEntries(new List<string>(new string[] { companyName }));

        }



        [TestMethod]
        public async Task Company_GetAll()
        {
            var companiesFromDb = await phoneBookContext!.Companies.Select(c => c).ToListAsync();
            var companiesRes = await client!.GetFromJsonAsync<List<Company>>("api/companies");
            Assert.IsNotNull(companiesRes);
            Assert.AreEqual(companiesRes.Count(), companiesFromDb.Count());
            foreach (var company in companiesRes)
            {
                Assert.IsTrue(companiesFromDb.Exists(c => c.CompanyName == company.CompanyName && c.RegistrationDate == c.RegistrationDate));
                var companyPeopleCountFromDb = await phoneBookContext.People.Where(p => p.CompanyName == company.CompanyName).CountAsync();
                Assert.AreEqual(companyPeopleCountFromDb, company.PeopleCount);
            }


        }







    }
}