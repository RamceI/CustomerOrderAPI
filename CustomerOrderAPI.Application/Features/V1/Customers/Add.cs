using MediatR;
using CustomerOrderAPI.Domain.Entities;
using CustomerOrderAPI.Domain.Interface;
using CustomerOrderAPI.Application.DTO.Customer;

namespace CustomerOrderAPI.Application.Features.V1.Customers
{
    public class Add
    {
        // This now specifies IRequest<Unit> since we don't expect a return value.
        public record Command(
            CustomerDto customerDto
        ) : IRequest<Unit> // Specify Unit as the return type
        {
            public Customer ToEntity() => new Customer
            {
                FirstName = customerDto.FirstName,
                LastName = customerDto.LastName,
                Address = customerDto.Address,
                PostalCode = customerDto.PostalCode
            };
        }

        // Update the IRequestHandler to handle Command and return Task<Unit>.
        public class CommandHandler : IRequestHandler<Command, Unit>
        {
            private readonly IUnitOfWork _unitOfWork;

            public CommandHandler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var entity = request.ToEntity();

                // Adding the customer to the repository
                await _unitOfWork.Customers.AddAsync(entity);

                // Saving changes in the Unit of Work
                await _unitOfWork.SaveChangesAsync();

                // Return Unit.Value to indicate success
                return Unit.Value;
            }
        }
    }
}
