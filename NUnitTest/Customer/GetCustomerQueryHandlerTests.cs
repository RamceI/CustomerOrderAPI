using Moq;
using CustomerOrderAPI.Domain.Interface;
using CustomerOrderAPI.Domain.Entities;
using CustomerOrderAPI.Application.Features.V1.Customers;
using MockQueryable;

namespace CustomerOrderAPI.Tests.Application.Features.V1.Customers
{
    [TestFixture]
    public class GetCustomerQueryHandlerTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IRepository<Customer>> _mockCustomerRepository;
        private Get.QueryHandler _queryHandler;

        [SetUp]
        public void SetUp()
        {
            // Initialize the mocks
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCustomerRepository = new Mock<IRepository<Customer>>();

            // Set up UnitOfWork to return mocked Customer repository
            _mockUnitOfWork.Setup(u => u.Customers).Returns(_mockCustomerRepository.Object);

            // Initialize the query handler with the mocked unit of work
            _queryHandler = new Get.QueryHandler(_mockUnitOfWork.Object);
        }

        [Test]
        public async Task Handle_ShouldReturnPaginatedListOfCustomers_WhenCustomersExist()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { Id = 1, FirstName = "John", LastName = "Doe", Address = "123 Main St", PostalCode = "12345" },
                new Customer { Id = 2, FirstName = "Jane", LastName = "Smith", Address = "456 Elm St", PostalCode = "67890" },
                new Customer { Id = 3, FirstName = "Mike", LastName = "Johnson", Address = "789 Oak St", PostalCode = "13579" }
            }.AsQueryable();

            var mockDbSet = customers.BuildMock(); // Using MockQueryable to mock IQueryable

            _mockCustomerRepository.Setup(r => r.AsQueryable()).Returns(mockDbSet);

            var query = new Get.Query(page: 1, size: 2, filter: "Jane");

            // Act
            var result = await _queryHandler.Handle(query, CancellationToken.None);

            // Assert
            Assert.AreEqual(1, result.Results.Count());  // Updated to 'Results' which contains the paginated items
            Assert.AreEqual(1, result.Total);            // Updated to 'Total' which represents the total count of items
            Assert.AreEqual("Jane", result.Results.First().firstName);  // Updated to 'Results' and 'FirstName', matching the Response class

            // Verify that AsQueryable() was called once
            _mockCustomerRepository.Verify(r => r.AsQueryable(), Times.Once);
        }

        [Test]
        public async Task Handle_ShouldReturnEmptyList_WhenNoCustomersMatchFilter()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { Id = 1, FirstName = "John", LastName = "Doe", Address = "123 Main St", PostalCode = "12345" },
                new Customer { Id = 2, FirstName = "Jane", LastName = "Smith", Address = "456 Elm St", PostalCode = "67890" }
            }.AsQueryable();

            var mockDbSet = customers.BuildMock(); // MockQueryable to mock IQueryable

            _mockCustomerRepository.Setup(r => r.AsQueryable()).Returns(mockDbSet);

            // This filter won't match any customer
            var query = new Get.Query(page: 1, size: 2, filter: "NonExistingCustomer");

            // Act
            var result = await _queryHandler.Handle(query, CancellationToken.None);

            // Assert
            Assert.AreEqual(0, result.Results.Count());  // Updated to 'Results'
            Assert.AreEqual(0, result.Total);            // Updated to 'Total'

            // Verify that AsQueryable() was called once
            _mockCustomerRepository.Verify(r => r.AsQueryable(), Times.Once);
        }
    }
}
