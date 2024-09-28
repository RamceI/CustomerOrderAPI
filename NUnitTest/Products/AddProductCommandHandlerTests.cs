using Moq;
using CustomerOrderAPI.Application.Features.V1.Products;
using CustomerOrderAPI.Domain.Interface;
using CustomerOrderAPI.Application.DTO.Product;
using CustomerOrderAPI.Domain.Entities;
using MediatR;

namespace CustomerOrderAPI.Tests.Application.Features.V1.Products
{
    [TestFixture]
    public class AddProductCommandHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Add.CommandHandler _commandHandler;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _commandHandler = new Add.CommandHandler(_mockUnitOfWork.Object);
        }

        [Test]
        public async Task Handle_ShouldAddProductAndSaveChanges_WhenValidProductIsPassed()
        {
            // Arrange
            var productDto = new ProductDto
            {
                Name = "Laptop",
                Price = 49.99M
            };
            var command = new Add.Command(productDto);

            // Mock the AddAsync and SaveChangesAsync methods
            _mockUnitOfWork.Setup(u => u.Products.AddAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1); // Simulate that 1 change was saved

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            _mockUnitOfWork.Verify(u => u.Products.AddAsync(It.Is<Product>(p =>
                p.Name == productDto.Name &&
                p.Price == productDto.Price
            )), Times.Once);

            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.AreEqual(Unit.Value, result);
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenUnitOfWorkIsNull()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new Add.CommandHandler(null));

            // Ensure the parameter name in the exception matches
            Assert.AreEqual("unitOfWork", ex.ParamName);
        }

    }
}
