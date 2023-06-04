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
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Models.Person>("SELECT * FROM Person");
    }

    public async Task<IEnumerable<Employee>> GetEmployees(int num = 10)
    {
        using var connection = _context.CreateConnection();
        var query = @"SELECT e.PersonId AS id, e.Salary, e.DepartmentId AS Department,
                     p.Name, p.Email, p.JoinDate, p.Age, p.Gender
                     FROM Employee e
                     JOIN Person p ON e.Personid = p.Id";

        IEnumerable<Employee> emps = await connection.QueryAsync<Employee>(query);

        return emps.Take(num);
    }   
    
    public async Task<IEnumerable<Customer>> GetCustomers(int num = 10)
    {
        using var connection = _context.CreateConnection();
        var query = @"SELECT c.PersonId AS id, c.ShippingAddress, c.HasPremiumMembership,
                     p.Name, p.Email, p.JoinDate, p.Age, p.Gender
                     FROM Customers c
                     JOIN Person p ON c.Personid = p.Id";
        IEnumerable<Customer> customers = await connection.QueryAsync<Customer>(query);

        return customers.Take(num);
    }
}
