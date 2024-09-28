using Microsoft.AspNetCore.Mvc;
using MediatR;
using CustomerOrderAPI.Application.Features.V1.Customers;
using CustomerOrderAPI.Application.DTO.Customer;
using System.Threading.Tasks;

namespace CustomerOrderAPI.Controllers.V1
{
    /// <summary>
    /// Controller responsible for handling customer-related API requests.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Constructor to initialize the CustomerController.
        /// </summary>
        /// <param name="mediator">IMediator service for handling commands and queries.</param>
        public CustomerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Adds a new customer.
        /// </summary>
        /// <param name="customer">Customer data transfer object.</param>
        /// <returns>A newly created customer.</returns>
        /// <response code="200">Returns the newly created customer.</response>
        /// <response code="400">If the customer DTO is invalid.</response>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerDto customer)
        {
            var command = new Add.Command(customer); // Pass the DTO into the command
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves paginated customers with an optional filter.
        /// </summary>
        /// <param name="page">The page number.</param>
        /// <param name="size">The number of items per page.</param>
        /// <param name="filter">An optional filter parameter for customer search.</param>
        /// <returns>A list of customers based on the paging and filter parameters.</returns>
        /// <response code="200">Returns the list of customers.</response>
        [HttpGet("{page}/{size}/{filter?}")]
        public async Task<IActionResult> Get(int page, int size, string? filter = null)
        {
            var result = await _mediator.Send(new Get.Query(page, size, filter));
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all customers.
        /// </summary>
        /// <returns>A list of all customers.</returns>
        /// <response code="200">Returns the list of all customers.</response>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAll.Query());
            return Ok(result);
        }

        /// <summary>
        /// Updates a customer by ID.
        /// </summary>
        /// <param name="id">The ID of the customer to update.</param>
        /// <param name="customerDto">Updated customer data transfer object.</param>
        /// <returns>The updated customer details.</returns>
        /// <response code="200">Returns the updated customer details.</response>
        /// <response code="404">If the customer with the given ID is not found.</response>
        [HttpPut("{id}/update")]
        public async Task<IActionResult> Update(int id, CustomerDto customerDto)
        {
            var command = new Update.Command(id, customerDto);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a customer by ID.
        /// </summary>
        /// <param name="id">The ID of the customer to delete.</param>
        /// <returns>An action result indicating success or failure.</returns>
        /// <response code="200">If the customer is successfully deleted.</response>
        /// <response code="404">If the customer with the given ID is not found.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new Delete.Command(id));
            return Ok(result);
        }
    }
}
