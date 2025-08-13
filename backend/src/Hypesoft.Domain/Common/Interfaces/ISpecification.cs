using System.Linq.Expressions;

namespace Hypesoft.Domain.Common.Interfaces
{
    /// <summary>
    /// Specification pattern interface with projection support
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TResult">Result projection type</typeparam>
    public interface ISpecification<T, TResult>
    {
        Expression<Func<T, bool>>? Criteria { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        Expression<Func<T, object>>? OrderBy { get; }
        Expression<Func<T, object>>? OrderByDescending { get; }
        Expression<Func<T, TResult>>? Selector { get; } // Esta é a propriedade chave para projection
        bool IsPagingEnabled { get; }
        int Take { get; }
        int Skip { get; }
    }
}
