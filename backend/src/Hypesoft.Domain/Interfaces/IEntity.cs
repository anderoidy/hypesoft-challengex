namespace Hypesoft.Domain.Common.Interfaces
{
    public interface IEntity<TId>
    {
        // Remova a exigência de set público
        // Apenas getter é exigido pela interface
        TId Id { get; }
    }
}
