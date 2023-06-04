using Bogus;
using GraphQL.Models;
using System;
using static GraphQL.Models.Enums;

namespace GraphQL.DataAccess
{
    public static class FakeDataGenerator
    {
        public static List<Models.Person> GenPeople(int num = 1)
        {
            List<Models.Person> people = new List<Models.Person>();
            do
            {
                people.Add(GenPerson());
                num--;

            } while (num > 0);
            return people;
        }
        public static List<Employee> GenEmployees(int num = 1)
        {
            List<Employee> employees = new List<Employee>();
            do
            {
                employees.Add(GenEmployee());
                num--;

            } while (num > 0);
            return employees;
        }
        public static List<Customer> GenCustomers(int num = 1)
        {
            List<Customer> customers = new List<Customer>();
            do
            {
                customers.Add(GenCustomer());
                num--;
            } while (num > 0);
            return customers;
        }
        public static List<Item> GenItems(int num = 1)
        {
            List<Item> items = new List<Item>();
            do
            {
                items.Add(GenItem());
                num--;
            } while (num > 0);
            return items;
        }
        private static Models.Person GenPerson()
        {
            return new Faker<Models.Person>()
                .RuleFor(p => p.Id, f => f.Random.Int(1, 20000))
                .RuleFor(p => p.Gender, f => f.PickRandom<Gender>())
                .RuleFor(p => p.Name, f => f.Name.FirstName())
                .RuleFor(p => p.Email, f => f.Internet.Email())
                .RuleFor(p => p.JoinDate, f => f.Date.Between(DateTime.Now, DateTime.Now.AddYears(-3)))
                .RuleFor(p => p.Age, f => f.Random.Int(18, 56));
        }
        private static Employee GenEmployee()
        {
            return new Faker<Employee>()
                .RuleFor(p => p.Id, f => f.Random.Int(1, 20000))
                .RuleFor(p => p.Name, f => f.Name.FirstName())
                .RuleFor(p => p.Email, f => f.Internet.Email())
                .RuleFor(p => p.JoinDate, f => f.Date.Between(DateTime.Now, DateTime.Now.AddYears(-3)))
                .RuleFor(p => p.Age, f => f.Random.Int(18, 56))
                .RuleFor(p => p.Gender, f => f.PickRandom<Gender>())
                .RuleFor(e => e.Salary, f => f.Random.Decimal(300, 3000))
                .RuleFor(e => e.Department, f => f.PickRandom<Department>());
        }
        private static Customer GenCustomer()
        {
            return new Faker<Customer>()
                .RuleFor(p => p.Id, f => f.Random.Int(1, 20000))
                .RuleFor(p => p.Name, f => f.Name.FirstName())
                .RuleFor(p => p.Email, f => f.Internet.Email())
                .RuleFor(p => p.JoinDate, f => f.Date.Between(DateTime.Now, DateTime.Now.AddYears(-3)))
                .RuleFor(p => p.Age, f => f.Random.Int(18, 56))
                .RuleFor(p => p.Gender, f => f.PickRandom<Gender>())
                .RuleFor(p => p.ShippingAddress, f => f.Address.FullAddress())
                .RuleFor(p => p.HasPremiumMembership, f => f.Random.Bool());
        }
        private static Category GenCategory()
        {
            return new Faker<Category>()
                .RuleFor(c => c.Id, f => f.Random.Int(1, 100))
                .RuleFor(c => c.Name, f => f.Random.String());
        }
        private static Item GenItem()
        {
            return new Faker<Item>()
                .RuleFor(i => i.ItemId, f => f.Random.Int(1000, 9999))
                .RuleFor(i => i.Price, f => f.Random.Decimal(5, 500,3))
                .RuleFor(i => i.Description, f => f.Lorem.Sentences(4))
                .RuleFor(i => i.Name, f => f.Lorem.Sentence(3))
                .RuleFor(i => i.Qtn, f => f.Random.Int(5, 25))
                .RuleFor(i => i.IsAvailble, f => f.Random.Bool())
                .RuleFor(i => i.Category, f => GenCategory());
        }
        public static decimal Decimal(this Randomizer r, decimal min = 0.0m, decimal max = 1.0m, int? decimals = null)
        {
            var value = r.Decimal(min, max);
            if (decimals.HasValue)
            {
                return Math.Round(value, decimals.Value);
            }
            return value;
        }
    }
}
