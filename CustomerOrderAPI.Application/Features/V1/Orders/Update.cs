using CustomerOrderAPI.Application.DTO.Order;
using CustomerOrderAPI.Domain.Entities;
using CustomerOrderAPI.Domain.Interface;
using MediatR;

namespace CustomerOrderAPI.Application.Features.V1.Orders
{
    // Update class to handle the updating of an existing order
    public class Update
    {
        // Command record that encapsulates the order ID and the new OrderDto data to update
        public record Command(
            int Id,              // The ID of the order to be updated
            OrderDto OrderDto     // Data Transfer Object containing new order details
        ) : IRequest<Unit>       // The command will return Unit (no specific data) when completed
        {
            // Method to map the OrderDto to the existing Order entity
            public async Task<Order> ToEntityAsync(Order existingOrder, IUnitOfWork unitOfWork)
            {
                // Step 1: Update basic order properties (CustomerId, OrderDate)
                existingOrder.CustomerId = OrderDto.CustomerId;
                existingOrder.OrderDate = OrderDto.OrderDate;

                // Step 2: Update the order items (add new, update existing, remove missing)
                var itemsToUpdate = OrderDto.Items.Select(i => new { i.ProductId, i.Quantity }).ToList();

                // Step 3: Remove items that are no longer present in the DTO
                var itemsToRemove = existingOrder.Items
                    .Where(item => !itemsToUpdate.Any(i => i.ProductId == item.ProductId)) // Find missing items
                    .ToList(); // Avoid modifying collection while iterating

                foreach (var item in itemsToRemove)
                {
                    // Remove missing items from the order
                    existingOrder.Items.Remove(item);
                }

                // Step 4: Calculate total price and update items (add/update items)
                decimal totalPrice = 0;

                foreach (var itemDto in OrderDto.Items)
                {
                    var existingItem = existingOrder.Items.FirstOrDefault(i => i.ProductId == itemDto.ProductId);

                    // Fetch product details to get the price
                    var product = await unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
                    if (product == null)
                    {
                        // Throw exception if the product is not found in the database
                        throw new Exception($"Product with ID {itemDto.ProductId} not found");
                    }

                    if (existingItem != null)
                    {
                        // Step 5: If item exists, update its quantity
                        existingItem.Quantity = itemDto.Quantity;
                    }
                    else
                    {
                        // Step 6: If item doesn't exist, add it to the order
                        existingOrder.Items.Add(new Item
                        {
                            ProductId = itemDto.ProductId,
                            Quantity = itemDto.Quantity
                        });
                    }

                    // Step 7: Update the total price of the order
                    totalPrice += itemDto.Quantity * product.Price;
                }

                // Step 8: Set the total price for the order
                existingOrder.TotalPrice = totalPrice;

                return existingOrder;
            }
        }

        // CommandHandler class to process the Command and update the order in the database
        public class CommandHandler : IRequestHandler<Command, Unit>
        {
            private readonly IUnitOfWork _unitOfWork; // UnitOfWork for database interaction

            // Constructor to inject the UnitOfWork dependency
            public CommandHandler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork;
            }

            // Handle method to process the update command
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                // Step 1: Retrieve the existing order by its ID
                var existingOrder = await _unitOfWork.Orders.GetByIdAsync(request.Id);

                if (existingOrder == null)
                {
                    // Step 2: Throw an exception if the order is not found
                    throw new Exception("Order not found");
                }

                // Step 3: Update the existing order with new data from OrderDto
                await request.ToEntityAsync(existingOrder, _unitOfWork);

                // Step 4: Update the order in the repository
                _unitOfWork.Orders.Update(existingOrder);

                // Step 5: Save the changes to the database
                await _unitOfWork.SaveChangesAsync();

                // Step 6: Return Unit (indicating the operation has completed)
                return Unit.Value;
            }
        }
    }
}
