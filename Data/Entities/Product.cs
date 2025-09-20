﻿using System.ComponentModel.DataAnnotations.Schema;

namespace ASP_421.Data.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public Guid? GroupId { get; set; }  // посилання на групу
        public String Name { get; set; } = null!;
        public String? Description { get; set; } = null!;
        public String? Slug { get; set; } = null!;   // частина URL адреси
        public String? ImageUrl { get; set; } = null!;

        [Column(TypeName = "decimal(12,2)")]
        public double Price { get; set; }
        public int Stock { get; set; }
        public DateTime? DeletedAt { get; set; }


        public ProductGroup Group { get; set; } = null!;
    }
}
