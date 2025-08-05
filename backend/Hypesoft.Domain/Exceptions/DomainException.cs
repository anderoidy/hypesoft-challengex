using System;

namespace Hypesoft.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException() { }
    
    public DomainException(string message) : base(message) { }
    
    public DomainException(string message, Exception innerException) 
        : base(message, innerException) { }
}

public class DomainValidationException : DomainException
{
    public Dictionary<string, string[]> Errors { get; }
    
    public DomainValidationException(Dictionary<string, string[]> errors) 
        : base("Uma ou mais falhas de validação ocorreram.")
    {
        Errors = errors;
    }
}

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object key) 
        : base($"A entidade '{entityName}' com o identificador '{key}' não foi encontrada.") { }
}

public class EntityAlreadyExistsException : DomainException
{
    public EntityAlreadyExistsException(string entityName, object key) 
        : base($"Já existe uma entidade '{entityName}' com o identificador '{key}'.") { }
}

public class InvalidOperationDomainException : DomainException
{
    public InvalidOperationDomainException(string message) : base(message) { }
}
