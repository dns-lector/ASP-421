using ASP_421.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ASP_421.Data
{
    public class DataAccessor(DataContext dataContext)
    {
        private readonly DataContext _dataContext = dataContext;

        public ProductGroup? GetProductGroupBySlug(String slug)
        {
            return _dataContext
                .ProductGroups
                .Include(g => g.Products)
                .FirstOrDefault(g => g.Slug == slug && g.DeletedAt == null);
        }
        public Product? GetProductBySlug(String slug)
        {
            return _dataContext
                .Products
                .Include(p => p.Group)
                .ThenInclude(g => g.Products.OrderBy(p => Guid.NewGuid()).Take(3))
                .FirstOrDefault(p => p.DeletedAt == null &&
                    (p.Slug == slug || p.Id.ToString() == slug) );
        }

        public void AddProduct(Product product)
        {
            if(product.Id == default)
            {
                product.Id = Guid.NewGuid();
            }
            product.DeletedAt = null;
            _dataContext.Products.Add(product);
            _dataContext.SaveChanges();
        }

        public IEnumerable<ProductGroup> ProductGroups()
        {
            return _dataContext
                .ProductGroups
                .Where(g => g.DeletedAt == null)
                .AsEnumerable();
        }
    }
}
