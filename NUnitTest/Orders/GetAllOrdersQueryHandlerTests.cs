using Moq;
using CustomerOrderAPI.Domain.Entities;
using CustomerOrderAPI.Domain.Interface;
using CustomerOrderAPI.Application.Features.V1.Orders;
using MockQueryable.Moq;

namespace CustomerOrderAPI.Tests.Application.Features.V1.Orders
{
    [TestFixture]
    public class GetAllOrdersQueryHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IRepository<Order>> _mockOrderRepository;
        private GetAll.QueryHandler _queryHandler;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockOrderRepository = new Mock<IRepository<Order>>();
            _mockUnitOfWork.Setup(u => u.Orders).Returns(_mockOrderRepository.Object);

            _queryHandler = new GetAll.QueryHandler(_mockUnitOfWork.Object);
        }

        [Test]
        public async Task Handle_ShouldReturnListOfOrders_WhenOrdersExist()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    OrderDate = DateTime.Now,
                    TotalPrice = 100.00m,
                    Items = new List<Item>
                    {
                        new Item { Id = 1, OrderId = 1, ProductId = 1, Quantity = 2, Product = new Product { Id = 1, Name = "Product 1", Price = 10.50m } }
                    },
                    Customer = new Customer { Id = 1, FirstName = "John", LastName = "Doe", Address = "123 Street", PostalCode = "12345" }
                },
                new Order
                {
                    Id = 2,
                    OrderDate = DateTime.Now,
                    TotalPrice = 200.00m,
                    Items = new List<Item>
                    {
                        new Item { Id = 2, OrderId = 2, ProductId = 2, Quantity = 1, Product = new Product { Id = 2, Name = "Product 2", Price = 20.00m } }
                    },
                    Customer = new Customer { Id = 2, FirstName = "Jane", LastName = "Doe", Address = "456 Avenue", PostalCode = "67890" }
                }
            }.AsQueryable();

            // Create a mock DbSet<Order> using MockQueryable to support async operations
            var mockDbSet = orders.BuildMockDbSet();

            _mockOrderRepository.Setup(r => r.AsQueryable()).Returns(mockDbSet.Object);

            // Act
            var result = await _queryHandler.Handle(new GetAll.Query(), CancellationToken.None);

            // Assert
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("John", result[0].CustomerResponse.firstName);
            Assert.AreEqual(100.00m, result[0].totalPrice);
            Assert.AreEqual(2, result[0].OrderItemResponses.First().quantity);

            Assert.AreEqual("Jane", result[1].CustomerResponse.firstName);
            Assert.AreEqual(200.00m, result[1].totalPrice);
            Assert.AreEqual(1, result[1].OrderItemResponses.First().quantity);

            _mockOrderRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Test]
        public async Task Handle_ShouldReturnEmptyList_WhenNoOrdersExist()
        {
            // Arrange
            var orders = new List<Order>().AsQueryable();

            // Create a mock DbSet<Order> using MockQueryable
            var mockDbSet = orders.BuildMockDbSet();

            _mockOrderRepository.Setup(r => r.AsQueryable()).Returns(mockDbSet.Object);

            // Act
            var result = await _queryHandler.Handle(new GetAll.Query(), CancellationToken.None);

            // Assert
            Assert.IsEmpty(result);
            _mockOrderRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenUnitOfWorkIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new GetAll.QueryHandler(null));
        }
    }
}
