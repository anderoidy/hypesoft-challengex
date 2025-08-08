using Ardalis.Result;
using MediatR;

namespace Hypesoft.Application.Commands;

public record DeleteProductCommand(int Id) : IRequest<Result>;