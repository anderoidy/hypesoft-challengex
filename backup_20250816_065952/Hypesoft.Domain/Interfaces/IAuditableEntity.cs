using System;

namespace Hypesoft.Domain.Common.Interfaces;

/// <summary>
/// Interface que define propriedades para rastreamento de alterações em entidades.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Obtém ou define o identificador do usuário que criou a entidade.
    /// </summary>
    string? CreatedBy { get; set; }

    /// <summary>
    /// Obtém ou define a data e hora em que a entidade foi criada.
    /// </summary>
    DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Obtém ou define o identificador do último usuário que modificou a entidade.
    /// </summary>
    string? ModifiedBy { get; set; }

    /// <summary>
    /// Obtém ou define a data e hora da última modificação da entidade.
    /// </summary>
    DateTimeOffset? ModifiedAt { get; set; }
}
