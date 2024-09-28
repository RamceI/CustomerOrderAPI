using NUnit.Framework;
using Moq;
using CustomerOrderAPI.Application.Features.V1.Customers;
using CustomerOrderAPI.Domain.Interface;
using CustomerOrderAPI.Domain.Entities;
using MediatR;

namespace CustomerOrderAPI.Tests.Application.Features.V1.Customers
{
    [TestFixture]
    public class DeleteCustomerCommandHandlerTests
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
        public async Task Handle_ShouldDeleteCustomer_WhenCustomerExists()
        {
            // Arrange
            var customerId = 1;
            var existingCustomer = new Customer
            {
                Id = customerId,
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Main St",
                PostalCode = "12345"
            };

            var command = new Delete.Command(customerId);

            // Mock the GetByIdAsync method to return the customer
            _mockUnitOfWork.Setup(u => u.Customers.GetByIdAsync(customerId))
                .ReturnsAsync(existingCustomer);

            // Mock the SaveChangesAsync method to simulate successful save
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            _mockUnitOfWork.Verify(u => u.Customers.GetByIdAsync(It.Is<int>(id => id == customerId)), Times.Once);
            _mockUnitOfWork.Verify(u => u.Customers.Delete(It.Is<Customer>(c => c.Id == customerId)), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);

            Assert.AreEqual(Unit.Value, result);
        }

        [Test]
        public void Handle_ShouldThrowException_WhenCustomerNotFound()
        {
            // Arrange
            var customerId = 1;
            var command = new Delete.Command(customerId);

            // Mock the GetByIdAsync method to return null (customer not found)
            _mockUnitOfWork.Setup(u => u.Customers.GetByIdAsync(customerId))
                .ReturnsAsync((Customer)null);

            // Act & Assert
            var exception = Assert.ThrowsAsync<Exception>(async () =>
                await _commandHandler.Handle(command, CancellationToken.None));

            // Assert the exception message
            Assert.AreEqual("Customer not found", exception.Message);

            _mockUnitOfWork.Verify(u => u.Customers.GetByIdAsync(It.Is<int>(id => id == customerId)), Times.Once);
            _mockUnitOfWork.Verify(u => u.Customers.Delete(It.IsAny<Customer>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
    }
}
