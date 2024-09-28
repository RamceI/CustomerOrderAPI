using Moq;
using CustomerOrderAPI.Application.Features.V1.Customers;
using CustomerOrderAPI.Domain.Interface;
using CustomerOrderAPI.Application.DTO.Customer;
using CustomerOrderAPI.Domain.Entities;


namespace CustomerOrderAPI.Tests.Application.Features.V1.Customers
{
    [TestFixture]
    public class UpdateCustomerCommandHandlerTests
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
        public async Task Handle_ShouldUpdateCustomer_WhenValidCustomerIsPassed()
        {
            // Arrange
            var customerDto = new CustomerDto
            {
                FirstName = "Jane",
                LastName = "Doe",
                Address = "456 Main St",
                PostalCode = "54321"
            };
            var customerId = 1;
            var command = new Update.Command(customerId, customerDto);

            var existingCustomer = new Customer
            {
                Id = customerId,
                FirstName = "John",
                LastName = "Smith",
                Address = "123 Main St",
                PostalCode = "12345"
            };

            // Mock the GetByIdAsync method to return an existing customer
            _mockUnitOfWork.Setup(u => u.Customers.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(existingCustomer);

            // Mock the SaveChangesAsync method to simulate that changes were saved
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            _mockUnitOfWork.Verify(u => u.Customers.GetByIdAsync(It.Is<int>(id => id == customerId)), Times.Once);

            _mockUnitOfWork.Verify(u => u.Customers.Update(It.Is<Customer>(c =>
                c.FirstName == customerDto.FirstName &&
                c.LastName == customerDto.LastName &&
                c.Address == customerDto.Address &&
                c.PostalCode == customerDto.PostalCode
            )), Times.Once);

            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);

            Assert.AreEqual(customerId, result.id);
        }

        [Test]
        public void Handle_ShouldThrowException_WhenCustomerNotFound()
        {
            // Arrange
            var customerDto = new CustomerDto
            {
                FirstName = "Jane",
                LastName = "Doe",
                Address = "456 Main St",
                PostalCode = "54321"
            };
            var customerId = 1;
            var command = new Update.Command(customerId, customerDto);

            // Mock the GetByIdAsync method to return null (customer not found)
            _mockUnitOfWork.Setup(u => u.Customers.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Customer)null);

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => await _commandHandler.Handle(command, CancellationToken.None), "Customer not found");

            _mockUnitOfWork.Verify(u => u.Customers.GetByIdAsync(It.Is<int>(id => id == customerId)), Times.Once);
            _mockUnitOfWork.Verify(u => u.Customers.Update(It.IsAny<Customer>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
    }
}
