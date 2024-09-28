using CustomerOrderAPI.Domain.Entities;
using CustomerOrderAPI.Domain.Interface;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerOrderAPI.Application.Features.V1.Orders
{
    // Command class to encapsulate the data needed to delete an order, identified by its Id.
    public class Delete
    {
        public record Command(int Id) : IRequest<Unit>; // The Command takes an Id of the order to delete.

        // CommandHandler class to handle the Delete Command using MediatR pattern.
        public class CommandHandler : IRequestHandler<Command, Unit>
        {
            private readonly IUnitOfWork _unitOfWork; // IUnitOfWork to manage database operations.

            // Constructor that initializes the CommandHandler with the IUnitOfWork dependency.
            public CommandHandler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork;
            }

            // Handle method to execute the logic for the delete command.
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                // Step 1: Retrieve the order by its Id from the database.
                var existingOrder = await _unitOfWork.Orders.GetByIdAsync(request.Id);

                // Step 2: Check if the order exists, if not, throw an exception.
                if (existingOrder == null)
                {
                    throw new Exception("Order not found");
                }

                // Step 3: Get the associated order items (if not handling cascade delete in DB).
                // Convert the order's Items collection to a list for iteration.
                var itemsToDelete = existingOrder.Items.ToList();

                // Step 4: Loop through each order item and delete it explicitly.
                foreach (var item in itemsToDelete)
                {
                    _unitOfWork.Items.Delete(item); // Delete each item via the Items repository.
                }

                // Step 5: Delete the order itself after its items have been deleted.
                _unitOfWork.Orders.Delete(existingOrder);

                // Step 6: Commit all changes in the Unit of Work, ensuring both the order
                // and its associated items are deleted from the database.
                await _unitOfWork.SaveChangesAsync();

                // Return a successful response (Unit.Value) signaling that the operation is complete.
                return Unit.Value;
            }
        }
    }
}
