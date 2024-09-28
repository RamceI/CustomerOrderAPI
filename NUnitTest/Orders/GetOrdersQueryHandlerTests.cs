using Moq;
using CustomerOrderAPI.Application.Features.V1.Orders;
using CustomerOrderAPI.Domain.Entities;
using CustomerOrderAPI.Domain.Interface;
using MockQueryable;

namespace CustomerOrderAPI.Tests.Application.Features.V1.Orders
{
    [TestFixture]
    public class GetOrdersQueryHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IRepository<Order>> _mockOrderRepository;
        private Get.QueryHandler _queryHandler;

        [SetUp]
        public void SetUp()
        {
            // Initialize the mocks
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockOrderRepository = new Mock<IRepository<Order>>();

            // Set up UnitOfWork to return mocked Order repository
            _mockUnitOfWork.Setup(u => u.Orders).Returns(_mockOrderRepository.Object);

            // Initialize the query handler with the mocked unit of work
            _queryHandler = new Get.QueryHandler(_mockUnitOfWork.Object);
        }

        [Test]
        public async Task Handle_ShouldReturnPaginatedListOfOrders_WhenOrdersExist()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = 1, OrderDate = DateTime.Now, TotalPrice = 100.00m, CustomerId = 1, Customer = new Customer { FirstName = "John", LastName = "Doe", Address = "123 Main St", PostalCode = "12345" } },
                new Order { Id = 2, OrderDate = DateTime.Now.AddDays(-1), TotalPrice = 150.00m, CustomerId = 2, Customer = new Customer { FirstName = "Jane", LastName = "Doe", Address = "456 Market St", PostalCode = "67890" } },
                new Order { Id = 3, OrderDate = DateTime.Now.AddDays(-2), TotalPrice = 200.00m, CustomerId = 3, Customer = new Customer { FirstName = "Alex", LastName = "Smith", Address = "789 Oak St", PostalCode = "11223" } }
            }.AsQueryable();

            var mockDbSet = orders.BuildMock(); // Mocking IQueryable

            _mockOrderRepository.Setup(r => r.AsQueryable()).Returns(mockDbSet);

            var query = new Get.Query(page: 1, size: 2); // No filter, just pagination

            // Act
            var result = await _queryHandler.Handle(query, CancellationToken.None);

            // Assert
            Assert.AreEqual(2, result.Results.Count()); // Expect 2 orders on the first page
            Assert.AreEqual(3, result.Total);           // Total orders should be 3
            Assert.AreEqual("Alex", result.Results.First().CustomerResponse.firstName); // Expect "Alex" first due to descending order

            // Verify that AsQueryable() was called once
            _mockOrderRepository.Verify(r => r.AsQueryable(), Times.Once);
        }


        [Test]
        public async Task Handle_ShouldReturnListOfFilteredOrders_WhenFilterMatches()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    OrderDate = DateTime.Now,
                    TotalPrice = 100.00m,
                    CustomerId = 1,
                    Customer = new Customer
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Address = "123 Main St",
                        PostalCode = "12345"
                    }
                },
                new Order
                {
                    Id = 2,
                    OrderDate = DateTime.Now.AddDays(-1),
                    TotalPrice = 150.00m,
                    CustomerId = 2,
                    Customer = new Customer
                    {
                        FirstName = "Jane",
                        LastName = "Doe",
                        Address = "456 Market St",
                        PostalCode = "67890"
                    }
                }
            }.AsQueryable();

            var mockDbSet = orders.BuildMock(); // Mocking IQueryable

            _mockOrderRepository.Setup(r => r.AsQueryable()).Returns(mockDbSet);

            // Filter by "Jane"
            var query = new Get.Query(page: 1, size: 2, filter: "jane");

            // Act
            var result = await _queryHandler.Handle(query, CancellationToken.None);

            // Assert
            Assert.AreEqual(1, result.Results.Count()); // Expect 1 matching result
            Assert.AreEqual(1, result.Total);           // Total matching orders should be 1
            Assert.AreEqual("Jane", result.Results.First().CustomerResponse.firstName); // Ensure "Jane" was returned

            // Verify that AsQueryable() was called once
            _mockOrderRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Test]
        public async Task Handle_ShouldReturnEmptyList_WhenNoOrdersMatchFilter()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = 1, OrderDate = DateTime.Now, TotalPrice = 100.00m, CustomerId = 1, Customer = new Customer { FirstName = "John", LastName = "Doe", Address = "123 Main St", PostalCode = "12345" } },
                new Order { Id = 2, OrderDate = DateTime.Now.AddDays(-1), TotalPrice = 150.00m, CustomerId = 2, Customer = new Customer { FirstName = "Jane", LastName = "Doe", Address = "456 Market St", PostalCode = "67890" } }
            }.AsQueryable();

            var mockDbSet = orders.BuildMock(); // Mocking IQueryable

            _mockOrderRepository.Setup(r => r.AsQueryable()).Returns(mockDbSet);

            // Filter by a non-existing value
            var query = new Get.Query(page: 1, size: 2, filter: "NonExistingFilter");

            // Act
            var result = await _queryHandler.Handle(query, CancellationToken.None);

            // Assert
            Assert.AreEqual(0, result.Results.Count()); // Expect no orders to match the filter
            Assert.AreEqual(0, result.Total);           // Total matching orders should be 0

            // Verify that AsQueryable() was called once
            _mockOrderRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenUnitOfWorkIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new Get.QueryHandler(null));
        }
    }
}
