using Moq;
using CustomerOrderAPI.Application.Features.V1.Customers;
using CustomerOrderAPI.Domain.Interface;
using CustomerOrderAPI.Application.DTO.Customer;
using CustomerOrderAPI.Domain.Entities;
using MediatR;

namespace CustomerOrderAPI.Tests.Application.Features.V1.Customers
{
    [TestFixture]
    public class AddCustomerCommandHandlerTests
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
        public async Task Handle_ShouldAddCustomerAndSaveChanges_WhenValidCustomerIsPassed()
        {
            // Arrange
            var customerDto = new CustomerDto
            {
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Main St",
                PostalCode = "12345"
            };
            var command = new Add.Command(customerDto);

            _mockUnitOfWork.Setup(u => u.Customers.AddAsync(It.IsAny<Customer>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1); // Simulate that 1 change was saved

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            _mockUnitOfWork.Verify(u => u.Customers.AddAsync(It.Is<Customer>(c =>
                c.FirstName == customerDto.FirstName &&
                c.LastName == customerDto.LastName &&
                c.Address == customerDto.Address &&
                c.PostalCode == customerDto.PostalCode
            )), Times.Once);

            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.AreEqual(Unit.Value, result);
        }
    }
}
