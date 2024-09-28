using CustomerOrderAPI.Application.DTO.Order;
using CustomerOrderAPI.Domain.Entities;
using CustomerOrderAPI.Domain.Interface;
using MediatR;

namespace CustomerOrderAPI.Application.Features.V1.Orders
{
    // Defines the feature to add a new order, including the details for the customer and the ordered items.
    public class Add
    {
        // Command object used to encapsulate the order data needed for creating a new order.
        // It contains an OrderDto with all necessary information.
        public record Command(
            OrderDto OrderDto  // Data Transfer Object (DTO) that contains the order details.
        ) : IRequest<Unit>    // The command expects a response indicating that the operation was successful (Unit).
        {
            // Converts the OrderDto into the Order entity, mapping the DTO fields to the entity.
            public Order ToEntity()
            {
                var order = new Order
                {
                    CustomerId = OrderDto.CustomerId,   // Maps the Customer ID.
                    OrderDate = OrderDto.OrderDate      // Maps the Order Date.
                };

                // Maps the order items from the DTO to the entity's Items collection.
                foreach (var itemDto in OrderDto.Items)
                {
                    order.Items.Add(new Item
                    {
                        ProductId = itemDto.ProductId,  // Maps the Product ID.
                        Quantity = itemDto.Quantity     // Maps the Quantity for each item.
                    });
                }

                return order;  // Returns the Order entity with mapped data.
            }
        }

        // CommandHandler processes the add order request and performs the logic to save the order in the database.
        public class CommandHandler : IRequestHandler<Command, Unit>
        {
            private readonly IUnitOfWork _unitOfWork;  // Unit of Work pattern for handling database operations.

            // Constructor to inject the IUnitOfWork dependency.
            public CommandHandler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            }

            // Handles the incoming add order command.
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                // Converts the DTO into the Order entity using the ToEntity method.
                var order = request.ToEntity();

                // Initialize the total price for the order.
                decimal totalPrice = 0;

                // Loop through each item in the order to calculate the total price.
                foreach (var item in order.Items)
                {
                    // Fetch the product information (including price) from the repository.
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);

                    if (product != null)
                    {
                        // Calculate the price for the item and add it to the total price.
                        totalPrice += item.Quantity * product.Price;
                    }
                }

                // Set the total price for the order.
                order.TotalPrice = totalPrice;

                // Add the new order to the repository (but doesn't commit to the database yet).
                await _unitOfWork.Orders.AddAsync(order);

                // Save changes in the Unit of Work (committing the order and all related entities to the database).
                await _unitOfWork.SaveChangesAsync();

                // Return Unit to indicate the operation was successful (this is a MediatR convention for commands with no specific return value).
                return Unit.Value;
            }
        }
    }
}
