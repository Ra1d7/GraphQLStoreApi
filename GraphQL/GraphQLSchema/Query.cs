using Dapper;
using GraphQL.DataAccess;
using GraphQL.Models;
using System.Text.Json;
using static GraphQL.Models.Enums;

namespace GraphQL.GraphQLSchema;

public class Query
{
    //A few queries with fake data to test the API

    public List<Models.Person> GetPeople(int num = 10) => FakeDataGenerator.GenPeople(num);
    public List<Customer> GetCustomers(int num = 10) => FakeDataGenerator.GenCustomers(num);
    public List<Employee> GetEmployees(int num = 10) => FakeDataGenerator.GenEmployees(num);
    public List<Item> GetItems(int num = 10) => FakeDataGenerator.GenItems(num);
}
