using CustomerOrderAPI.Application.DTO.Product;
using CustomerOrderAPI.Domain.Entities;
using CustomerOrderAPI.Domain.Interface;
using MediatR;

namespace CustomerOrderAPI.Application.Features.V1.Products
{
    // Feature to handle adding a new product
    public class Add
    {
        // Command to encapsulate the ProductDto which contains the product details to be added
        public record Command(
            ProductDto productDto // Data Transfer Object containing the new product information
        ) : IRequest<Unit> // This command will return Unit when completed, indicating success
        {
            // Method to map the ProductDto to a new Product entity
            public Product ToEntity() => new Product
            {
                Name = productDto.Name,   // Set the product name
                Price = productDto.Price, // Set the product price
            };
        }

        // CommandHandler to handle the execution of the add product command
        public class CommandHandler : IRequestHandler<Command, Unit>
        {
            private readonly IUnitOfWork _unitOfWork; // Inject the UnitOfWork for data access

            // Constructor to inject UnitOfWork dependency
            public CommandHandler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            }

            // Handle method to process the command and add the new product
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                // Step 1: Convert the ProductDto to a Product entity using ToEntity method
                var entity = request.ToEntity();

                // Step 2: Add the new product entity to the Products repository
                await _unitOfWork.Products.AddAsync(entity);

                // Step 3: Save the changes in the Unit of Work (persist the new product in the database)
                await _unitOfWork.SaveChangesAsync();

                // Step 4: Return Unit.Value to indicate the command was handled successfully
                return Unit.Value;
            }
        }
    }
}
