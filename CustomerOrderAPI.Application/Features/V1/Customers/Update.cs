using CustomerOrderAPI.Domain.Interface;
using CustomerOrderAPI.Application.Common.Exceptions;
using MediatR;
using CustomerOrderAPI.Application.DTO.Customer;

namespace CustomerOrderAPI.Application.Features.V1.Customers
{
    // Defines a feature for updating an existing customer's information.
    public class Update
    {
        // Command object used to encapsulate the data needed for the update operation.
        // This contains the customer's ID and the new customer data in a DTO format.
        public record Command(
            int Id,                  // The ID of the customer to be updated.
            CustomerDto CustomerDto   // Data Transfer Object (DTO) containing the updated customer data.
        ) : IRequest<Response>;       // The command expects a response containing the updated customer details.

        // CommandHandler processes the update request and performs the actual update in the database.
        public class CommandHandler : IRequestHandler<Command, Response>
        {
            private readonly IUnitOfWork _unitOfWork;  // Unit of Work pattern to access repositories for database operations.

            // Constructor to inject the IUnitOfWork dependency.
            public CommandHandler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork;
            }

            // Handles the incoming update command.
            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                // Retrieve the customer entity from the repository by ID.
                var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id);

                // If the customer is not found, throw an exception.
                if (customer == null)
                {
                    throw new Exception("Customer not found");
                }

                // Map the fields from the CustomerDto to the existing Customer entity.
                customer.FirstName = request.CustomerDto.FirstName;
                customer.LastName = request.CustomerDto.LastName;
                customer.Address = request.CustomerDto.Address;
                customer.PostalCode = request.CustomerDto.PostalCode;

                // Update the customer entity in the repository.
                _unitOfWork.Customers.Update(customer);

                // Commit the changes to the database.
                await _unitOfWork.SaveChangesAsync();

                // Return the updated customer details in the response object.
                return new Response(
                    customer.Id,
                    customer.FirstName,
                    customer.LastName,
                    customer.Address,
                    customer.PostalCode
                );
            }
        }

        // Response model that contains the updated customer details (ID, first name, last name, address, postal code).
        public record Response(int id, string FirstName, string LastName, string Address, string PostalCode);
    }
}
