using Dapper;
using GraphQL.DataAccess;
using GraphQL.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using static GraphQL.Models.Enums;

namespace GraphQL.Mutations;

public class Mutation
{
    private readonly ILogger<Mutation> _logger;
    private readonly DapperContext _context;

    public Mutation(ILogger<Mutation> logger, DapperContext context)
    {
        _logger = logger;
        _context = context;
    }

    #region Dtos
    // ------------ Data Transfer Objects ------------
    public record RegisterPersonDTO(string Name, string Email, string Password, int Age, Gender Gender);
    public record RegisterCustomernDTO(string Name, string Email, string Password, int Age, Gender Gender, bool HasPremium = false, string ShippingAddress = "");
    public record RegisterEmployeeDTO(string Name, string Email, string Password, int Age, Gender Gender, decimal Salary, Department Department);
    public record EditCustomerDTO(string? Name, string? Email, string? Password, int? Age, Gender? Gender, bool? HasPremiumMemberShip , string? ShippingAddress);
    public record EditEmployeeDTO(string? Name, string? Email, string? Password, int? Age, Gender? Gender, decimal? Salary, Department? Department);
    public record AddItemDTO(string Name, decimal Price, string Description, int Quantity, bool IsAvaliable, string Category);
    public record EditItemDTO(string? Name, decimal? Price, string? Description, int? Quantity, bool? IsAvaliable, int? CategoryId);
    #endregion
    #region Methods

    // ------------ Methods ------------

    #region Creation
    // ======> Creation
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
        int Id = await RegisterPerson(new(customer.Name, customer.Email, customer.Password, customer.Age, customer.Gender));
        if (Id == -1) return "Email Already Exists! please login.";

