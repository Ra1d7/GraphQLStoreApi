namespace GraphQL.Models
{
    public class Item
    {
        public Item() { }
        public Item(int itemId, decimal? price, string description, string name,Category category, int? qtn, bool? isAvailble)
        {
            ItemId = itemId;
            Price = price;
            Description = description;
            Name = name;
            Qtn = qtn;
            IsAvailble = isAvailble;
            Category = category;
        }

        [GraphQLNonNullType]
        public int ItemId { get; set; }
        [GraphQLNonNullType]
        public decimal? Price { get; set; }
        public string Description { get; set; } = "";
        [GraphQLNonNullType]
        public string Name { get; set; }
        [GraphQLNonNullType]
        [GraphQLName("Quantity")]
        public int? Qtn { get; set; }
        public bool? IsAvailble { get; set; } = false;
        public Category Category { get; set; }

        // this method can be called straight from the GraphQL Api without adding it to the Query :)
        public string ShortDescription()
        {
            return Description[..50].Replace("\n"," ") + "...";
        }
    }
}
