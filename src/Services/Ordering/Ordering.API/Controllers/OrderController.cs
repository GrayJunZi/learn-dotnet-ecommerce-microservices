using Microsoft.AspNetCore.Mvc;
using Ordering.Application.DTOs;
using Ordering.Application.Mappers;
using Ordering.Application.Orders.CreateOrder;
using Ordering.Application.Orders.DeleteOrder;
using Ordering.Application.Orders.GetOrders;
using Ordering.Application.Orders.UpdateOrder;

namespace Ordering.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class OrderController(
    CreateOrderHandler createOrderHandler,
    UpdateOrderHandler updateOrderHandler,
    DeleteOrderHandler deleteOrderHandler,
    GetOrderListHandler getOrderListHandler,
    ILogger<OrderController> logger) : ControllerBase
{
    [HttpGet("{userName}", Name = "GetOrdersByUserName")]
    public async Task<IActionResult> GetOrdersByUserName(string userName, CancellationToken cancellationToken)
    {
        var query = new GetOrderListQuery(userName);

        var orders = await getOrderListHandler.Handle(query, cancellationToken);

        logger.LogInformation("Order fetched for user {UserName}", userName);
        return Ok(orders);
    }

    [HttpPost(Name = "CheckoutOrder")]
    public async Task<IActionResult> CheckoutOrder([FromBody] CreateOrderDto createOrderDto,
        CancellationToken cancellationToken)
    {
        var command = createOrderDto.ToCommand();

        var orderId = await createOrderHandler.Handle(command, cancellationToken);

        logger.LogInformation("Order created with Id {OrderId}", orderId);
        return Ok(orderId);
    }

    [HttpPut(Name = "UpdateOrder")]
    public async Task<IActionResult> UpdateOrder([FromBody] OrderDto orderDto, CancellationToken cancellationToken)
    {
        var command = orderDto.ToCommand();

        await updateOrderHandler.Handle(command, cancellationToken);

        logger.LogInformation("Order updated with Id {OrderId}", orderDto.Id);
        return  Ok(orderDto.Id);
    }

    [HttpDelete("{id}", Name = "DeleteOrder")]
    public async Task<IActionResult> DeleteOrder(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteOrderCommand(id);

        await deleteOrderHandler.Handle(command, cancellationToken);
        
        logger.LogInformation("Order deleted with Id {Id}", id);
        return NoContent();
    }
}