using Bogus;
using Dapper;
using GraphQL.DataAccess;
using GraphQL.Models;

namespace GraphQL.GraphQLSchema;

public class Query
{
    private readonly ILogger<Query> _logger;
    private readonly DapperContext _context;

    public Query(ILogger<Query> logger, DapperContext context)
    {
        _logger = logger;
        _context = context;
    }
    //A few queries with fake data from Bogus to test the API

    public List<Models.Person> GetExamplePeople(int num = 10) => FakeDataGenerator.GenPeople(num);
    public List<Customer> GetExampleCustomers(int num = 10) => FakeDataGenerator.GenCustomers(num);
    public List<Employee> GetExampleEmployees(int num = 10) => FakeDataGenerator.GenEmployees(num);
    public List<Item> GetExampleItems(int num = 10) => FakeDataGenerator.GenItems(num);


    // Database Queries

    public async Task<IEnumerable<Models.Person>> GetPeople()
    {
        _logger.LogInformation("Getting Database People");
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Models.Person>("SELECT * FROM Person");
    }

    public async Task<IEnumerable<Employee>> GetEmployees(int num = 10)
    {
        _logger.LogInformation("Getting Database Employees");

        using var connection = _context.CreateConnection();
        var query = @"SELECT e.Id AS id, e.Salary, e.DepartmentId AS Department,
                     p.Name, p.Email, p.JoinDate, p.Age, p.Gender
                     FROM Employee e
                     JOIN Person p ON e.Id = p.Id";

        IEnumerable<Employee> emps = await connection.QueryAsync<Employee>(query);

        return emps.Take(num);
    }   
    
    public async Task<IEnumerable<Customer>> GetCustomers(int num = 10)
    {
        _logger.LogInformation("Getting Database Customers");
        using var connection = _context.CreateConnection();
        var query = @"SELECT c.Id AS id, c.ShippingAddress, c.HasPremiumMembership,
                     p.Name, p.Email, p.JoinDate, p.Age, p.Gender
                     FROM Customers c
                     JOIN Person p ON c.Id = p.Id";
        IEnumerable<Customer> customers = await connection.QueryAsync<Customer>(query);
        return customers.Take(num);
    } 
    
    public async Task<IEnumerable<Item>> GetItems(int num = 10)
    {
        _logger.LogInformation("Getting Database Items");
        using var connection = _context.CreateConnection();
        var query = @"SELECT Id, Price,   Description, [Name], Quantity Qtn, IsAvaliable, categoryid FROM Item";
        IEnumerable<Item> items = await connection.QueryAsync<Item>(query);
        foreach (var item in items)
        {
            var category = (await connection.QueryAsync<Category>("SELECT * FROM Categories WHERE Id = @CategoryId", new { CategoryId = item.CategoryId })).FirstOrDefault();
            item.Category = category ?? throw new GraphQLException("Category doesn't exist");
        }
        return items.Take(num);
    }  
    public async Task<IEnumerable<Category>> GetCategories(int num = 10)
    {
        _logger.LogInformation("Getting Database Categories");
        using var connection = _context.CreateConnection();
        var query = @"SELECT * From Categories";
        IEnumerable<Category> categories = await connection.QueryAsync<Category>(query);
        return categories.Take(num);
    }
}
