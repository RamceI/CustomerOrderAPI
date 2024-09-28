using CustomerOrderAPI.Application.DTO.Order;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using CustomerOrderAPI.Application.Features.V1.Orders;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace CustomerOrderAPI.Controllers.V1
{
    /// <summary>
    /// Controller responsible for handling order-related API requests.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderController"/> class.
        /// </summary>
        /// <param name="mediator">IMediator service for handling commands and queries.</param>
        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a new order.
        /// </summary>
        /// <param name="orderDto">The order data transfer object (DTO) containing order details.</param>
        /// <returns>The created order details.</returns>
        /// <response code="200">If the order is successfully created.</response>
        /// <response code="400">If the order DTO is invalid.</response>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OrderDto orderDto)
        {
            var command = new Add.Command(orderDto);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves paginated orders with an optional filter.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="size">The number of items per page.</param>
        /// <param name="filter">An optional filter parameter to search orders.</param>
        /// <returns>A list of paginated orders.</returns>
        /// <response code="200">Returns the paginated list of orders.</response>
        [HttpGet("{page}/{size}/{filter?}")]
        public async Task<IActionResult> Get(int page, int size, string? filter = null)
        {
            var result = await _mediator.Send(new Get.Query(page, size, filter));
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all orders.
        /// </summary>
        /// <returns>A list of all orders.</returns>
        /// <response code="200">Returns the list of all orders.</response>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAll.Query());
            return Ok(result);
        }

        /// <summary>
        /// Retrieves orders for a specific customer.
        /// </summary>
        /// <param name="customerId">The ID of the customer whose orders are to be retrieved.</param>
        /// <returns>A list of orders for the specified customer.</returns>
        /// <response code="200">Returns the list of orders for the customer.</response>
        [HttpGet("GetCustomerOrders")]
        public async Task<IActionResult> GetCustomerOrders(int customerId)
        {
            var orders = await _mediator.Send(new GetCustomerOrdersByDate.Query(customerId));
            return Ok(orders);
        }

        /// <summary>
        /// Updates an existing order by ID.
        /// </summary>
        /// <param name="id">The ID of the order to update.</param>
        /// <param name="orderDto">The updated order data transfer object (DTO).</param>
        /// <returns>The updated order details.</returns>
        /// <response code="200">Returns the updated order details.</response>
        /// <response code="404">If the order with the given ID is not found.</response>
        [HttpPut("{id}/update")]
        public async Task<IActionResult> Update(int id, OrderDto orderDto)
        {
            var command = new Update.Command(id, orderDto);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Deletes an order by ID.
        /// </summary>
        /// <param name="id">The ID of the order to delete.</param>
        /// <returns>An action result indicating success or failure.</returns>
        /// <response code="200">If the order is successfully deleted.</response>
        /// <response code="404">If the order with the given ID is not found.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new Delete.Command(id));
            return Ok(result);
        }
    }
}
