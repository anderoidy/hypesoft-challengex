using System;
using System.Collections.Generic;
using Hypesoft.Domain.Common.Interfaces;

namespace Hypesoft.Domain.Common
{
    /// <summary>
    /// Base class for all domain entities.
    /// </summary>
    public abstract class BaseEntity : IEntity<Guid>
    {
        /// <summary>
        /// Unique identifier (never Guid.Empty).
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Creation timestamp (UTC).
        /// </summary>
        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Creator user id/name.
        /// </summary>
        public string CreatedBy { get; private set; } = "system";

        /// <summary>
        /// Last modification timestamp (UTC).
        /// </summary>
        public DateTimeOffset? ModifiedAt { get; private set; }

        /// <summary>
        /// Last modifier user id/name.
        /// </summary>
        public string? ModifiedBy { get; private set; }

        /// <summary>
        /// Soft delete flag.
        /// </summary>
        public bool IsDeleted { get; private set; }

        /// <summary>
        /// Soft delete timestamp.
        /// </summary>
        public DateTimeOffset? DeletedAt { get; private set; }

        /// <summary>
        /// Soft delete user id/name.
        /// </summary>
        public string? DeletedBy { get; private set; }

        /// <summary>
        /// Active flag.
        /// </summary>
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// Concurrency token (if needed by provider).
        /// </summary>
        public byte[]? RowVersion { get; private set; }

        /// <summary>
        /// Domain events.
        /// </summary>
        // Campo de apoio
        private List<IDomainEvent>? _domainEvents;

        // Getter seguro e tipado (sem ?? entre tipos diferentes)
        public IReadOnlyCollection<IDomainEvent> DomainEvents
        {
            get
            {
                if (_domainEvents is null || _domainEvents.Count == 0)
                    return Array.Empty<IDomainEvent>(); // retorna IReadOnlyCollection<IDomainEvent>

                return _domainEvents.AsReadOnly(); // ReadOnlyCollection<IDomainEvent> implementa IReadOnlyCollection<IDomainEvent>
            }
        }

        #region Constructors
        protected BaseEntity() { }

        // Adicione dentro de BaseEntity
        protected void SetId(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id não pode ser Guid.Empty", nameof(id));

            Id = id;
        }

        protected BaseEntity(string createdBy)
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTimeOffset.UtcNow;
            CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "system" : createdBy;
            IsActive = true;
        }
        #endregion

        #region Domain Events Methods
        public void AddDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents ??= new List<IDomainEvent>();
            _domainEvents.Add(eventItem);
        }

        public void RemoveDomainEvent(IDomainEvent eventItem) => _domainEvents?.Remove(eventItem);

        public void ClearDomainEvents() => _domainEvents?.Clear();
        #endregion

        #region Activation & Deletion Methods
        public void Deactivate(string userId)
        {
            IsActive = false;
            UpdateAuditFields(userId);
        }

        public void Activate(string userId)
        {
            IsActive = true;
            UpdateAuditFields(userId);
        }

        public void SoftDelete(string userId)
        {
            IsDeleted = true;
            DeletedAt = DateTimeOffset.UtcNow;
            DeletedBy = string.IsNullOrWhiteSpace(userId) ? "system" : userId;
            UpdateAuditFields(userId);
        }
        #endregion

        #region Audit Methods
        public void UpdateAuditFields(string userId)
        {
            ModifiedAt = DateTimeOffset.UtcNow;
            ModifiedBy = string.IsNullOrWhiteSpace(userId) ? "system" : userId;
        }

        public void SetCreatedBy(string userId)
        {
            CreatedBy = string.IsNullOrWhiteSpace(userId) ? "system" : userId;

            // Se CreatedAt estiver default (em cenários de reidratação)
            if (CreatedAt == default)
                CreatedAt = DateTimeOffset.UtcNow;

            // Ajusta também a primeira modificação para o mesmo usuário
            ModifiedBy = CreatedBy;
        }
        #endregion
    }
}
