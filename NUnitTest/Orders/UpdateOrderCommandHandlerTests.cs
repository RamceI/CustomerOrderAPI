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
    public class UpdateOrderCommandHandlerTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Update.CommandHandler _commandHandler;

        [SetUp]
        public void SetUp()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _commandHandler = new Update.CommandHandler(_unitOfWorkMock.Object);
        }

        [Test]
        public async Task Handle_ShouldUpdateOrderAndCalculateTotalPriceCorrectly()
        {
            // Arrange
            var existingOrder = new Order
            {
                Id = 1,
                CustomerId = 1,
                OrderDate = DateTime.Now.AddDays(-1),
                Items = new List<Item>
                {
                    new Item { ProductId = 1, Quantity = 1 }, // Existing item
                    new Item { ProductId = 2, Quantity = 2 }  // Existing item to be removed
                }
            };

            var orderDto = new OrderDto
            {
                CustomerId = 1,
                OrderDate = DateTime.Now,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = 1, Quantity = 3 },  // Updated item
                    new OrderItemDto { ProductId = 3, Quantity = 2 }   // New item
                }
            };

            var command = new Update.Command(existingOrder.Id, orderDto);

            var product1 = new Product { Id = 1, Price = 10m };
            var product3 = new Product { Id = 3, Price = 20m };

            // Mock fetching existing order
            _unitOfWorkMock.Setup(uow => uow.Orders.GetByIdAsync(existingOrder.Id)).ReturnsAsync(existingOrder);

            // Mock fetching product details
            _unitOfWorkMock.Setup(uow => uow.Products.GetByIdAsync(1)).ReturnsAsync(product1);
            _unitOfWorkMock.Setup(uow => uow.Products.GetByIdAsync(3)).ReturnsAsync(product3);

            // Mock update and save operations
            _unitOfWorkMock.Setup(uow => uow.Orders.Update(existingOrder));
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Orders.GetByIdAsync(existingOrder.Id), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.Products.GetByIdAsync(1), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.Products.GetByIdAsync(3), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.Orders.Update(It.IsAny<Order>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);

            Assert.AreEqual(Unit.Value, result);

            // Check that the total price was calculated correctly
            Assert.AreEqual(3 * 10m + 2 * 20m, existingOrder.TotalPrice); // 30 + 40 = 70

            // Ensure items were updated correctly
            Assert.AreEqual(2, existingOrder.Items.Count); // One item removed, one item added
            Assert.IsTrue(existingOrder.Items.Any(i => i.ProductId == 1 && i.Quantity == 3)); // Updated
            Assert.IsTrue(existingOrder.Items.Any(i => i.ProductId == 3 && i.Quantity == 2)); // New item added
        }

        [Test]
        public void Handle_ShouldThrowException_WhenOrderNotFound()
        {
            // Arrange
            var command = new Update.Command(1, new OrderDto());

            _unitOfWorkMock.Setup(uow => uow.Orders.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Order)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<Exception>(async () =>
                await _commandHandler.Handle(command, CancellationToken.None));

            Assert.AreEqual("Order not found", exception.Message);
        }

        [Test]
        public void ToEntityAsync_ShouldThrowException_WhenProductNotFound()
        {
            // Arrange
            var existingOrder = new Order
            {
                Id = 1,
                CustomerId = 1,
                OrderDate = DateTime.Now,
                Items = new List<Item>()
            };

            var orderDto = new OrderDto
            {
                CustomerId = 1,
                OrderDate = DateTime.Now,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = 1, Quantity = 2 } // Product that doesn't exist
                }
            };

            var command = new Update.Command(1, orderDto);

            // Mock fetching existing order
            _unitOfWorkMock.Setup(uow => uow.Orders.GetByIdAsync(existingOrder.Id)).ReturnsAsync(existingOrder);

            // Mock fetching product details with null result (product not found)
            _unitOfWorkMock.Setup(uow => uow.Products.GetByIdAsync(1)).ReturnsAsync((Product)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<Exception>(async () =>
                await command.ToEntityAsync(existingOrder, _unitOfWorkMock.Object));

            Assert.AreEqual("Product with ID 1 not found", exception.Message);
        }
    }
}
