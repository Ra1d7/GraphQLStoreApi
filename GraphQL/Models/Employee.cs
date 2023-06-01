using static GraphQL.Models.Enums;

namespace GraphQL.Models
{
    public class Employee : Person
    {
        public decimal Salary { get; set; }

        public Department Department { get; set; }
        
        public Employee() { }
        public Employee(int id, string name, string email, DateTime joinDate, int age, Gender gender , decimal salary , Department department) : base(id, name, email, joinDate, age, gender)
        {
            Salary = salary;
            Department = department;
        }
    }
}
