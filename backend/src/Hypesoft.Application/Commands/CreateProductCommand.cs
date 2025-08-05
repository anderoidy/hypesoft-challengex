# Commands - Comandos CQRS que representam intenções de alteração de estado
using MediatR;

namespace Hypesoft.Application.Commands
{
    public class CreateProductCommand : IRequest<Guid>
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        // ...
    }
}
