using MediatR;
using Microsoft.EntityFrameworkCore;
using CustomerOrderAPI.Domain.Interface;
using CustomerOrderAPI.Domain.Enum;
using CustomerOrderAPI.Application.Common.Models;

namespace CustomerOrderAPI.Application.Features.V1.Orders
{
    // Query class to encapsulate the parameters for pagination, filtering, and retrieving orders.
    public class Get
    {
        // Query record takes optional parameters for page, size, and filter criteria.
        public record Query(
            int page = (int)PaginationDefaultEnum.Page, // Default page number from an enum.
            int size = (int)PaginationDefaultEnum.Size, // Default page size from an enum.
            string? filter = null // Optional filter string for searching orders.
        ) : IRequest<PaginationModel<Response>>; // Query returns a paginated list of Response objects.

        // QueryHandler class to handle the Get Query using MediatR.
        public class QueryHandler : IRequestHandler<Query, PaginationModel<Response>>
        {
            private readonly IUnitOfWork _unitOfWork; // UnitOfWork to manage data access operations.

            // Constructor to initialize the QueryHandler with the IUnitOfWork dependency.
            public QueryHandler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            }

            // Handle method to execute the logic for fetching paginated orders with optional filtering.
            public async Task<PaginationModel<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Step 1: Start building a query from the Orders repository using IQueryable.
                var query = _unitOfWork
                    .Orders
                    .AsQueryable() // Allows chaining further filtering and ordering.
                    .Include(x => x.Items) // Include related order items.
                    .Include(x => x.Customer) // Include related customer data.
                                              // Step 2: Apply filtering based on the filter string (if provided).
                    .Where(p => string.IsNullOrEmpty(request.filter) ? true : // If filter is empty, return all orders.
                        p.CustomerId.ToString().ToLower().Contains(request.filter) || // Match CustomerId.
                        p.OrderDate.ToString().ToLower().Contains(request.filter) || // Match OrderDate.
                        p.TotalPrice.ToString().ToLower().Contains(request.filter) || // Match TotalPrice.
                        p.Customer.Id.ToString().ToLower().Contains(request.filter) || // Match Customer Id.
                        p.Customer.FirstName.ToLower().Contains(request.filter) || // Match Customer FirstName.
                        p.Customer.LastName.ToLower().Contains(request.filter) || // Match Customer LastName.
                        p.Customer.Address.ToLower().Contains(request.filter) || // Match Customer Address.
                        p.Customer.PostalCode.ToLower().Contains(request.filter) // Match Customer PostalCode.
                )
                    .OrderByDescending(q => q.Id) // Step 3: Sort the orders by Id in descending order.
                                                  // Step 4: Project the result into the Response object with related data.
                    .Select(q => new Response(
                        q.Id,
                        q.OrderDate,
                        q.TotalPrice,
                        q.Items.Select(it => new OrderItemResponse(it.Id, it.OrderId, it.ProductId, it.Quantity, // Map Order Items.
                        new ProductResponse(it.Product.Id, it.Product.Name, it.Product.Price))), // Map Product details.
                        new CustomerResponse(q.Customer.Id, q.Customer.FirstName, q.Customer.LastName, q.Customer.Address, q.Customer.PostalCode) // Map Customer details.
                    ));

                // Step 5: Return the paginated result using the requested page size and page number.
                return new PaginationModel<Response>(
                    await query.CountAsync(cancellationToken), // Get the total count of orders for pagination.
                    request.page, // Current page number.
                    request.size, // Page size.
                    await query
                        .Skip((request.page - 1) * request.size) // Skip records based on the page number.
                        .Take(request.size) // Take only the required number of records.
                        .ToListAsync(cancellationToken) // Convert to a list asynchronously.
                );
            }
        }

        // Response record representing the order details returned in the paginated response.
        public record Response(int id, DateTime orderDate, decimal totalPrice, IEnumerable<OrderItemResponse> OrderItemResponses, CustomerResponse CustomerResponse);

        // OrderItemResponse record representing each order item in the order.
        public record OrderItemResponse(int id, int orderId, int productId, int quantity, ProductResponse ProductResponse);

        // ProductResponse record representing the product details in each order item.
        public record ProductResponse(int id, string name, decimal price);

        // CustomerResponse record representing the customer details for the order.
        public record CustomerResponse(int id, string firstName, string lastName, string address, string postalCode);
    }
}
