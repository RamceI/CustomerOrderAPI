using Moq;
using CustomerOrderAPI.Domain.Entities;
using CustomerOrderAPI.Domain.Interface;
using CustomerOrderAPI.Application.Features.V1.Orders;
using MediatR;

namespace CustomerOrderAPI.Tests.Features.V1.Orders
{
    [TestFixture]
    public class DeleteOrderCommandHandlerTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Delete.CommandHandler _commandHandler;

        [SetUp]
        public void SetUp()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _commandHandler = new Delete.CommandHandler(_unitOfWorkMock.Object);
        }

        [Test]
        public async Task Handle_ShouldDeleteOrderAndAssociatedItems_WhenOrderExists()
        {
            // Arrange
            var existingOrder = new Order
            {
                Id = 1,
                CustomerId = 1,
                OrderDate = DateTime.Now,
                Items = new List<Item>
                {
                    new Item { ProductId = 1, Quantity = 2 },
                    new Item { ProductId = 2, Quantity = 1 }
                }
            };

            var command = new Delete.Command(existingOrder.Id);

            // Mock fetching the order
            _unitOfWorkMock.Setup(uow => uow.Orders.GetByIdAsync(existingOrder.Id)).ReturnsAsync(existingOrder);

            // Mock deletion of items and order
            _unitOfWorkMock.Setup(uow => uow.Items.Delete(It.IsAny<Item>()));
            _unitOfWorkMock.Setup(uow => uow.Orders.Delete(existingOrder));

            // Mock saving changes
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Orders.GetByIdAsync(existingOrder.Id), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.Items.Delete(It.IsAny<Item>()), Times.Exactly(existingOrder.Items.Count));
            _unitOfWorkMock.Verify(uow => uow.Orders.Delete(existingOrder), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);

            Assert.AreEqual(Unit.Value, result);
        }

        [Test]
        public void Handle_ShouldThrowException_WhenOrderNotFound()
        {
            // Arrange
            var command = new Delete.Command(1);

            // Mock fetching a non-existent order
            _unitOfWorkMock.Setup(uow => uow.Orders.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Order)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<Exception>(async () =>
                await _commandHandler.Handle(command, CancellationToken.None));

            Assert.AreEqual("Order not found", exception.Message);
        }

        [Test]
        public async Task Handle_ShouldDeleteOrderWithoutItems_WhenOrderHasNoItems()
        {
            // Arrange
            var existingOrder = new Order
            {
                Id = 1,
                CustomerId = 1,
                OrderDate = DateTime.Now,
                Items = new List<Item>() // No items
            };

            var command = new Delete.Command(existingOrder.Id);

            // Mock fetching the order
            _unitOfWorkMock.Setup(uow => uow.Orders.GetByIdAsync(existingOrder.Id)).ReturnsAsync(existingOrder);

            // Mock deletion of the order (no items to delete)
            _unitOfWorkMock.Setup(uow => uow.Orders.Delete(existingOrder));

            // Mock saving changes
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Orders.GetByIdAsync(existingOrder.Id), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.Items.Delete(It.IsAny<Item>()), Times.Never); // No items to delete
            _unitOfWorkMock.Verify(uow => uow.Orders.Delete(existingOrder), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);

            Assert.AreEqual(Unit.Value, result);
        }
    }
}
