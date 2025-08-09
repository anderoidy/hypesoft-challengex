using System;

namespace Hypesoft.Domain.Exceptions;

/// <summary>
/// Exception lançada quando ocorre uma violação das regras de negócio.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DomainException"/>
    /// </summary>
    public DomainException()
    { }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DomainException"/> com uma mensagem de erro específica.
    /// </summary>
    /// <param name="message">A mensagem que descreve o erro.</param>
    public DomainException(string message) : base(message)
    { }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="DomainException"/> com uma mensagem de erro específica
    /// e uma referência para a exceção interna que é a causa desta exceção.
    /// </summary>
    /// <param name="message">A mensagem de erro que explica o motivo da exceção.</param>
    /// <param name="innerException">A exceção que é a causa da exceção atual, ou uma referência nula se não for especificada.</param>
    public DomainException(string message, Exception innerException) : base(message, innerException)
    { }
}