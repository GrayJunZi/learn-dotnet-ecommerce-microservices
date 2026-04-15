using Basket.Application.Commands;
using Basket.Application.DTOs;
using Basket.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Basket.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BasketController(IMediator mediator) : ControllerBase
{
    [HttpGet("{userName}")]
    public async Task<IActionResult> GetBasket(string userName)
    {
        var query = new GetBasketByUserNameQuery(userName);
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBasket([FromBody] CreateShoppingCartCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{userName}")]
    public async Task<IActionResult> DeleteBasket(string userName)
    {
        var command = new DeleteBasketByUserNameCommand(userName);
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> Checkout([FromBody] BasketCheckoutDto basketCheckoutDto)
    {
        var result = await mediator.Send(new BasketCheckoutCommand(basketCheckoutDto));
        return Accepted();
    }
}