using MediatR;
using Microsoft.EntityFrameworkCore;
using CustomerOrderAPI.Domain.Interface;

namespace CustomerOrderAPI.Application.Features.V1.Products
{
    // Feature to handle fetching a list of all products
    public class GetAll
    {
        // Query object representing a request for all products (no parameters needed)
        public record Query : IRequest<List<Response>>;

        // QueryHandler to process the Query and return a list of product data
        public class QueryHandler : IRequestHandler<Query, List<Response>>
        {
            private readonly IUnitOfWork _unitOfWork;  // Injecting UnitOfWork to access repositories

            // Constructor to initialize UnitOfWork dependency
            public QueryHandler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            }

            // Handle method to execute the logic for retrieving all products
            public async Task<List<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Step 1: Query the products repository
                // Fetch all products, ordered by their ID
                return await _unitOfWork
                    .Products
                    .AsQueryable()  // Use IQueryable to efficiently query the data source
                    .OrderBy(c => c.Id)  // Order products by ID in ascending order
                    .Select(q => new Response(
                        q.Id,  // Map product ID
                        q.Name,  // Map product Name
                        q.Price  // Map product Price
                    ))
                    .ToListAsync(cancellationToken);  // Convert the query result to a list asynchronously
            }
        }

        // DTO (Data Transfer Object) class for the response containing product details
        public record Response(int id, string name, decimal price);  // Response structure with product ID, Name, and Price
    }
}
