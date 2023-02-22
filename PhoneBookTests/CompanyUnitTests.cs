using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using PhoneBook.DTOs;
using PhoneBook.Model;
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
                    if (((short)x.Name.ToCharArray()[0]) > ((short)y.Name.ToCharArray()[0]))
                    {
                        return 1;
                    }
                    else if (((short)x.Name.ToCharArray()[0]) < ((short)y.Name.ToCharArray()[0]))
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
            companies = new List<string>(new string[] { "Amazon", "Google", "Meta", "Tesla" });
            string json = File.ReadAllText("PEOPLE_TEST_DATA.json");
            people = JsonConvert.DeserializeObject<List<NewPersonDTO>>(json);
            createdPersons = new List<NewPersonDTO>();

            var postTasks = new List<Task>();
            int i = 0;
            foreach (var company in companies!)
            {
                people![i].CompanyName = company;
                people![i + 1].CompanyName = company;
                client!.PostAsJsonAsync("api/people", people![i]).Wait();
                client!.PostAsJsonAsync("api/people", people![i+1]).Wait();

                var companyPeople = new List<NewPersonDTO>(new NewPersonDTO[] { people![i], people![i + 1] });
                i += 2;
                postTasks.Add(client.PostAsJsonAsync("api/companies", new NewCompanyDTO { CompanyName = company, RegistrationDate = DateTime.Today.ToString(), people = companyPeople }));
                createdPersons.AddRange(companyPeople.ToArray());
            }

            Task.WaitAll(postTasks.ToArray());
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            factory!.Dispose();
            client!.Dispose();

        }

        private static NewPersonDTO GetRandomPerson()
        {
            var index = (new Random()).Next(people!.Count);
            return people.ElementAt(index);
        }

        [TestMethod]
        public async Task Company_Add()
        {

            var client = factory!.CreateClient();
            var companies = await client.GetFromJsonAsync<List<Company>>("api/companies");
            Assert.IsNotNull(companies);
            var companiesCountBefore = companies.Count;
            var postedDto = new NewCompanyDTO { CompanyName = CompanyUnitTests.companies!.First(), RegistrationDate = DateTime.Today.ToString() };
            var response = await client.PostAsJsonAsync("api/companies", postedDto);
            Assert.AreEqual(StatusCodes.Status201Created, ((int)response.StatusCode));
            companies = await client.GetFromJsonAsync<List<Company>>("api/companies");
            Assert.IsNotNull(companies);
            Assert.AreEqual(companies.Count - companiesCountBefore, 1);
            Assert.AreEqual(companies.Where(c => c.Name == postedDto.CompanyName && c.registrationDate.Equals(DateTime.Parse(postedDto.RegistrationDate))).Select(c => c).ToList().Count,1);

        }



        [TestMethod]
        public async Task Company_GetAll()
        {
            var companiesRes = await client.GetFromJsonAsync<List<Company>>("api/companies");
            Assert.IsNotNull(companiesRes);
            Assert.AreEqual(companiesRes.Count(), companies.Count);
            foreach (var company in companiesRes)
            {
                Assert.AreEqual(company.PersonCount, 2);
            }

            companiesRes.Sort(comparatorCompanies);
            for (int j = 0; j < companiesRes.Count; j++)
            {
                Assert.AreEqual(companiesRes[j].Name, companies[j]);
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
                var foundPersons = peopleRes.Where(p => p.Address == personDto.Address && p.FullName == personDto.FullName && p.PhoneNumber == personDto.PhoneNumber && p.Company?.Name == personDto.CompanyName).ToList();
                Assert.AreEqual(foundPersons.Count, 1);
            }


        }




    }
}