using System.ComponentModel.DataAnnotations;
using static GraphQL.Models.Enums;

namespace GraphQL.Models
{

    public class Person
    {
        public Person() { }
        public Person(int id, string name, string email, DateTime joinDate, int age, Gender gender)
        {
            Id = id;
            Name = name;
            Email = email;
            JoinDate = joinDate;
            Age = age;
            Gender = gender;
        }
        [GraphQLNonNullType]
        public int Id { get; set; }
        [GraphQLNonNullType]
        public string Name { get; set; }
        [EmailAddress]
        [GraphQLNonNullType]
        public string Email { get; set; }
        [GraphQLNonNullType]
        public DateTime JoinDate { get; set; }
        [GraphQLNonNullType]
        public int Age { get; set; }
        [GraphQLNonNullType]
        public Gender Gender { get; set; }
    }
}
