namespace OrderService.Api.DTOs;

public class CreateOrderDto
{
    public decimal Total { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
}