        //register customer
        int rows = await connection.ExecuteAsync("INSERT INTO Customers(Id,ShippingAddress,HasPremiumMembership) VALUES (@Id,@ShippingAddress,@HasPremium)", new
        {
            Id = Id,
            ShippingAddress = customer.ShippingAddress,
            HasPremium = customer.HasPremium
        });
        return rows > 0 ? $"Successfully Registered! customer with id of {Id}" : $"An error has occured while registering person \n{JsonSerializer.Serialize(customer)}";
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
        int Id = await RegisterPerson(new(employee.Name, employee.Email, employee.Password, employee.Age, employee.Gender));
        if (Id == -1) return "Email Already Exists! please login.";
        //register employee
        int rows = await connection.ExecuteAsync("INSERT INTO Employee(Id,Salary,DepartmentId) VALUES (@Id,@Salary,@Department)", new
        {
            Id = Id,
            Salary = employee.Salary,
            Department = employee.Department
        });
        return rows > 0 ? $"Successfully Registered! employee with id of {Id}" : $"An error has occured while registering person \n{JsonSerializer.Serialize(employee)}";
    }

    public async Task<string> AddACategory(string category)
    {
        _logger.LogInformation($"Adding a new Category {category}");
        using var connection = _context.CreateConnection();
        int rows = await connection.ExecuteAsync("INSERT INTO Categories VALUES (@category)", new { category });
        return rows > 0 ? "Sucessfully added!" : "An error has occured while adding a new category";
    }

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
    #endregion
    #region Updating
    // ======> Updating

    /// <summary>
    /// Provides dynamic editing of an item with the ability to only specifiy the attributes you'd like to edit and the other attributes will stay the same in the database
    /// </summary>
    /// <param name="id"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public async Task<string> EditAnItem(int id, EditItemDTO item)
    {
        _logger.LogInformation($"Editing an item with id {id} with a price of");
        using var connection = _context.CreateConnection();
        bool result = await EditAnything(id, "Item", item);
        return result  ? "Successfully edited!" : "An error occured while editing an item";
    }
    public async Task<string> UpdateEmployee(int id, EditEmployeeDTO employee)
    {
        bool PersonResult = await EditAnything(id, "Person", new { employee.Name, employee.Email, employee.Age, employee.Gender });
        bool EmpResult = await EditAnything(id, "Employee", new {employee.Salary,employee.Department });
        return (PersonResult || EmpResult) ? "Successfully edited!" : "Cannot edit Employee";
    }
    public async Task<string> UpdateCustomer(int id,EditCustomerDTO customer)
    {
        bool PersonResult = await EditAnything(id, "Person", new { customer.Name, customer.Email, customer.Age, customer.Gender });
        bool EmpResult = await EditAnything(id, "Customers", new { customer.ShippingAddress, customer.HasPremiumMemberShip });
        return (PersonResult || EmpResult) ? "Successfully edited!" : "Cannot edit Employee";
    }
    public async Task<string> EditACategory(int id, string category)
    {
        _logger.LogInformation($"Editing category with id {id}");
        using var connection = _context.CreateConnection();
        int rows = await connection.ExecuteAsync("UPDATE Categories SET Name = @category WHERE id = @id", new { category, id });
        return rows > 0 ? "Successfully edited!" : "An error has occured while editing a category";
    }
    #endregion
    #region Deletion
    // ======> Deletion

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
    public async Task<bool> DeleteAPerson(int id)
    {
        _logger.LogInformation($"Deleting person with id {id}");
        using var connection = _context.CreateConnection();
        int rows = await connection.ExecuteAsync("DELETE FROM Person WHERE Id = @id", new { id });
        return rows > 0;
    }
    public async Task<bool> DeleteAnItem(int id)
    {
        _logger.LogInformation($"Deleting item with id {id}");
        using var connection = _context.CreateConnection();
        int rows = await connection.ExecuteAsync("DELETE FROM Item WHERE Id = @id", new { id });
        return rows > 0;
    }
    public async Task<bool> DeleteACategory(int id)
    {
        _logger.LogInformation($"Deleting category with id {id}");
        using var connection = _context.CreateConnection();
        int rows = await connection.ExecuteAsync("DELETE FROM Categories WHERE Id = @id", new { id });
        return rows > 0;
    }

    #endregion
    #region Helpers
    // ------------ Helper Methods ------------
    private async Task<bool> CheckIfPersonExists(string Email)
    {
        using var connection = _context.CreateConnection();
        return (await connection.QueryAsync<int>("SELECT COUNT(*) FROM Person WHERE Email = @Email", new { Email })).FirstOrDefault() > 0 ? true : false;
    }
    private async Task<bool> SaveLoginDetails(int id, string Password)
    {
        using var connection = _context.CreateConnection();
        int rows = await connection.ExecuteAsync("INSERT INTO Login(Id,Password) VALUES (@Id,@Password)", new { Id = id, Password = Password });
        return rows > 0;
    }
    private async Task<int> RegisterPerson(RegisterPersonDTO person)
    {
        int Id = -1;
        if (await CheckIfPersonExists(person.Email)) return -1; //check if user already exists
        using var connection = _context.CreateConnection();
        int rows = await connection.ExecuteAsync("INSERT INTO Person(Name,Email,Age,Gender) VALUES (@Name,@Email,@Age,@Gender)", person);
        Id = (await connection.QueryAsync<int>("SELECT Id FROM Person WHERE Email = @Email", new { Email = person.Email })).FirstOrDefault();
        if (!await SaveLoginDetails(Id, person.Password)) return -1;
        return Id;
    }
    /// <summary>
    /// Provides dynamic editing of any entry in a table with the ability to only specifiy the attributes you'd like to edit and the other attributes will stay the same in the database
    /// </summary>
    /// <param name="id">the id of the thing</param>
    /// <param name="Table">the table to edit</param>
    /// <param name="objecttoEdit">the object parameters you'd like to edit</param>
    /// <returns></returns>
    /// <exception cref="GraphQLException"></exception>
    private async Task<bool> EditAnything(int id,string Table, object objecttoEdit)
    {
        Dictionary<string, object> Properties = new();
        objecttoEdit.GetType().GetProperties().Where(p => p.GetValue(objecttoEdit) != null).ToList().ForEach(p => Properties.Add(p.Name, p.GetValue(objecttoEdit)!));
        foreach(var property in Properties)
        {
            if (property.Value.GetType() == typeof(string) && property.Value.ToString().IsNullOrEmpty())
                throw new GraphQLException($"{property.Key} cannot be empty!");
            if (property.Value.GetType() == typeof(int) && (int)property.Value <= 0)
                throw new GraphQLException($"{property.Key} cannot be less than zero!");  
            if (property.Value.GetType() == typeof(decimal) && (decimal)property.Value <= 0)
                throw new GraphQLException($"{property.Key} cannot be less than zero!");
        }
        using var connection = _context.CreateConnection();
        int rows = 0;
        foreach (var prop in Properties)
        {
            rows += await connection.ExecuteAsync($"UPDATE {Table} SET {prop.Key} = @Value WHERE id = @id",
                new
                {
                    Value = prop.Value,
                    id = id
                });
        }
        return rows > 0;
    }
    #endregion
    #endregion


}
