using Moq;
using CustomerOrderAPI.Application.Features.V1.Products;
using CustomerOrderAPI.Domain.Entities;
using CustomerOrderAPI.Domain.Interface;
using MockQueryable;

namespace CustomerOrderAPI.Tests.Application.Features.V1.Products
{
    [TestFixture]
    public class GetProductsQueryHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IRepository<Product>> _mockProductRepository;
        private Get.QueryHandler _queryHandler;

        [SetUp]
        public void SetUp()
        {
            // Initialize the mocks
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockProductRepository = new Mock<IRepository<Product>>();

            // Set up UnitOfWork to return mocked Product repository
            _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepository.Object);

            // Initialize the query handler with the mocked unit of work
            _queryHandler = new Get.QueryHandler(_mockUnitOfWork.Object);
        }

        [Test]
        public async Task Handle_ShouldReturnPaginatedListOfProducts_WhenProductsExist()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", Price = 10.50m },
                new Product { Id = 2, Name = "Product 2", Price = 20.00m },
                new Product { Id = 3, Name = "Product 3", Price = 30.00m }
            }.AsQueryable();

            var mockDbSet = products.BuildMock(); // Using MockQueryable to mock IQueryable

            _mockProductRepository.Setup(r => r.AsQueryable()).Returns(mockDbSet);

            var query = new Get.Query(page: 1, size: 2, filter: "Product 2");

            // Act
            var result = await _queryHandler.Handle(query, CancellationToken.None);

            // Assert
            Assert.AreEqual(1, result.Results.Count());  // Expect 1 result matching "Product 2"
            Assert.AreEqual(1, result.Total);            // Total matching items is 1
            Assert.AreEqual("Product 2", result.Results.First().name);  // Ensure it's the correct product

            // Verify that AsQueryable() was called once
            _mockProductRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Test]
        public async Task Handle_ShouldReturnEmptyList_WhenNoProductsMatchFilter()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", Price = 10.50m },
                new Product { Id = 2, Name = "Product 2", Price = 20.00m }
            }.AsQueryable();

            var mockDbSet = products.BuildMock(); // MockQueryable to mock IQueryable

            _mockProductRepository.Setup(r => r.AsQueryable()).Returns(mockDbSet);

            // This filter won't match any product
            var query = new Get.Query(page: 1, size: 2, filter: "NonExistingProduct");

            // Act
            var result = await _queryHandler.Handle(query, CancellationToken.None);

            // Assert
            Assert.AreEqual(0, result.Results.Count());  // Expect no products to match the filter
            Assert.AreEqual(0, result.Total);            // Total matching items is 0

            // Verify that AsQueryable() was called once
            _mockProductRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Test]
        public async Task Handle_ShouldReturnPaginatedListOfProducts_WhenNoFilterIsProvided()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", Price = 10.50m },
                new Product { Id = 2, Name = "Product 2", Price = 20.00m },
                new Product { Id = 3, Name = "Product 3", Price = 30.00m }
            }.AsQueryable();

            var mockDbSet = products.BuildMock(); // Using MockQueryable to mock IQueryable

            _mockProductRepository.Setup(r => r.AsQueryable()).Returns(mockDbSet);

            var query = new Get.Query(page: 1, size: 2); // No filter

            // Act
            var result = await _queryHandler.Handle(query, CancellationToken.None);

            // Assert
            Assert.AreEqual(2, result.Results.Count());  // Expect 2 products on the first page
            Assert.AreEqual(3, result.Total);            // Total products should be 3
            Assert.AreEqual("Product 3", result.Results.First().name);  // Ensure correct ordering by Id (descending)

            // Verify that AsQueryable() was called once
            _mockProductRepository.Verify(r => r.AsQueryable(), Times.Once);
        }
    }
}
