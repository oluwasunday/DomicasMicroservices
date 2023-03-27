using Microsoft.Extensions.Logging;
using Ordering.Domain.Entities;

namespace Ordering.Infrastructure.Persistence
{
    public class OrderContextSeed
    {
        public static async Task SeedAsync(OrderContext orderContext, ILogger<OrderContextSeed> logger)
        {
            if (!orderContext.Orders.Any())
            {
                orderContext.Orders.AddRange(GetPreconfiguredOrders());
                await orderContext.SaveChangesAsync();
                logger.LogInformation("Seed database associated with context {DbContextName}", typeof(OrderContext).Name);
            }
        }

        private static IEnumerable<Order> GetPreconfiguredOrders()
        {
            return new List<Order>
            {
                new Order() {
                    //Id = Guid.NewGuid(),
                    UserName = "swn", 
                    FirstName = "Sunday", 
                    LastName = "Oladejo", 
                    EmailAddress = "soladejo@gmail.com", 
                    AddressLine = "Bahcelievler", 
                    Country = "Turkey", 
                    TotalPrice = 350,
                    CVV = "122",
                    CardName = "Sunday Oladejo",
                    CardNumber = "4433009933221234",
                    Expiration = "test",
                    LastModifiedBy = "Sunday",
                    CreatedBy = "Sunday",
                    State = "Ogun",
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    ZipCode= "212022",
                    PaymentMethod = 1
                }
            };
        }
    }
}
