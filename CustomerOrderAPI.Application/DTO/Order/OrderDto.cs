using CustomerOrderAPI.Application.DTO.Item;

namespace CustomerOrderAPI.Application.DTO.Order
{
    public class OrderDto
    {
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public ICollection<OrderItemDto> Items { get; set; }
    }
}
