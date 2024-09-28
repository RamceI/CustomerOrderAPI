using MediatR;
using Microsoft.EntityFrameworkCore;
using CustomerOrderAPI.Domain.Interface;
using System.Linq;
using CustomerOrderAPI.Domain.Enum;
using CustomerOrderAPI.Application.Common.Models;

namespace CustomerOrderAPI.Application.Features.V1.Customers
{
    // Defines a feature for retrieving paginated customer data with optional filtering.
    public class Get
    {
        // Request model for querying customer data with pagination and optional filter parameters.
        public record Query(
            int page = (int)PaginationDefaultEnum.Page,   // Default page number (from enum).
            int size = (int)PaginationDefaultEnum.Size,   // Default page size (from enum).
            string? filter = null                         // Optional search filter (by customer name, address, or postal code).
        ) : IRequest<PaginationModel<Response>>;         // Return type is a paginated model of customer responses.

        // QueryHandler handles the request and processes the logic to retrieve and return the data.
        public class QueryHandler : IRequestHandler<Query, PaginationModel<Response>>
        {
            private readonly IUnitOfWork _unitOfWork;   // Unit of Work pattern to interact with repositories.

            // Constructor that injects the IUnitOfWork dependency.
            public QueryHandler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            }

            // Handles the incoming query and returns the paginated list of customer responses.
            public async Task<PaginationModel<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Query the customer repository using IQueryable, applying filter if provided.
                var query = _unitOfWork
                    .Customers
                    .AsQueryable()
                    .Where(q => string.IsNullOrEmpty(request.filter) ? true :
                        // Filter by matching the first name, last name, address, or postal code (case-insensitive).
                        q.FirstName.ToLower().Contains(request.filter.ToLower()) ||
                        q.LastName.ToLower().Contains(request.filter.ToLower()) ||
                        q.Address.ToLower().Contains(request.filter.ToLower()) ||
                        q.PostalCode.ToLower().Contains(request.filter.ToLower())
                    )
                    // Order the results by customer ID in descending order.
                    .OrderByDescending(q => q.Id)
                    // Project the customer entity into the Response model.
                    .Select(q => new Response(
                        q.Id,
                        q.FirstName,
                        q.LastName,
                        q.Address,
                        q.PostalCode
                    ));

                // Return the paginated result:
                // 1. Count total items that match the filter (for total pages calculation).
                // 2. Skip the items based on the current page and size.
                // 3. Take only the items for the current page.
                return new PaginationModel<Response>(
                    await query.CountAsync(cancellationToken),        // Total count of matching records.
                    request.page,                                     // Current page number.
                    request.size,                                     // Page size.
                    await query
                        .Skip((request.page - 1) * request.size)       // Skip items for previous pages.
                        .Take(request.size)                            // Take items for the current page.
                        .ToListAsync(cancellationToken)                // Convert query to a list asynchronously.
                );
            }
        }

        // Response model that defines the data structure returned for each customer in the paginated results.
        public record Response(int id, string firstName, string lastName, string address, string postalCode);
    }
}
