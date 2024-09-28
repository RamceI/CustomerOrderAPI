using CustomerOrderAPI.Domain.Interface;
using MediatR;

namespace CustomerOrderAPI.Application.Features.V1.Customers
{
    // Defines a feature for deleting a customer by ID.
    public class Delete
    {
        // Command model representing the customer deletion request by customer ID.
        public record Command(int Id) : IRequest<Unit>;  // `Unit` represents a void-like result in MediatR.

        // CommandHandler handles the logic for processing the delete request.
        public class CommandHandler : IRequestHandler<Command, Unit>
        {
            private readonly IUnitOfWork _unitOfWork;   // Unit of Work pattern for repository access.

            // Constructor to inject the IUnitOfWork dependency.
            public CommandHandler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork;
            }

            // Handles the incoming delete command.
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                // Fetch the customer entity from the repository by ID.
                var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id);

                // If the customer is not found, throw an exception.
                if (customer == null)
                {
                    throw new Exception("Customer not found");
                }

                // Delete the customer entity from the repository.
                _unitOfWork.Customers.Delete(customer);

                // Commit the changes to the database.
                await _unitOfWork.SaveChangesAsync();

                // Return Unit.Value indicating successful completion.
                return Unit.Value;  // MediatR's way of returning a void result.
            }
        }
    }
}