using ASP_421.Data;
using ASP_421.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASP_421.Controllers
{
    public class ShopController(DataAccessor dataAccessor) : Controller
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;

        public IActionResult Index()
        {
            ShopIndexViewModel model = new()
            {
                ProductGroups = _dataAccessor.ProductGroups()
            };
            return View(model);
        }

        public IActionResult Cart(String? id)
        {
            // String? userId = HttpContext
            //     .User
            //     .Claims
            //     .FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid)
            //     ?.Value;

            ShopCartViewModel model = new()
            {
                Cart = id == null 
                ? HttpContext.Items["UserCart"] as Data.Entities.Cart
                : _dataAccessor.GetCart(id),
            };
            return View(model);
        }

        public IActionResult Group(String id)
        {
            ShopGroupViewModel model = new()
            {
                Slug = id,
                Group = _dataAccessor.GetProductGroupBySlug(id)
            };
            return View(model);
        }

        public IActionResult Product(String id)
        {
            var product = _dataAccessor.GetProductBySlug(id);
            ShopProductViewModel model = new()
            {
                SlugOrId = id,
                Product = product,
                Associations = product == null ? [] : product.Group.Products
            };
            return View(model);
        }


        public IActionResult Admin()
        {
            if(HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                String? role = HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                if (role == "Admin")
                {
                    ShopAdminViewModel model = new()
                    {
                        ProductGroups = _dataAccessor.ProductGroups()
                    };
                    return View(model);
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
/* Д.З. Реалізувати валідацію моделі форми товару (адмінка),
 * що передається на додавання до БД. 
 * За відсутності помилок очищувати форму від введених даних.
 * 
 * Додати до форми створення нової групи поле з введенням
 * батьківської групи (для створення підгруп), доповнити валідацію
 * моделі групи.
 * 
 * ** За зразком сайту Amazon додати до карточки групи (на домашній
 * сторінці) відомості про підгрупи (або виводити текстом їх кількість
 * або формувати зображення з зображень перших підгруп)
 */