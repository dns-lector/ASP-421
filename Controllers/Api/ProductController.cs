using ASP_421.Data;
using ASP_421.Data.Entities;
using ASP_421.Models.Shop.Api;
using ASP_421.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_421.Controllers.Api
{
    [Route("api/product")]
    [ApiController]
    public class ProductController(
            IStorageService storageService,
            ILogger<ProductController> logger,
            DataAccessor dataAccessor
        ) : ControllerBase
    {
        private readonly IStorageService _storageService = storageService;
        private readonly ILogger<ProductController> _logger = logger;
        private readonly DataAccessor _dataAccessor = dataAccessor;

        [HttpPost]
        public object CreateProduct(ShopApiProductFormModel formModel)
        {
            // валідація моделі

            try
            {
                Product product = new()
                {
                    Name = formModel.Name,
                    Description = formModel.Description,
                    Slug = formModel.Slug,
                    Stock = formModel.Stock,
                    Price = formModel.Price,
                    GroupId = Guid.Parse(formModel.GroupId),
                    ImageUrl = formModel.Image == null ? null :
                        _storageService.Save(formModel.Image)
                };
                _dataAccessor.AddProduct(product);
                return new
                {
                    Status = "Ok"
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Add product error {ex}", ex.Message);
                return new
                {
                    Status = "Fail"
                };
            }
        }
    }
}
