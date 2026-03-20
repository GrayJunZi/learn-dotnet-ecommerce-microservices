using Catalog.Application.Commands;
using Catalog.Application.DTOs;
using Catalog.Application.Mappers;
using Catalog.Application.Queries;
using Catalog.Core.Specification;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public class CatalogController(IMediator mediator) : ControllerBase
{
    [HttpGet("GetAllProducts")]
    public async Task<IActionResult> GetProducts([FromQuery] CatalogSpecParams catalogSpecParams)
    {
        var query = new GetAllProductsQuery(catalogSpecParams);
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(string id)
    {
        var query = new GetProductByIdQuery(id);
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("productName/{productName}")]
    public async Task<IActionResult> GetProductsByProductName(string productName)
    {
        var query = new GetProductsByNameQuery(productName);
        var result = await mediator.Send(query);
        if (result is null || result.Count == 0)
            return NotFound();

        var products = result.Select(x => x.ToDto());
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var command = new DeleteProductByIdCommand(id);
        var result = await mediator.Send(command);
        if (!result)
            return NotFound();
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductDto updateProductDto)
    {
        var command = updateProductDto.ToCommand(id);
        var result = await mediator.Send(command);
        if (!result)
            return NotFound();
        return NoContent();
    }

    [HttpGet("GetAllBrands")]
    public async Task<IActionResult> GetAllBrands()
    {
        var query = new GetAllBrandsQuery();
        var result = await mediator.Send(query);
        return Ok(result);
    }
    
    [HttpGet("GetAllTypes")]
    public async Task<IActionResult> GetAllTypes()
    {
        var query = new GetAllTypesQuery();
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("/brand/{brand}", Name = "GetProductsByBrandName")]
    public async Task<IActionResult> GetProductsByBrandName(string brand)
    {
        var query = new GetAllProductsByBrandQuery(brand);
        var result = await mediator.Send(query);
        return Ok(result);
    }
}