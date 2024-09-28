using Moq;
using CustomerOrderAPI.Application.Features.V1.Orders;
using CustomerOrderAPI.Domain.Entities;
using CustomerOrderAPI.Domain.Interface;
using MockQueryable;

namespace CustomerOrderAPI.Tests.Application.Features.V1.Orders
{
    [TestFixture]
    public class GetCustomerOrdersByDateQueryHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IRepository<Order>> _mockOrderRepository;
        private GetCustomerOrdersByDate.QueryHandler _queryHandler;

        [SetUp]
        public void Setup()
        {
            // Initialize the mocks
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockOrderRepository = new Mock<IRepository<Order>>();

            // Set up UnitOfWork to return mocked Order repository
            _mockUnitOfWork.Setup(u => u.Orders).Returns(_mockOrderRepository.Object);
            _queryHandler = new GetCustomerOrdersByDate.QueryHandler(_mockUnitOfWork.Object);
        }

        [Test]
        public async Task Handle_ShouldReturnCustomerOrdersSortedByOrderDate_WhenCustomerIdMatches()
        {
            // Arrange
            var customerId = 1;
            var orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    CustomerId = customerId,
                    OrderDate = new DateTime(2023, 9, 1),
                    TotalPrice = 100.00m,
                    Customer = new Customer { Id = customerId, FirstName = "John", LastName = "Doe", Address = "123 Main St", PostalCode = "12345" },
                    Items = new List<Item>
                    {
                        new Item { Id = 1, OrderId = 1, ProductId = 1, Quantity = 2, Product = new Product { Id = 1, Name = "Product 1", Price = 50.00m } }
                    }
                },
                new Order
                {
                    Id = 2,
                    CustomerId = customerId,
                    OrderDate = new DateTime(2023, 8, 1),
                    TotalPrice = 150.00m,
                    Customer = new Customer { Id = customerId, FirstName = "John", LastName = "Doe", Address = "123 Main St", PostalCode = "12345" },
                    Items = new List<Item>
                    {
                        new Item { Id = 2, OrderId = 2, ProductId = 2, Quantity = 3, Product = new Product { Id = 2, Name = "Product 2", Price = 50.00m } }
                    }
                }
            }.AsQueryable();

            var mockOrders = orders.BuildMock(); // Mock IQueryable for orders

            _mockUnitOfWork.Setup(uow => uow.Orders.AsQueryable()).Returns(mockOrders);

            var query = new GetCustomerOrdersByDate.Query(customerId);

            // Act
            var result = await _queryHandler.Handle(query, CancellationToken.None);

            // Assert
            Assert.AreEqual(2, result.Count); // Check that 2 orders were returned
            Assert.AreEqual(2, result[0].Id); // Check that the first order is sorted by OrderDate (earliest date first)
            Assert.AreEqual(1, result[1].Id); // Check that the second order is the most recent one
            Assert.AreEqual("Product 2", result[0].OrderItemResponses.First().ProductResponse.Name); // Validate product name of the first order
            _mockUnitOfWork.Verify(uow => uow.Orders.AsQueryable(), Times.Once); // Ensure AsQueryable was called once
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenUnitOfWorkIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new GetCustomerOrdersByDate.QueryHandler(null));
        }
    }
}
