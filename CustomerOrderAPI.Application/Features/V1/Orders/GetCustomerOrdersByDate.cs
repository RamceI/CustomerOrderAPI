using MediatR;
using Microsoft.EntityFrameworkCore;
using CustomerOrderAPI.Domain.Interface;

namespace CustomerOrderAPI.Application.Features.V1.Orders
{
    // GetCustomerOrdersByDate class to fetch orders of a specific customer, sorted by OrderDate.
    public class GetCustomerOrdersByDate
    {
        // Query record that encapsulates the customer ID. It returns a list of Response objects (customer's orders).
        public record Query(int CustomerId) : IRequest<List<Response>>;

        // QueryHandler class to handle the GetCustomerOrdersByDate Query using MediatR.
        public class QueryHandler : IRequestHandler<Query, List<Response>>
        {
            private readonly IUnitOfWork _unitOfWork; // UnitOfWork to manage data access.

            // Constructor to initialize the QueryHandler with the IUnitOfWork dependency.
            public QueryHandler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            }

            // Handle method that contains the logic for fetching orders by customer ID.
            public async Task<List<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Step 1: Fetch orders for the specified customer using their CustomerId.
                return await _unitOfWork.Orders
                    .AsQueryable() // Use IQueryable for flexibility in further query manipulations.
                    .Include(x => x.Items) // Include related order items.
                    .ThenInclude(x => x.Product) // Include product details for each order item.
                    .Include(x => x.Customer) // Include customer details.
                    .Where(x => x.CustomerId == request.CustomerId)  // Step 2: Filter the orders by the provided CustomerId.
                    .OrderBy(x => x.OrderDate)  // Step 3: Sort the results by OrderDate in ascending order.
                                                // Step 4: Project the result into the Response object with related order, item, and customer details.
                    .Select(q => new Response(
                        q.Id, // Order ID.
                        q.OrderDate, // Date the order was placed.
                        q.TotalPrice, // Total price of the order.
                        q.Items.Select(it => new OrderItemResponse(
                            it.Id, // Order item ID.
                            it.OrderId, // Related order ID.
                            it.ProductId, // Related product ID.
                            it.Quantity, // Quantity of the product.
                            new ProductResponse(it.Product.Id, it.Product.Name, it.Product.Price) // Map product details.
                        )),
                        new CustomerResponse(q.Customer.Id, q.Customer.FirstName, q.Customer.LastName, q.Customer.Address, q.Customer.PostalCode) // Map customer details.
                    ))
                    .ToListAsync(cancellationToken); // Step 5: Execute the query and return the list asynchronously.
            }
        }

        // Response DTO representing the details of the order that is returned.
        public record Response(int Id, DateTime OrderDate, decimal TotalPrice, IEnumerable<OrderItemResponse> OrderItemResponses, CustomerResponse CustomerResponse);

        // OrderItemResponse DTO representing the details of each order item.
        public record OrderItemResponse(int Id, int OrderId, int ProductId, int Quantity, ProductResponse ProductResponse);

        // ProductResponse DTO representing the product details for each order item.
        public record ProductResponse(int Id, string Name, decimal Price);

        // CustomerResponse DTO representing the customer details related to the order.
        public record CustomerResponse(int Id, string FirstName, string LastName, string Address, string PostalCode);
    }
}
