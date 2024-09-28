using MediatR;
using Microsoft.EntityFrameworkCore;
using CustomerOrderAPI.Domain.Interface;
using CustomerOrderAPI.Domain.Enum;
using CustomerOrderAPI.Application.Common.Models;

namespace CustomerOrderAPI.Application.Features.V1.Products
{
    // Feature to handle paginated retrieval of products, with an optional filter
    public class Get
    {
        // Query object to encapsulate pagination and filtering parameters
        public record Query(
            int page = (int)PaginationDefaultEnum.Page,  // Default page number
            int size = (int)PaginationDefaultEnum.Size,  // Default page size
            string? filter = null  // Optional filter string to search by product name
        ) : IRequest<PaginationModel<Response>>;  // Returns a paginated response of products

        // QueryHandler to process the Query and return paginated product data
        public class QueryHandler : IRequestHandler<Query, PaginationModel<Response>>
        {
            private readonly IUnitOfWork _unitOfWork;  // Injecting UnitOfWork to access repositories

            // Constructor to initialize UnitOfWork dependency
            public QueryHandler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            }

            // Handle method to execute the query logic
            public async Task<PaginationModel<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Step 1: Query the products repository with optional filtering and pagination
                var query = _unitOfWork
                    .Products
                    .AsQueryable()  // Start with a queryable collection of products
                    .Where(q => string.IsNullOrEmpty(request.filter) ? true :
                           q.Name.ToLower().Contains(request.filter.ToLower()) ||  // Apply filter if provided
                           q.Price.ToString().ToLower().Contains(request.filter.ToLower())
                    )
                    .OrderByDescending(q => q.Id)  // Order products by ID, newest first
                    .Select(q => new Response(
                        q.Id,    // Map product ID
                        q.Name,  // Map product Name
                        q.Price  // Map product Price
                    ));

                // Step 2: Return a PaginationModel containing the total count and paginated data
                return new PaginationModel<Response>(
                    await query.CountAsync(cancellationToken),  // Count the total number of filtered products
                    request.page,  // Current page number
                    request.size,  // Page size
                    await query
                        .Skip((request.page - 1) * request.size)  // Skip records for pagination
                        .Take(request.size)  // Take only the required number of records
                        .ToListAsync(cancellationToken)  // Convert query to a list
                );
            }
        }

        // DTO (Data Transfer Object) class for the response containing product details
        public record Response(int id, string name, decimal price);  // Response with product ID, Name, and Price
    }
}
