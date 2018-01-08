using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevStackCore.OrientDb;
using RevStackCore.Pattern;
using RevStackCore.Extensions;
using System.Linq;

namespace UnitTestOrientDb
{
    [TestClass]
    public class UnitTest1
    {
        const string CONNECTION_STRING = "server=http://localhost:2480;database=testgraph;user=admin;password=admin";

        OrientDbContext dbContext;
        IRepository<Person, string> personRepository;

        public UnitTest1()
        {
            dbContext = new OrientDbContext(CONNECTION_STRING);
            personRepository = new OrientDbRepository<Person, string>(dbContext);
        }

        [TestMethod]
        public void CRUD()
        {
            // Create
            var person = new Person
            {
                Age = 33,
                FirstName = "Jane",
                LastName = "Doe"
            };
            person = personRepository.Add(person);
            // Get by id
            person = personRepository.GetById(person.Id);

            

            // Update
            person.Age = 65;
            person = personRepository.Update(person);
            // Delete
            personRepository.Delete(person);
        }

        [TestMethod]
        public void Queryable()
        {
            var userName = "Jane";
            var user = personRepository.Find(x => x.Compare(x.FirstName, userName));
            int count = user.Count();
            //var user = personRepository.Find(x => x.FirstName == userName).ToSingleOrDefault();
            Assert.AreNotEqual(null, user);
            //var query = personRepository.Find(x=>x.Age == 65);
            //var users = query.ToList();
            //Assert.AreNotEqual(0, users.Count);
        }

    }


}
