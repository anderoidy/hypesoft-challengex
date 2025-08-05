[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    // Injete os handlers/serviços necessários

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        // Implemente busca por nome e listagem
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        // Implemente criação
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command)
    {
        // Implemente edição
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        // Implemente exclusão
    }
}