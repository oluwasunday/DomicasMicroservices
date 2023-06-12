using AutoMapper;
using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using EventBus.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Basket.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository _basketRepository;
        private readonly DiscountGrpcService _discountGrpcService;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;
        public BasketController(IBasketRepository basketRepository, DiscountGrpcService discountGrpcService, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _basketRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));
            _discountGrpcService = discountGrpcService ?? throw new ArgumentNullException(nameof(discountGrpcService)); ;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        [HttpGet("{userName}", Name = "GetBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> GetBasket(string userName)
        {
            var basket = await _basketRepository.GetBasket(userName);
            return Ok(basket ?? new ShoppingCart(userName));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> UpdateBasket([FromBody] ShoppingCart basket)
        {
            // TODO: Communicate with discount.grpc to get lates discount amount
            // and calculate latest prices of product into shopping cart
            // consume Discount.Grpc
            foreach (var item in basket.Items)
            {
                var coupon = await _discountGrpcService.GetDiscount(item.ProductName);
                item.Price -= coupon.Amount;
            }

            return Ok(await _basketRepository.UpdateBasket(basket));
        }

        [HttpDelete("{userName}")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.NoContent)]
        public async Task<ActionResult> DeleteBasket(string userName)
        {
            await _basketRepository.DeleteBasket(userName);
            return Ok();
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            // 1. get existing basket with total price using a username
            // 2. create basketCheckoutEvent -- set TotalPrice on basketCheckout eventMessage
            // 3. send basketCheckout event to rabbitMQ usingMassTransit
            // 4. remove the basket

            // get existing basket with total price using a username
            var basket = await _basketRepository.GetBasket(basketCheckout.UserName);
            if(basket == null)
            {
                return BadRequest();
            }

            // create basketCheckoutEvent --set TotalPrice on basketCheckout eventMessage
            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
            eventMessage.TotalPrice = basket.TotalPrice;

            // send basketCheckout event to rabbitMQ usingMassTransit
            await _publishEndpoint.Publish(eventMessage);

            // remove the basket
            await _basketRepository.DeleteBasket(basket.UserName);

            return Accepted(basket);
        }
    }
}
