using MediatR;
using Microsoft.EntityFrameworkCore;
using CustomerOrderAPI.Domain.Interface;

namespace CustomerOrderAPI.Application.Features.V1.Customers
{
    // Defines a feature for retrieving all customers without pagination or filters.
    public class GetAll
    {
        // Query object used to request the list of all customers.
        public record Query : IRequest<List<Response>>;  // MediatR IRequest indicating the expected return type is a list of Response.

        // QueryHandler processes the request to get all customers.
        public class QueryHandler : IRequestHandler<Query, List<Response>>
        {
            private readonly IUnitOfWork _unitOfWork;   // Unit of Work pattern for interacting with repositories.

            // Constructor to inject the IUnitOfWork dependency.
            public QueryHandler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));  // Ensure unitOfWork is not null.
            }

            // Handles the incoming query and returns a list of customer responses.
            public async Task<List<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Query the customer repository using IQueryable to retrieve all customer records.
                return await _unitOfWork
                    .Customers
                    .AsQueryable()  // Start querying the Customers table.
                    .OrderBy(c => c.Id)  // Order customers by their Id (ascending order).
                    .Select(q => new Response(  // Project each customer entity into the Response model.
                        q.Id,
                        q.FirstName,
                        q.LastName,
                        q.Address,
                        q.PostalCode
                    ))
                    .ToListAsync(cancellationToken);  // Convert the query to a list asynchronously.
            }
        }

        // Response model defining the structure of the customer data that will be returned to the client.
        public record Response(int id, string firstName, string lastName, string address, string postalCode);
    }
}
