using Moq;
using CustomerOrderAPI.Application.DTO.Order;
using CustomerOrderAPI.Application.DTO.Item;
using CustomerOrderAPI.Domain.Entities;
using CustomerOrderAPI.Domain.Interface;
using CustomerOrderAPI.Application.Features.V1.Orders;
using MediatR;

namespace CustomerOrderAPI.Tests.Features.V1.Orders
{
    [TestFixture]
    public class AddOrderCommandHandlerTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Add.CommandHandler _commandHandler;

        [SetUp]
        public void SetUp()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _commandHandler = new Add.CommandHandler(_unitOfWorkMock.Object);
        }

        [Test]
        public async Task Handle_ShouldAddOrderAndCalculateTotalPriceCorrectly()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                CustomerId = 1,
                OrderDate = System.DateTime.Now,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = 1, Quantity = 2 },
                    new OrderItemDto { ProductId = 2, Quantity = 3 }
                }
            };

            var command = new Add.Command(orderDto);

            var product1 = new Product { Id = 1, Price = 10m };
            var product2 = new Product { Id = 2, Price = 20m };

            _unitOfWorkMock.Setup(uow => uow.Products.GetByIdAsync(1)).ReturnsAsync(product1);
            _unitOfWorkMock.Setup(uow => uow.Products.GetByIdAsync(2)).ReturnsAsync(product2);

            _unitOfWorkMock.Setup(uow => uow.Orders.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);


            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Products.GetByIdAsync(1), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.Products.GetByIdAsync(2), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.Orders.AddAsync(It.IsAny<Order>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);

            Assert.AreEqual(Unit.Value, result);

            // Check that the total price was calculated correctly
            _unitOfWorkMock.Verify(uow => uow.Orders.AddAsync(It.Is<Order>(o => o.TotalPrice == 80m)), Times.Once);
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
