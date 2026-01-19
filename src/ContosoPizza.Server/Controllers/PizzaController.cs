using ContosoPizza.Models;
using ContosoPizza.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContosoPizza.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PizzaController : ControllerBase
{
    private readonly PizzaService _pizzaService;

    public PizzaController(PizzaService pizzaService)
    {
        _pizzaService = pizzaService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<Pizza>>> GetAll() =>
        await _pizzaService.GetAllAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Pizza>> Get(int id)
    {
        var pizza = await _pizzaService.GetAsync(id);

        if (pizza == null)
            return NotFound();

        return pizza;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Pizza pizza)
    {
        await _pizzaService.AddAsync(pizza);
        return CreatedAtAction(nameof(Get), new { id = pizza.Id }, pizza);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Pizza pizza)
    {
        if (id != pizza.Id)
            return BadRequest();

        var updated = await _pizzaService.UpdateAsync(pizza);
        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _pizzaService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}