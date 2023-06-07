using static GraphQL.Models.Enums;
using System.Text.Json;
using GraphQL.DataAccess;
using Dapper;
using GraphQL.Models;
using Microsoft.IdentityModel.Tokens;

namespace GraphQL.Mutations
{
    public class Mutation
    {
        private readonly ILogger<Mutation> _logger;
        private readonly DapperContext _context;

        public Mutation(ILogger<Mutation> logger, DapperContext context)
        {
            _logger = logger;
            _context = context;
        }
        // ------------ Data Transfer Objects ------------
        public record RegisterPersonDTO(string Name, string Email, string Password, int Age, Gender Gender);
        public record RegisterCustomernDTO(string Name, string Email, string Password, int Age, Gender Gender, bool HasPremium = false, string ShippingAddress = "");
        public record RegisterEmployeeDTO(string Name, string Email, string Password, int Age, Gender Gender, decimal Salary, Department Department);
        public record AddItemDTO(string Name,decimal Price , string Description , int Quantity , bool IsAvaliable , string Category);

        // ------------ Methods ------------

        /// <summary>
        /// Registers a new customer into the database
        /// </summary>
        /// <param name="customer"></param>
        /// <returns>Registeration status</returns>
        public async Task<string> RegisterCustomer(RegisterCustomernDTO customer)
        {
            _logger.LogInformation($"Registering a new Customer with name {customer.Name}");
            using var connection = _context.CreateConnection();
            //register person if new
            int personid = await RegisterPerson(new(customer.Name, customer.Email, customer.Password, customer.Age, customer.Gender));
            if (personid == -1) return "Email Already Exists! please login.";

            //register customer
            int rows = await connection.ExecuteAsync("INSERT INTO Customers(PersonId,ShippingAddress,HasPremiumMembership) VALUES (@PersonId,@ShippingAddress,@HasPremium)", new
            {
                PersonId = personid,
                ShippingAddress = customer.ShippingAddress,
                HasPremium = customer.HasPremium
            });
            return rows > 0 ? $"Successfully Registered! customer with id of {personid}" : $"An error has occured while registering person \n{JsonSerializer.Serialize(customer)}";
        }
        /// <summary>
        /// Registers a new employee into the database
        /// </summary>
        /// <param name="employee"></param>
        /// <returns>Registeration status</returns>
        public async Task<string> RegisterEmployee(RegisterEmployeeDTO employee)
        {
            _logger.LogInformation($"Registering a new Employee with name {employee.Name} and salary of {employee.Salary}");
            using var connection = _context.CreateConnection();
            //register person if new
            int personid = await RegisterPerson(new(employee.Name, employee.Email, employee.Password, employee.Age, employee.Gender));
            if (personid == -1) return "Email Already Exists! please login.";
            //register employee
            int rows = await connection.ExecuteAsync("INSERT INTO Employee(PersonId,Salary,DepartmentId) VALUES (@PersonId,@Salary,@Department)", new
            {
                PersonId = personid,
                Salary = employee.Salary,
                Department = employee.Department
            });
            return rows > 0 ? $"Successfully Registered! employee with id of {personid}" : $"An error has occured while registering person \n{JsonSerializer.Serialize(employee)}";
        }

        /// <summary>
        /// Adds a new category for items in the database
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<string> AddACategory(string category)
        {
            _logger.LogInformation($"Adding a new Category {category}");
            using var connection = _context.CreateConnection();
            int rows = await connection.ExecuteAsync("INSERT INTO Categories VALUES (@category)", new {category });
            return rows > 0 ? "Sucessfully added!" : "An error has occured while adding a new category";
        }

        /// <summary>
        /// Adds a new item into the database
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<string> AddAnItem(AddItemDTO item)
        {
            _logger.LogInformation($"Adding a new item {item.Name} with a price of {item.Price}");
            using var connection = _context.CreateConnection();
            int? CategoryId = (await connection.QueryAsync<int>("SELECT Id FROM Categories WHERE Name = @Category", new { Category = item.Category })).FirstOrDefault();
            if (CategoryId == null) return "Category Doesn't Exist!";
            if (item.Price <= 0 || item.Name.IsNullOrEmpty() || item.Description.IsNullOrEmpty()) return "Item parameters are not valid!";
            int rows = await connection.ExecuteAsync("INSERT INTO Item (Name,Price,Description,Quantity,IsAvaliable,CategoryId)" +
                " VALUES (@Name,@Price,@Description,@Quantity,@IsAvaliable,@CategoryId)",
                new
                {
                    Name = item.Name,
                    Price = item.Price,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    IsAvaliable = item.IsAvaliable,
                    Categoryid = CategoryId
                });
            return rows > 0 ? "Successfully added!" : "An error occured while adding item";
        }



        /// <summary>
        /// Clears the Person table in the database , leading to a cascade deleting all other entries in other tables.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ClearPersonTable()
        {
            _logger.LogInformation("Clearing Person's table!");
            using var connection = _context.CreateConnection();
            int rows = await connection.ExecuteAsync("DELETE FROM Person");
            return rows > 0;
        }
        /// <summary>
        /// Delete a person from the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAPerson(int id)
        {
            _logger.LogInformation($"Deleting person with id {id}");
            using var connection = _context.CreateConnection();
            int rows = await connection.ExecuteAsync("DELETE FROM Person WHERE Id = @id", new { id });
            return rows > 0;
        }


        // ------------ Helper Methods ------------
        private async Task<bool> CheckIfPersonExists(string Email)
        {
            using var connection = _context.CreateConnection();
            return (await connection.QueryAsync<int>("SELECT COUNT(*) FROM Person WHERE Email = @Email", new { Email })).FirstOrDefault() > 0 ? true : false;
        }
        private async Task<bool> SaveLoginDetails(int id, string Password)
        {
            using var connection = _context.CreateConnection();
            int rows = await connection.ExecuteAsync("INSERT INTO Login(PersonId,Password) VALUES (@PersonId,@Password)", new { PersonId = id, Password = Password });
            return rows > 0;
        }
        private async Task<int> RegisterPerson(RegisterPersonDTO person)
        {
            int personid = -1;
            if (await CheckIfPersonExists(person.Email)) return -1; //check if user already exists
            using var connection = _context.CreateConnection();
            int rows = await connection.ExecuteAsync("INSERT INTO Person(Name,Email,Age,Gender) VALUES (@Name,@Email,@Age,@Gender)", person);
            personid = (await connection.QueryAsync<int>("SELECT Id FROM Person WHERE Email = @Email", new { Email = person.Email })).FirstOrDefault();
            if (!await SaveLoginDetails(personid, person.Password)) return -1;
            return personid;
        }
    }
}
