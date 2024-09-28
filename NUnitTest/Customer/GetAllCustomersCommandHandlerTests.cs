using Moq;
using CustomerOrderAPI.Application.Features.V1.Customers;
using CustomerOrderAPI.Domain.Interface;
using CustomerOrderAPI.Domain.Entities;
using MockQueryable.Moq;

namespace CustomerOrderAPI.Tests.Application.Features.V1.Customers
{
    [TestFixture]
    public class GetAllCustomersQueryHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IRepository<Customer>> _mockCustomerRepository;
        private GetAll.QueryHandler _queryHandler;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCustomerRepository = new Mock<IRepository<Customer>>();
            _mockUnitOfWork.Setup(u => u.Customers).Returns(_mockCustomerRepository.Object);

            _queryHandler = new GetAll.QueryHandler(_mockUnitOfWork.Object);
        }

        [Test]
        public async Task Handle_ShouldReturnListOfCustomers_WhenCustomersExist()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { Id = 1, FirstName = "John", LastName = "Doe", Address = "123 Main St", PostalCode = "12345" },
                new Customer { Id = 2, FirstName = "Jane", LastName = "Smith", Address = "456 Elm St", PostalCode = "67890" }
            }.AsQueryable();

            // Create a mock DbSet<Customer> using MockQueryable to support async operations
            var mockDbSet = customers.BuildMockDbSet();

            _mockCustomerRepository.Setup(r => r.AsQueryable()).Returns(mockDbSet.Object);

            // Act
            var result = await _queryHandler.Handle(new GetAll.Query(), CancellationToken.None);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("John", result[0].firstName);  // Correct property names (FirstName, LastName)
            Assert.AreEqual("Doe", result[0].lastName);
            Assert.AreEqual("Jane", result[1].firstName);
            Assert.AreEqual("Smith", result[1].lastName);

            _mockCustomerRepository.Verify(r => r.AsQueryable(), Times.Once);
        }


        [Test]
        public async Task Handle_ShouldReturnEmptyList_WhenNoCustomersExist()
        {
            // Arrange
            var customers = new List<Customer>().AsQueryable();

            // Create a mock DbSet<Customer> using MockQueryable
            var mockDbSet = customers.BuildMockDbSet();

            _mockCustomerRepository.Setup(r => r.AsQueryable()).Returns(mockDbSet.Object);

            // Act
            var result = await _queryHandler.Handle(new GetAll.Query(), CancellationToken.None);

            // Assert
            Assert.IsEmpty(result);
            _mockCustomerRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_WhenUnitOfWorkIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new GetAll.QueryHandler(null));
        }
    }
}
