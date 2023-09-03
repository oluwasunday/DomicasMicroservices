using Microsoft.AspNetCore.Mvc;
using Shopping.Aggregator.Models;
using Shopping.Aggregator.Services;
using System.Net;

namespace Shopping.Aggregator.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ShoppingController : ControllerBase
    {
        private readonly ICatalogService _catalogService;
        private readonly IBasketService _basketService;
        private readonly IOrderService _orderService;

        public ShoppingController(IOrderService orderService, ICatalogService catalogService, IBasketService basketService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _catalogService = catalogService ?? throw new ArgumentNullException(nameof(catalogService));
            _basketService = basketService ?? throw new ArgumentNullException(nameof(basketService));
        }

        [HttpGet("{userName}", Name = "GetShopping")]
        [ProducesResponseType(typeof(ShoppingModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingModel>> GetShopping(string userName)
        {
            //1. get basket with username
            var basket = await _basketService.GetBasket(userName);

            //2. iterate the basket items and consume products with basket item productId member
            foreach (var item in basket.Items)
            {
                var product = await _catalogService.GetCatalog(item.ProductId);

                //3. map product related members into basketitem dto with extended columns
                // set additional product fields onto basket item
                item.ProductName = product.Name;
                item.Description = product.Description;
                item.Category = product.Category;
                item.ImageFile = product.ImageFile;
                item.Summary = product.Summary;
            }
            
            //4. consume ordering microservices in order to retrieve order list
            var orders = await _orderService.GetOrdersByUserName(userName);

            //5.return root shoppingModel dto class which include all responses
            var shoppingModel = new ShoppingModel
            {
                UserName = userName,
                BasketWithProducts = basket,
                Orders = orders
            };

            return Ok(shoppingModel);
        }
    }
}
