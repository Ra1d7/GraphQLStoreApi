using static GraphQL.Models.Enums;
using System.Text.Json;
using GraphQL.DataAccess;
using Dapper;

namespace GraphQL.Mutations
{
    public class Mutation
    {
        private readonly ILogger<Mutation> _logger;
        private readonly DapperContext _context;

        public Mutation(ILogger<Mutation> logger , DapperContext context)
        {
            _logger = logger;
            _context = context;
        }
        // ------------ Data Transfer Objects ------------
        public record RegisterPersonDTO(string Name, string Email, string Password, int Age, Gender Gender);
        public record RegisterCustomernDTO(string Name, string Email, string Password, int Age, Gender Gender, bool HasPremium = false, string ShippingAddress = "");
        public record RegisterEmployeeDTO(string Name, string Email, string Password, int Age, Gender Gender, decimal Salary, Department Department);

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
        //Don't even worry about it :)
        public async Task<bool> ClearPersonTable()
        {
            using var connection = _context.CreateConnection();
            int rows = await connection.ExecuteAsync("DELETE FROM Person");
            return rows > 0;
        }


        // ------------ Helper Methods ------------
        private async Task<bool> CheckIfPersonExists(string Email)
        {
            using var connection = _context.CreateConnection();
            return (await connection.QueryAsync<int>("SELECT COUNT(*) FROM Person WHERE Email = @Email", new { Email = Email })).FirstOrDefault() > 0 ? true : false;
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
