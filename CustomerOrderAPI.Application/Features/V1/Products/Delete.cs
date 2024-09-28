using CustomerOrderAPI.Domain.Interface;
using MediatR;

namespace CustomerOrderAPI.Application.Features.V1.Products
{
    // Feature to handle deleting a product
    public class Delete
    {
        // Command to encapsulate the product ID to be deleted
        public record Command(int Id) : IRequest<Unit>; // Command contains the product ID to delete and returns Unit on success

        // CommandHandler to handle the delete product command
        public class CommandHandler : IRequestHandler<Command, Unit>
        {
            private readonly IUnitOfWork _unitOfWork; // Inject the UnitOfWork to access repositories

            // Constructor to inject the UnitOfWork dependency
            public CommandHandler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork;
            }

            // Handle method to process the delete command
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                // Step 1: Fetch the product by ID from the repository
                var product = await _unitOfWork.Products.GetByIdAsync(request.Id);

                // Step 2: Check if the product exists, if not, throw an exception
                if (product == null)
                {
                    throw new Exception("Product not found"); // Throw an exception if product is not found
                }

                // Step 3: Delete the product from the repository
                _unitOfWork.Products.Delete(product);

                // Step 4: Save changes in the Unit of Work (commit the deletion to the database)
                await _unitOfWork.SaveChangesAsync();

                // Step 5: Return Unit.Value to indicate the operation was successful
                return Unit.Value;
            }
        }
    }
}
