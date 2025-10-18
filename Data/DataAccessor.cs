using ASP_421.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ASP_421.Data
{
    public class DataAccessor(DataContext dataContext)
    {
        private readonly DataContext _dataContext = dataContext;

        public Cart? GetActiveCart(String userId)
        {
            Guid userGuid = Guid.Parse(userId);
            return _dataContext
                .Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c =>
                c.UserId == userGuid &&
                c.PaidAt == null &&
                c.DeletedAt == null);
        }

        public Cart? GetCart(String id)
        {
            Guid cartGuid = Guid.Parse(id);
            return _dataContext
                .Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.Id == cartGuid);
        }

        public void CheckoutCart(String userId)
        {
            Cart activeCart = this.GetActiveCart(userId)
                ?? throw new Exception("User has no active cart");
            activeCart.PaidAt = DateTime.Now;
            _dataContext.SaveChanges();
        }

        public void CancelCart(String userId)
        {
            Cart activeCart = this.GetActiveCart(userId)
                ?? throw new Exception("User has no active cart");
            activeCart.DeletedAt = DateTime.Now;
            _dataContext.SaveChanges();
        }

        public void ModifyCartItem(String userId, String cartItemId, int inc)
        {
            Guid cartItemGuid = Guid.Parse(cartItemId);
            Cart activeCart = this.GetActiveCart(userId)
                ?? throw new Exception("User has no active cart");
            CartItem cartItem = activeCart.CartItems
                .FirstOrDefault(ci => ci.Id == cartItemGuid)
                ?? throw new Exception("User has no requested cart item");
            cartItem.Quantity += inc;
            CalcCartPrice(activeCart);
            _dataContext.SaveChanges();
            /*
             * Д.З. Забезпечити перевірку можливості зміни CartItem
             * - нова кількість більша за ноль
             *  = якщо менша за ноль, то це помилка - throw new Exception
             *  = якщо дорівнює нулю, то позицію слід видалити замість
             *     встановлення нульової кількості
             * - нова кількість не перевищує складські залишки товару    
             * 
             * З боку фронтенда виводити повідомлення 
             * - підтвердження видалення при натисканні "-", що призводитиме до 
             *    нульової кількості
             * - про недостатність товару на складі
             * - про загальні помилки ("Сталась помилка, повторіть дію пізніше")
             * 
             * Реалізувати кнопку "х" видалення позиції на базі ModifyCartItem
             * з передачею inc, що призведе до нульової кількості. Також
             * додати повідомлення-погодження
             * 
             * Реалізувати перехід на картку товара при натисканні на його
             * зображення у кошику (ліва позиція). У самій картці товару 
             * також аналізувати його наявність у кошику та змінювати 
             * кнопку "Додати до кошику" / "Перейти до кошику"
             */
        }

        public void RepeatCart(String userId, String cartId)
        {
            Guid userGuid = Guid.Parse(userId);
            Guid cartGuid = Guid.Parse(cartId);
            User user = _dataContext.Users.Find(userGuid)
                ?? throw new KeyNotFoundException("User not found");
            Cart cart = _dataContext
                .Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.Id == cartGuid)
                ?? throw new KeyNotFoundException("Cart not found");
            // Перевіряємо, чи є у користувача відкритий (активний) кошик
            // Якщо ні, то відкриваємо (створюємо) новий
            //   якщо так, то додаємо до нього товари з повторюваного кошику
            Cart? activeCart = this.GetActiveCart(userId);

            if (activeCart == null)
            {
                activeCart = new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userGuid,
                    CreatedAt = DateTime.Now,
                };
                _dataContext.Carts.Add(activeCart);
            }
            foreach (CartItem oldCartItem in cart.CartItems)
            {
                CartItem? cartItem = activeCart
                .CartItems
                .FirstOrDefault(ci => ci.ProductId == oldCartItem.ProductId);

                if (cartItem == null)
                {
                    cartItem = new()
                    {
                        CartId = activeCart.Id,
                        ProductId = oldCartItem.ProductId,
                        Quantity = oldCartItem.Quantity,
                        Product = oldCartItem.Product,
                    };
                    _dataContext.CartItems.Add(cartItem);
                }
                else
                {
                    cartItem.Quantity = oldCartItem.Quantity;
                }
            }
            // Перераховуємо ціну всього кошику з урахуванням можливих акцій
            CalcCartPrice(activeCart);
            // зберігаємо зміни
            _dataContext.SaveChanges();
        }
        /* Д.З. Удосконалити роботу сервісу повторення замовлення
         * DataAccessor::RepeatCart
         * - забезпечити перевірку того, що повторюване замовлення належить
         *    тому ж користувачу, який ініціює дію
         * - додати перевірку того, що товар не був видалений (у старому
         *    кошику можуть бути замовлення, яких вже не існує)
         * - додати перевірку того, що товар є у достатній кількості
         * 
         * Скорегувати роботу сервісу замовлення 
         * DataAccessor::CheckoutCart
         * - зменшувати кількості товарів у залишках (stock) на відповідні
         *    кількості у замовленні
         */

        public void AddProductToCart(String userId, String productId)
        {
            Guid userGuid = Guid.Parse(userId);         // винятки від цих операцій
            Guid productGuid = Guid.Parse(productId);   // передаватимуться до контролера
            User user = _dataContext.Users.Find(userGuid)
                ?? throw new KeyNotFoundException("User not found");
            Product product = _dataContext.Products.Find(productGuid)
                ?? throw new KeyNotFoundException("Product not found");
            // Перевіряємо, чи є у користувача відкритий (активний) кошик
            // Якщо ні, то відкриваємо (створюємо) новий
            //   якщо так, то працюємо з відкритим
            // Перевіряємо чи є у кошику позиція (Item) з даним товаром
            // Якщо ні, то створюємо нову
            //   якщо так, то додаємо +1 до цієї позиції
            Cart? activeCart = this.GetActiveCart(userId);

            if (activeCart == null)
            {
                activeCart = new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userGuid,
                    CreatedAt = DateTime.Now,                    
                };
                _dataContext.Carts.Add(activeCart);
            }

            CartItem? cartItem = activeCart
                .CartItems
                .FirstOrDefault(ci => ci.ProductId == productGuid);

            if (cartItem == null)
            {
                cartItem = new()
                {
                    CartId = activeCart.Id,
                    ProductId = productGuid,
                    Quantity = 1,
                    Product = product,
                };
                _dataContext.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += 1;
            }
            // Перераховуємо ціну всього кошику з урахуванням можливих акцій
            CalcCartPrice(activeCart);
            // зберігаємо зміни
            _dataContext.SaveChanges();
        }

        private void CalcCartPrice(Cart cart)
        {
            double price = 0.0;
            foreach (var item in cart.CartItems)
            {
                if(item.DiscountItemId != null)
                {
                    // Тут буде перерахунок ціни позиції з урахуванням акції
                }
                else
                {
                    item.Price = item.Product.Price * item.Quantity;
                }
                price += item.Price;
            }
            if (cart.DiscountItemId != null)
            {
                // Тут буде перерахунок ціни кошику з урахуванням акції
            }
            cart.Price = price;
        }

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
