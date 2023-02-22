using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PhoneBook.Data;
using PhoneBook.DTOs;
using PhoneBook.Models;
using System.Net.Http.Json;

namespace PhoneBookTests
{

    [TestClass]
    public class CompanyUnitTests { 
        private static int comparatorPersons(Person x, Person y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal.
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater.
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    if (((short)x.FullName.ToCharArray()[0]) > ((short)y.FullName.ToCharArray()[0]))
                    {
                        return 1;
                    }
                    else if (((short)x.FullName.ToCharArray()[0]) < ((short)y.FullName.ToCharArray()[0]))
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }
        private static int comparatorCompanies(Company x, Company y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal.
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater.
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    if (((short)x.CompanyName.ToCharArray()[0]) > ((short)y.CompanyName.ToCharArray()[0]))
                    {
                        return 1;
                    }
                    else if (((short)x.CompanyName.ToCharArray()[0]) < ((short)y.CompanyName.ToCharArray()[0]))
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }

        private static WebApplicationFactory<Program>? factory;
        private static List<string>? companies;
        private static List<NewPersonDTO>? people;
        private static List<NewPersonDTO> createdPersons;
        private static HttpClient? client;
        private static PhoneBookContext phoneBookContext;

        [TestInitialize()]
        public void Startup(){
            
        }

        [TestCleanup()]
        public void Cleanup(){ 
            
            
        }

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            factory = new WebApplicationFactory<Program>();
            client = factory!.CreateClient();
            companies = new List<string>(new string[] { "Quatz", "Topiczoom", "Eire", "Shinra" });
            phoneBookContext = new PhoneBookContext();
            //string json = File.ReadAllText("PEOPLE_TEST_DATA.json");
            //people = JsonConvert.DeserializeObject<List<NewPersonDTO>>(json);
            //createdPersons = new List<NewPersonDTO>();

            //var postTasks = new List<Task>();
            //int i = 0;
            //foreach (var company in companies!)
            //{
            //    people![i].CompanyName = company;
            //    people![i + 1].CompanyName = company;
            //    client!.PostAsJsonAsync("api/people", people![i]).Wait();
            //    client!.PostAsJsonAsync("api/people", people![i+1]).Wait();

            //    var companyPeople = new List<NewPersonDTO>(new NewPersonDTO[] { people![i], people![i + 1] });
            //    i += 2;
            //    postTasks.Add(client.PostAsJsonAsync("api/companies", new NewCompanyDTO { CompanyName = company, RegistrationDate = DateTime.Today.ToString(), people = companyPeople }));
            //    createdPersons.AddRange(companyPeople.ToArray());
            //}

            //Task.WaitAll(postTasks.ToArray());
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            factory!.Dispose();
            client!.Dispose();
            phoneBookContext.Dispose();

        }

        private static NewPersonDTO GetRandomPerson()
        {
            var index = (new Random()).Next(people!.Count);
            return people.ElementAt(index);
        }

        [TestMethod]
        public async Task Company_Add()
        {

            var companies = await client!.GetFromJsonAsync<List<Company>>("api/companies");
            Assert.IsNotNull(companies);
            var companiesCountBefore = companies.Count;
            var companyName = $"TEST{(new Random()).Next()}";
            var postedDto = new NewCompanyDTO { CompanyName = companyName, RegistrationDate = DateTime.Today.ToString() };
            var response = await client!.PostAsJsonAsync("api/companies", postedDto);
            Assert.AreEqual(StatusCodes.Status201Created, ((int)response.StatusCode));
            companies = await client.GetFromJsonAsync<List<Company>>("api/companies");
            Assert.IsNotNull(companies);
            Assert.AreEqual(companies.Count - companiesCountBefore, 1);
            Assert.AreEqual(companies.Where(c => c.CompanyName == postedDto.CompanyName && c.RegistrationDate.Equals(DateTime.Parse(postedDto.RegistrationDate))).Select(c => c).ToList().Count,1);


            //TEST error condition
            var duplicateDto = new NewCompanyDTO { CompanyName = "Tesla", RegistrationDate = DateTime.Today.ToString() };
            response = await client.PostAsJsonAsync("api/companies", duplicateDto);
            Assert.AreEqual(StatusCodes.Status409Conflict, ((int)response.StatusCode));

        }



        [TestMethod]
        public async Task Company_GetAll()
        {
            var companiesFromDb = await phoneBookContext.Companies.Select(c=>c).ToListAsync();
            var companiesRes = await client!.GetFromJsonAsync<List<Company>>("api/companies");
            Assert.IsNotNull(companiesRes);
            Assert.AreEqual(companiesRes.Count(), companiesFromDb.Count());
            foreach (var company in companiesRes)
            {
                Assert.IsTrue(companiesFromDb.Exists(c => c.CompanyName == company.CompanyName && c.RegistrationDate == c.RegistrationDate));
                var companyPeopleCountFromDb = await phoneBookContext.People.Where(p => p.CompanyName == company.CompanyName).CountAsync();
                Assert.AreEqual(companyPeopleCountFromDb,company.PeopleCount);
            }


        }

        [TestMethod]
        public async Task Person_Add()
        {
            var peopleRes = await client!.GetFromJsonAsync<List<Person>>("api/People");
            Assert.IsNotNull(peopleRes);
            var peopleCountBefore = peopleRes.Count;
            NewPersonDTO newPerson = GetRandomPerson();

            var response = await client!.PostAsJsonAsync("api/people", new NewPersonDTO { Address = newPerson.Address, CompanyName = companies!.First(), FullName = newPerson.FullName, PhoneNumber = newPerson.PhoneNumber });
            Assert.AreEqual(StatusCodes.Status201Created, ((int)response.StatusCode));
            peopleRes = await client!.GetFromJsonAsync<List<Person>>("api/people");
            Assert.IsNotNull(peopleRes);
            Assert.IsTrue(peopleRes.Count - peopleCountBefore == 1);
        }

        [TestMethod]
        public async Task Person_GetAll()
        {

            var peopleRes = await client!.GetFromJsonAsync<List<Person>>("api/people");
            Assert.IsNotNull(peopleRes);
            Assert.AreEqual(peopleRes.Count, createdPersons.Count);

            foreach (var personDto in createdPersons!)
            {
                var foundPersons = peopleRes.Where(p => p.Address == personDto.Address && p.FullName == personDto.FullName && p.PhoneNumber == personDto.PhoneNumber && p.CompanyName == personDto.CompanyName).ToList();
                Assert.AreEqual(foundPersons.Count, 1);
            }


        }




    }
}