using Microsoft.AspNetCore.Mvc;
using MediatR;
using CustomerOrderAPI.Application.Features.V1.Products;
using CustomerOrderAPI.Application.DTO.Product;
using System.Threading.Tasks;

namespace CustomerOrderAPI.Controllers.V1
{
    /// <summary>
    /// Controller responsible for handling product-related API requests.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductController"/> class.
        /// </summary>
        /// <param name="mediator">IMediator service for handling commands and queries.</param>
        public ProductController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="productDto">The product data transfer object (DTO) containing product details.</param>
        /// <returns>The created product details.</returns>
        /// <response code="200">If the product is successfully created.</response>
        /// <response code="400">If the product DTO is invalid.</response>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductDto productDto)
        {
            var command = new Add.Command(productDto);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves paginated products with an optional filter.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="size">The number of items per page.</param>
        /// <param name="filter">An optional filter parameter to search products.</param>
        /// <returns>A list of paginated products.</returns>
        /// <response code="200">Returns the paginated list of products.</response>
        [HttpGet("{page}/{size}/{filter?}")]
        public async Task<IActionResult> Get(int page, int size, string? filter = null)
        {
            var result = await _mediator.Send(new Get.Query(page, size, filter));
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all products.
        /// </summary>
        /// <returns>A list of all products.</returns>
        /// <response code="200">Returns the list of all products.</response>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAll.Query());
            return Ok(result);
        }

        /// <summary>
        /// Updates an existing product by ID.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="productDto">The updated product data transfer object (DTO).</param>
        /// <returns>The updated product details.</returns>
        /// <response code="200">Returns the updated product details.</response>
        /// <response code="404">If the product with the given ID is not found.</response>
        [HttpPut("{id}/update")]
        public async Task<IActionResult> Update(int id, ProductDto productDto)
        {
            var command = new Update.Command(id, productDto);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a product by ID.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <returns>An action result indicating success or failure.</returns>
        /// <response code="200">If the product is successfully deleted.</response>
        /// <response code="404">If the product with the given ID is not found.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new Delete.Command(id));
            return Ok(result);
        }
    }
}
