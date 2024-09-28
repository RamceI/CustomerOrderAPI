using MediatR;
using Microsoft.EntityFrameworkCore;
using CustomerOrderAPI.Domain.Interface;

namespace CustomerOrderAPI.Application.Features.V1.Orders
{
    // GetAll class to retrieve all orders without pagination or filtering.
    public class GetAll
    {
        // Query record that serves as a request for all orders. It returns a list of Response objects.
        public record Query : IRequest<List<Response>>;

        // QueryHandler class to handle the GetAll Query using MediatR.
        public class QueryHandler : IRequestHandler<Query, List<Response>>
        {
            private readonly IUnitOfWork _unitOfWork; // UnitOfWork to handle database operations.

            // Constructor to initialize the QueryHandler with the IUnitOfWork dependency.
            public QueryHandler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            }

            // Handle method to execute the logic for fetching all orders.
            public async Task<List<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Step 1: Query the Orders repository using IQueryable, allowing us to include related data.
                return await _unitOfWork.Orders
                    .AsQueryable() // Start with an IQueryable to enable further manipulation.
                    .Include(x => x.Items) // Include related order items.
                    .Include(x => x.Customer) // Include related customer details.
                    .OrderBy(c => c.Id) // Step 2: Order the results by order ID in ascending order.
                                        // Step 3: Project the result into the Response object with related order, item, and customer data.
                    .Select(q => new Response(
                        q.Id, // Order ID.
                        q.OrderDate, // Date the order was placed.
                        q.TotalPrice, // Total price of the order.
                        q.Items.Select(it => new OrderItemResponse(
                            it.Id, // Order item ID.
                            it.OrderId, // Related order ID.
                            it.ProductId, // Related product ID.
                            it.Quantity, // Quantity of the product.
                            new ProductResponse(it.Product.Id, it.Product.Name, it.Product.Price) // Map Product details.
                        )),
                        new CustomerResponse(q.Customer.Id, q.Customer.FirstName, q.Customer.LastName, q.Customer.Address, q.Customer.PostalCode) // Map Customer details.
                    ))
                    .ToListAsync(cancellationToken); // Step 4: Execute the query and return a list asynchronously.
            }
        }

        // Response record representing the details of the order that is returned.
        public record Response(int id, DateTime orderDate, decimal totalPrice, IEnumerable<OrderItemResponse> OrderItemResponses, CustomerResponse CustomerResponse);

        // OrderItemResponse record representing the details of each order item.
        public record OrderItemResponse(int id, int orderId, int productId, int quantity, ProductResponse ProductResponse);

        // ProductResponse record representing the product details for each order item.
        public record ProductResponse(int id, string name, decimal price);

        // CustomerResponse record representing the customer details related to the order.
        public record CustomerResponse(int id, string firstName, string lastName, string address, string postalCode);
    }
}
