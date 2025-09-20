namespace ASP_421.Data.Entities
{
    public class ProductGroup
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }  // посилання на батьківську групу
        public String Name { get; set; } = null!;
        public String Description { get; set; } = null!;
        public String Slug { get; set; } = null!;   // частина URL адреси
        public String ImageUrl { get; set; } = null!;
        public DateTime? DeletedAt { get; set; }


        public ICollection<Product> Products { get; set; } = [];
    }
}
