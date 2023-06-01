using Dapper;
using GraphQL.DataAccess;
using GraphQL.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using static GraphQL.Models.Enums;

namespace GraphQL.GraphQLSchema
{
    public class Query
    {
        private readonly DapperContext _context;
        private readonly ILogger<Query> _logger;

        public Query(DapperContext context , ILogger<Query> logger)
        {
            _context = context;
            _logger = logger;
        }
        //A few queries with fake data to test the API

        public List<Person> GetPeople(int num = 10) => FakeDataGenerator.GenPeople(num);
        public List<Customer> GetCustomers(int num = 10) => FakeDataGenerator.GenCustomers(num);
        public List<Employee> GetEmployees(int num = 10) => FakeDataGenerator.GenEmployees(num);
        public List<Item> GetItems(int num = 10) => FakeDataGenerator.GenItems(num);






        // ------------ Data Transfer Objects ------------
        public record RegisterPersonDTO(string Name, string Email , int Age , Gender Gender = 0);


        // ------------ Methods ------------

        /// <summary>
        /// Registers a new person into the database , this can be a presequite to adding a customer or employee
        /// </summary>
        /// <param name="person"></param>
        /// <returns>Registeration status</returns>
        public async Task<string> RegisterPerson(RegisterPersonDTO person)
        {
            _logger.LogInformation($"Registering a new person with name {person.Name}");
            using(var connection = _context.CreateConnection())
            {
                //check if user already exists
                bool didAlreadyRegister = (await connection.QueryAsync<int>("SELECT COUNT(*) FROM Person WHERE Email = @Email", new { Email = person.Email })).FirstOrDefault() > 0 ? true : false;
                if (didAlreadyRegister) return "Email already exists! , please login.";
                //register user if new
                int rows = await connection.ExecuteAsync("INSERT INTO Person(Name,Email,Age,Gender) VALUES (@Name,@Email,@Age,@Gender)",person);
                return rows > 0 ? "Successfully Registered!" : $"An error has occured while registering person \n{JsonSerializer.Serialize(person)}";
            }
        }
    }
}
