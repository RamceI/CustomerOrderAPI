using Moq;
using CustomerOrderAPI.Application.DTO.Product;
using CustomerOrderAPI.Domain.Entities;
using CustomerOrderAPI.Domain.Interface;
using CustomerOrderAPI.Application.Features.V1.Products;

namespace CustomerOrderAPI.Tests.Application.Features.V1.Products
{
    [TestFixture]
    public class UpdateProductCommandHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Update.CommandHandler _commandHandler;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _commandHandler = new Update.CommandHandler(_mockUnitOfWork.Object);
        }

        [Test]
        public async Task Handle_ShouldUpdateProduct_WhenProductExists()
        {
            // Arrange
            var productId = 1;
            var productDto = new ProductDto
            {
                Name = "Updated Product Name",
                Price = 99.99m
            };

            var existingProduct = new Product
            {
                Id = productId,
                Name = "Old Product Name",
                Price = 49.99m
            };

            var command = new Update.Command(productId, productDto);

            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId))
                .ReturnsAsync(existingProduct);

            _mockUnitOfWork.Setup(u => u.Products.Update(It.IsAny<Product>()));
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            _mockUnitOfWork.Verify(u => u.Products.GetByIdAsync(productId), Times.Once);
            _mockUnitOfWork.Verify(u => u.Products.Update(It.Is<Product>(p =>
                p.Id == productId &&
                p.Name == productDto.Name &&
                p.Price == productDto.Price
            )), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);

            Assert.AreEqual(productId, result.id);
        }

        [Test]
        public void Handle_ShouldThrowException_WhenProductNotFound()
        {
            // Arrange
            var productId = 1;
            var productDto = new ProductDto
            {
                Name = "Laptop",
                Price = 99.99m
            };

            var command = new Update.Command(productId, productDto);

            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId))
                .ReturnsAsync((Product)null); // Simulate product not found

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _commandHandler.Handle(command, CancellationToken.None));
            Assert.AreEqual("Product not found", ex.Message);

            _mockUnitOfWork.Verify(u => u.Products.GetByIdAsync(productId), Times.Once);
            _mockUnitOfWork.Verify(u => u.Products.Update(It.IsAny<Product>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
    }
}
