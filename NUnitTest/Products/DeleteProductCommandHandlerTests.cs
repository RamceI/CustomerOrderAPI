using Moq;
using CustomerOrderAPI.Domain.Entities;
using CustomerOrderAPI.Domain.Interface;
using CustomerOrderAPI.Application.Features.V1.Products;
using MediatR;

namespace CustomerOrderAPI.Tests.Application.Features.V1.Products
{
    [TestFixture]
    public class DeleteProductCommandHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Delete.CommandHandler _commandHandler;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _commandHandler = new Delete.CommandHandler(_mockUnitOfWork.Object);
        }

        [Test]
        public async Task Handle_ShouldDeleteProduct_WhenProductExists()
        {
            // Arrange
            var productId = 1;
            var existingProduct = new Product
            {
                Id = productId,
                Name = "Laptop",
                Price = 99.99m
            };

            var command = new Delete.Command(productId);

            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId))
                .ReturnsAsync(existingProduct);

            _mockUnitOfWork.Setup(u => u.Products.Delete(It.IsAny<Product>()));
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            _mockUnitOfWork.Verify(u => u.Products.GetByIdAsync(productId), Times.Once);
            _mockUnitOfWork.Verify(u => u.Products.Delete(It.Is<Product>(p => p.Id == productId)), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);

            Assert.AreEqual(Unit.Value, result);
        }

        [Test]
        public void Handle_ShouldThrowException_WhenProductNotFound()
        {
            // Arrange
            var productId = 1;
            var command = new Delete.Command(productId);

            _mockUnitOfWork.Setup(u => u.Products.GetByIdAsync(productId))
                .ReturnsAsync((Product)null); // Simulate product not found

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(() => _commandHandler.Handle(command, CancellationToken.None));
            Assert.AreEqual("Customer not found", ex.Message);

            _mockUnitOfWork.Verify(u => u.Products.GetByIdAsync(productId), Times.Once);
            _mockUnitOfWork.Verify(u => u.Products.Delete(It.IsAny<Product>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
    }
}
