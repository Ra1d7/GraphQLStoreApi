namespace GraphQL.Models
{
    public class Customer : Person
    {
        public Customer() { }
        public Customer(int id, string name, string email, DateTime joinDate, int age, Enums.Gender gender , string shippingAddress , bool hasPremium) : base(id, name, email, joinDate, age, gender)
        {
            ShippingAddress = shippingAddress;
            HasPremiumMembership = hasPremium;
        }
        public string ShippingAddress { get; set; }

        public bool HasPremiumMembership { get; set; }

    }
}
