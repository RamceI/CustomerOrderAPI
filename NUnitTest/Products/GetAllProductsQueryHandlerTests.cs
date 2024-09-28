using Moq;
using CustomerOrderAPI.Application.Features.V1.Products;
using CustomerOrderAPI.Domain.Entities;
using CustomerOrderAPI.Domain.Interface;
using MockQueryable.Moq;

namespace CustomerOrderAPI.Tests.Application.Features.V1.Products
{
    [TestFixture]
    public class GetAllProductsQueryHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IRepository<Product>> _mockProductRepository;
        private GetAll.QueryHandler _queryHandler;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockProductRepository = new Mock<IRepository<Product>>();
            _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepository.Object);

            _queryHandler = new GetAll.QueryHandler(_mockUnitOfWork.Object);
        }

        [Test]
        public async Task Handle_ShouldReturnListOfProducts_WhenProductsExist()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", Price = 10.50m },
                new Product { Id = 2, Name = "Product 2", Price = 20.00m }
            }.AsQueryable();

            // Create a mock DbSet<Product> using MockQueryable to support async operations
            var mockDbSet = products.BuildMockDbSet();

            _mockProductRepository.Setup(r => r.AsQueryable()).Returns(mockDbSet.Object);

            // Act
            var result = await _queryHandler.Handle(new GetAll.Query(), CancellationToken.None);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Product 1", result[0].name);
            Assert.AreEqual(10.50m, result[0].price);
            Assert.AreEqual("Product 2", result[1].name);
            Assert.AreEqual(20.00m, result[1].price);

            _mockProductRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Test]
        public async Task Handle_ShouldReturnEmptyList_WhenNoProductsExist()
        {
            // Arrange
            var products = new List<Product>().AsQueryable();

            // Create a mock DbSet<Product> using MockQueryable
            var mockDbSet = products.BuildMockDbSet();

            _mockProductRepository.Setup(r => r.AsQueryable()).Returns(mockDbSet.Object);

            // Act
            var result = await _queryHandler.Handle(new GetAll.Query(), CancellationToken.None);

            // Assert
            Assert.IsEmpty(result);
            _mockProductRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenUnitOfWorkIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new GetAll.QueryHandler(null));
        }
    }
}
