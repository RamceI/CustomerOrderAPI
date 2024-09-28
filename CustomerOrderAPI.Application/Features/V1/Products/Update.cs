using CustomerOrderAPI.Application.DTO.Product;
using CustomerOrderAPI.Domain.Interface;
using MediatR;

namespace CustomerOrderAPI.Application.Features.V1.Products
{
    // Feature to update an existing product by its ID
    public class Update
    {
        // Command object representing the request to update a product
        public record Command(
            int Id,                 // The ID of the product to be updated
            ProductDto productDto    // The updated product details (name, price, etc.)
        ) : IRequest<Response>;     // Returns a Response containing the updated product's ID

        // CommandHandler to process the Command and perform the update operation
        public class CommandHandler : IRequestHandler<Command, Response>
        {
            private readonly IUnitOfWork _unitOfWork;  // Injecting the UnitOfWork to manage repositories

            // Constructor to initialize the UnitOfWork dependency
            public CommandHandler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork;
            }

            // Handle method to execute the product update logic
            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                // Step 1: Retrieve the product from the database by its ID
                var product = await _unitOfWork.Products.GetByIdAsync(request.Id);

                // Step 2: Check if the product exists
                if (product == null)
                {
                    throw new Exception("Product not found");  // Throw an exception if the product doesn't exist
                }

                // Step 3: Update the product properties with the new values from the DTO
                product.Name = request.productDto.Name;    // Update the product's name
                product.Price = request.productDto.Price;  // Update the product's price

                // Step 4: Mark the product as updated in the UnitOfWork
                _unitOfWork.Products.Update(product);

                // Step 5: Persist changes to the database
                await _unitOfWork.SaveChangesAsync();

                // Step 6: Return a Response containing the updated product's ID
                return new Response(product.Id);
            }
        }

        // DTO (Data Transfer Object) class for the response containing the updated product ID
        public record Response(int id);  // The response will contain the ID of the updated product
    }
}
